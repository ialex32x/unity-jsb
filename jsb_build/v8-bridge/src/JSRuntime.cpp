#include "JSRuntime.h"
#include "JSApi.h"
#include <cassert>
#include <string>
#include <libplatform/libplatform.h>

struct GlobalInitializer
{
	std::unique_ptr<v8::Platform> platform = v8::platform::NewDefaultPlatform();

	GlobalInitializer()
	{
#if defined(JSB_DEBUG) && JSB_DEBUG
			std::string args = "--expose-gc";
			v8::V8::SetFlagsFromString(args.c_str(), static_cast<int>(args.size()));
#endif
		v8::V8::InitializePlatform(platform.get());
		v8::V8::Initialize();
	}
};

static GlobalInitializer _gInit;

static void* default_malloc(JSMallocState* state, size_t size)
{
	return malloc(size);
}

static void* default_realloc(JSMallocState* state, void* buf, size_t size)
{
	return realloc(buf, size);
}

static void default_free(JSMallocState* state, void* buf)
{
	free(buf);
}

static size_t default_malloc_usable_size(const void* buf)
{
	return 0;
}

static void _PromiseRejectCallback(v8::PromiseRejectMessage message)
{
	v8::Local<v8::Promise> promise = message.GetPromise();
	v8::Isolate* isolate = promise->GetIsolate();
	JSRuntime* rt = (JSRuntime*)isolate->GetData(JS_ISOLATE_DATA_SELF);

	if (rt->_promiseRejectionTracker)
	{
		v8::Isolate::Scope isolateScope(isolate);
		v8::HandleScope handleScope(isolate);

		v8::Local<v8::Value> value = message.GetValue();
		v8::PromiseRejectEvent rejectEvent = message.GetEvent();
		v8::Local<v8::Context> context = isolate->GetCurrentContext();
		JSContext* ctx = (JSContext*)context->GetAlignedPointerFromEmbedderData(JS_CONTEXT_DATA_SELF);
		JS_BOOL is_handled = rejectEvent == v8::kPromiseRejectWithNoHandler ? 0 : 1;

		JSValue promise_jsval = rt->AddObject(context, promise);
		JSValue reason_jsval = rt->AddValue(context, value);
		rt->_promiseRejectionTracker(ctx, promise_jsval, reason_jsval, is_handled, rt->_promiseRejectionOpaque);
		rt->FreeValue(promise_jsval);
		rt->FreeValue(reason_jsval);
	}
}

JSRuntime::JSRuntime(JSGCObjectFinalizer* finalizer)
: _arrayBufferAllocator(v8::ArrayBuffer::Allocator::NewDefaultAllocator())
{
	malloc_functions.js_malloc = default_malloc;
	malloc_functions.js_free = default_free;
	malloc_functions.js_realloc = default_realloc;
	malloc_functions.js_malloc_usable_size = default_malloc_usable_size;

	v8::Isolate::CreateParams create_params;
	create_params.array_buffer_allocator = _arrayBufferAllocator;
	_isolate = v8::Isolate::New(create_params);
	_isolate->SetData(JS_ISOLATE_DATA_SELF, this);
	_isolate->SetPromiseRejectCallback(_PromiseRejectCallback);
	_objectRefs.emplace_back(JSValueRef());
	_atomRefs.emplace_back(JSAtomRef());
	_gcObjects.emplace_back(nullptr);

	{
		v8::Isolate::Scope isolateScope(_isolate);
		v8::HandleScope handleScope(_isolate);

		_PrivateCacheIndexKey.Reset(_isolate, v8::Private::New(_isolate, v8::String::NewFromUtf8Literal(_isolate, "PrivateCacheIndex")));
		_isolate->SetMicrotasksPolicy(v8::MicrotasksPolicy::kExplicit);
#define DEF(name, str) \
		{ JSAtom atom = GetAtom(str, sizeof(str) - 1); assert(atom._value == (uint32_t)JS_ATOM_##name); }
		#include "quickjs-atom.h"
#undef DEF

		NewClass(JSB_GetBridgeClassID(), "CSharpClass", finalizer);
	}
}

JSRuntime::~JSRuntime() = default;

JS_BOOL JSRuntime::Release()
{
	_PrivateCacheIndexKey.Reset();
#define DEF(name, str) \
	FreeAtom((uint32_t)JS_ATOM_##name);
#include "quickjs-atom.h"
#undef DEF

	// cleanup gc objects
	{
		v8::Isolate::Scope isolateScope(_isolate);
		v8::HandleScope handleScope(_isolate);
		for (GCObject* gcObject : _gcObjects)
		{
			if (gcObject && !gcObject->_obj.IsEmpty())
			{
				v8::Local<v8::Object> obj = gcObject->_obj.Get(_isolate);
				if (obj->InternalFieldCount() == EIFN_FieldCount)
				{
					if (void* sv = obj->GetAlignedPointerFromInternalField(EIFN_Payload))
					{
						obj->SetAlignedPointerInInternalField(EIFN_Payload, nullptr);
						if (JSGCObjectFinalizer* finalizer = gcObject->_classDef->_finalizer)
						{
							JSPayloadHeader header = *static_cast<JSPayloadHeader*>(sv);
							finalizer(this, header);
						}
						mem_free(nullptr, sv);
					}
				}
				if (gcObject->_value_handle != 0)
				{
					JSValueRef& valueRef = _objectRefs[gcObject->_value_handle];
					valueRef._gc_handle = 0;
					if (valueRef._references == 0)
					{
						--_aliveObjectNum;
						valueRef._next = _freeObjectSlot;
						_freeObjectSlot = gcObject->_value_handle;
					}
				}
				gcObject->_obj.Reset();
				mem_free(nullptr, gcObject);
			}
		}
	}

	JS_BOOL res = _aliveAtomNum == 0 && _aliveObjectNum == 0 ? TRUE : FALSE;
	_atomRefs.clear();
	_objectRefs.clear();
	_atoms.clear();
	_gcObjects.clear();

	for (auto pair : _classes)
	{
		JSClassDef* def = pair.second;
		def->_class.Reset();
		mem_free(nullptr, def);
	}
	_classes.clear();
	_exception.Reset();
	_isolate->Dispose();
	delete _arrayBufferAllocator;
	//assert(res);
	return res;
}

void JSRuntime::ComputeMemoryUsage(JSMemoryUsage* memoryUsage)
{
	memoryUsage->obj_count = _aliveObjectNum;
	memoryUsage->atom_count = _aliveAtomNum;
}

size_t JSRuntime::_AddValueInternal(v8::Local<v8::Context> context, v8::Local<v8::Value> val, size_t gc_handle)
{
	v8::Local<v8::Object> obj;

	if (val->IsObject())
	{
		obj = v8::Local<v8::Object>::Cast(val);
		v8::MaybeLocal<v8::Value> maybe_cacheIndex = obj->GetPrivate(context, _PrivateCacheIndexKey.Get(_isolate));
		v8::Local<v8::Value> cacheIndex;
		if (maybe_cacheIndex.ToLocal(&cacheIndex) && cacheIndex->IsInt32())
		{
			size_t last_id = static_cast<size_t>(cacheIndex->ToInt32(context).ToLocalChecked()->Value());
			JSValueRef& valueRef = _objectRefs[last_id];
			if (valueRef._next == 0 && valueRef._references > 0 && valueRef._value == val)
			{
				++valueRef._references;
				return last_id;
			}

			// resurrect
			if (gc_handle != 0 && valueRef._gc_handle == gc_handle && _gcObjects[valueRef._gc_handle]->_obj == val)
			{
				++valueRef._references;
				//printf("resurrect: JSValue is alive again %zd: %d (should be 1)", last_id, valueRef._references);
				return last_id;
			}
		}
	}

	size_t id;
	++_aliveObjectNum;
	if (_freeObjectSlot == 0)
	{
		JSValueRef valueRef(_isolate, val);
		id = _objectRefs.size();
		valueRef._gc_handle = gc_handle;
		_objectRefs.emplace_back(valueRef);
	}
	else
	{
		id = _freeObjectSlot;
		JSValueRef& valueRef = _objectRefs[id];
		_freeObjectSlot = valueRef._next;
		valueRef._next = 0;
		valueRef._value.Reset(_isolate, val);
		valueRef._references = 1;
		valueRef._gc_handle = gc_handle;
	}

	if (val->IsObject())
	{
		obj->SetPrivate(context, _PrivateCacheIndexKey.Get(_isolate), v8::Int32::New(_isolate, static_cast<int32_t>(id)));
	}
	return id;
}

void* JSRuntime::mem_alloc(JSMallocState* s, size_t size)
{
	return malloc_functions.js_malloc(s, size);
}

void JSRuntime::mem_free(JSMallocState* s, void* ptr)
{
	malloc_functions.js_free(s, ptr);
}

void* JSRuntime::mem_realloc(JSMallocState* s, void* ptr, size_t size)
{
	return malloc_functions.js_realloc(s, ptr, size);
}

size_t JSRuntime::mem_malloc_usable_size(const void* ptr)
{
	return malloc_functions.js_malloc_usable_size(ptr);
}

void JSRuntime::OnGarbadgeCollectCallback(const v8::WeakCallbackInfo<GCObject>& info)
{
	GCObject* gcObject = info.GetParameter();
	JSRuntime* rt = gcObject->_runtime;
	JSGCObjectFinalizer* finalizer = gcObject->_classDef->_finalizer;
	gcObject->_classDef = nullptr;
	gcObject->_next = rt->_freeGCObjectSlot;
	gcObject->_obj.Reset();

	size_t value_handle = gcObject->_value_handle;
	if (value_handle != 0)
	{
		JSValueRef& valueRef = rt->_objectRefs[value_handle];
		valueRef._gc_handle = 0;
		--rt->_aliveObjectNum;
		valueRef._next = rt->_freeObjectSlot;
		rt->_freeObjectSlot = value_handle;
		gcObject->_value_handle = 0;
		//printf("resurrect: gc value removed %zd\n", value_handle);
	}
	rt->_freeGCObjectSlot = gcObject->_index;
	--rt->_aliveGCObjectNum;

	if (void* payload = info.GetInternalField(EIFN_Payload))
	{
		if (finalizer)
		{
			JSPayloadHeader header = *static_cast<JSPayloadHeader*>(payload);
			finalizer(rt, header);
		}
		rt->mem_free(nullptr, payload);
	}
}

JSValue JSRuntime::AddGCObject(v8::Local<v8::Context> context, v8::Local<v8::Object> obj, JSClassDef* def)
{
	GCObject* gcObject = nullptr;
	if (_freeGCObjectSlot == 0)
	{
		size_t index = _gcObjects.size();
		gcObject = static_cast<GCObject*>(mem_alloc(nullptr, sizeof(GCObject)));
		memset(gcObject, 0, sizeof(GCObject));
		gcObject->_index = index;
		gcObject->_runtime = this;

		_gcObjects.push_back(gcObject);
	}
	else
	{
		gcObject = _gcObjects[_freeGCObjectSlot];
		_freeGCObjectSlot = gcObject->_next;
		gcObject->_next = 0;
	}

	++_aliveGCObjectNum;
	gcObject->_classDef = def;
	gcObject->_next = 0;
	gcObject->_obj.Reset(_isolate, obj);
	gcObject->_obj.SetWeak<GCObject>(gcObject, OnGarbadgeCollectCallback, v8::WeakCallbackType::kInternalFields);
	JSValue js_value = JS_MKPTR(JS_TAG_OBJECT, _AddValueInternal(context, obj, gcObject->_index));
	gcObject->_value_handle = js_value.u.ptr;
	return js_value;
}

JSValue JSRuntime::AddValue(v8::Local<v8::Context> context, v8::Local<v8::Value> val)
{
	if (val->IsInt32())
	{
		return JS_MKINT32(JS_TAG_INT, v8::Local<v8::Int32>::Cast(val)->Value());
	}
	if (val->IsBoolean())
	{
		return JS_MKINT32(JS_TAG_BOOL, v8::Local<v8::Boolean>::Cast(val)->Value());
	}
	if (val->IsNull())
	{
		return JS_NULL;
	}
	if (val->IsUndefined())
	{
		return JS_UNDEFINED;
	}
	if (val->IsString())
	{
		return AddString(context, v8::Local<v8::String>::Cast(val));
	}
	if (val->IsSymbol())
	{
		return AddSymbol(context, v8::Local<v8::Symbol>::Cast(val));
	}
	if (val->IsBigInt())
	{
		return JS_MKPTR(JS_TAG_BIG_INT, _AddValueInternal(context, val));
	}
	return JS_MKPTR(JS_TAG_OBJECT, _AddValueInternal(context, val));
}

JSValue JSRuntime::AddString(v8::Local<v8::Context> context, v8::Local<v8::String> val)
{
	return JS_MKPTR(JS_TAG_STRING, _AddValueInternal(context, val));
}

JSValue JSRuntime::AddObject(v8::Local<v8::Context> context, v8::Local<v8::Object> val)
{
	return JS_MKPTR(JS_TAG_OBJECT, _AddValueInternal(context, val));
}

JSValue JSRuntime::AddSymbol(v8::Local<v8::Context> context, v8::Local<v8::Symbol> val)
{
	return JS_MKPTR(JS_TAG_SYMBOL, _AddValueInternal(context, val));
}

JSValue JSRuntime::ThrowException(v8::Local<v8::Context> context, v8::Local<v8::Value> exception)
{
	//v8::String::Utf8Value str(_isolate, exception);
	//printf("error: %s\n", *str);
	//if (stack.IsEmpty())
	//{
	//	v8::String::Utf8Value stack_str(_isolate, stack.ToLocalChecked());
	//	printf("stack: %s\n", *stack_str);
	//}
	_exception.Reset(_isolate, exception);
	return JS_EXCEPTION;
}

JSValue JSRuntime::ThrowError(v8::Local<v8::Context> context, const char* exception, int len)
{
	v8::MaybeLocal<v8::String> maybe_string = v8::String::NewFromUtf8(_isolate, exception, v8::NewStringType::kNormal, len);
	v8::Local<v8::String> string_;
	if (maybe_string.ToLocal(&string_))
	{
		_exception.Reset(_isolate, _isolate->ThrowError(string_));
	}
	else
	{
		_exception.Reset(_isolate, _isolate->ThrowError("failed to new error string"));
	}
	return JS_EXCEPTION;
}

JSValue JSRuntime::ThrowTypeError(v8::Local<v8::Context> context, const char* exception, int len)
{
	v8::MaybeLocal<v8::String> maybe_string = v8::String::NewFromUtf8(_isolate, exception, v8::NewStringType::kNormal, len);
	v8::Local<v8::String> string_;
	if (maybe_string.ToLocal(&string_))
	{
		_exception.Reset(_isolate, _isolate->ThrowException(v8::Exception::TypeError(string_)));
	}
	else
	{
		_exception.Reset(_isolate, _isolate->ThrowError("failed to new error string"));
	}
	return JS_EXCEPTION;
}

JSValue JSRuntime::DupValue(JSValue value)
{
	if (JS_TAG_IS_BYREF(value.tag))
	{
		size_t id = value.u.ptr;
		if (id > 0 && id < _objectRefs.size())
		{
			JSValueRef& valueRef = _objectRefs[id];
			if (valueRef._gc_handle != 0 && valueRef._references == 0)
			{
				v8::Isolate::Scope isolateScope(_isolate);
				v8::HandleScope handleScope(_isolate);

				GCObject* gcObject = _gcObjects[valueRef._gc_handle];
				valueRef._value.Reset(_isolate, gcObject->_obj.Get(_isolate));
				//printf("resurrect: JSValue is alive again %zd\n", id);
			}
			++valueRef._references;
		}
	}
	return value;
}

void JSRuntime::FreeValue(JSValue value)
{
	if (JS_TAG_IS_BYREF(value.tag))
	{
		size_t id = value.u.ptr;
		if (id > 0 && id < _objectRefs.size())
		{
			JSValueRef& valueRef = _objectRefs[id];
			assert(valueRef._references > 0);
			if (--valueRef._references == 0)
			{
				valueRef._value.Reset();
				if (valueRef._gc_handle == 0)
				{
					--_aliveObjectNum;
					valueRef._next = _freeObjectSlot;
					_freeObjectSlot = id;
				}
			}
		}
	}
}

JSPayloadHeader JSRuntime::FreePayload(JSValue value)
{
	if (value.tag == JS_TAG_OBJECT)
	{
		v8::MaybeLocal<v8::Value> maybe_local = GetReferencedValue(value.u.ptr);
		v8::Local<v8::Value> local;
		if (maybe_local.ToLocal(&local) && local->IsObject())
		{
			v8::Local<v8::Object> obj = v8::Local<v8::Object>::Cast(local);
			if (obj->InternalFieldCount() == EIFN_FieldCount)
			{
				if (void* sv = obj->GetAlignedPointerFromInternalField(EIFN_Payload))
				{
					obj->SetAlignedPointerInInternalField(EIFN_Payload, nullptr);
					JSPayloadHeader header = *static_cast<JSPayloadHeader*>(sv);
					mem_free(nullptr, sv);
					return header;
				}
			}
		}
	}
	return EmptyPayload;
}


v8::MaybeLocal<v8::Value> JSRuntime::GetValue(JSValue val)
{
	switch (val.tag)
	{
	case JS_TAG_OBJECT:
	case JS_TAG_STRING:
	case JS_TAG_SYMBOL:	return GetReferencedValue(val.u.ptr);
	case JS_TAG_INT: return v8::Int32::New(_isolate, val.u.int32);
	case JS_TAG_BOOL: return v8::Boolean::New(_isolate, val.u.int32 == 1);
	case JS_TAG_NULL: return v8::Null(_isolate);
	case JS_TAG_UNDEFINED: return v8::Undefined(_isolate);
	case JS_TAG_EXCEPTION: return _exception.Get(_isolate);
	case JS_TAG_FLOAT64: return v8::Number::New(_isolate, val.u.float64);
	default: return {};
	}
}

v8::MaybeLocal<v8::Value> JSRuntime::GetReferencedValue(size_t id)
{
	if (id > 0 && id < _objectRefs.size())
	{
		JSValueRef& valueRef = _objectRefs[id];
		if (valueRef._references != 0)
		{
			return valueRef._value.Get(_isolate);
		}
		if (valueRef._gc_handle != 0)
		{
			GCObject* gcObject = _gcObjects[valueRef._gc_handle];
			return gcObject->_obj.Get(_isolate);
		}
	}
	return {};
}

JSAtom JSRuntime::GetAtom(const char* val, size_t len)
{
	std::string str(val, len);
	return GetAtom(str);
}

JSAtom JSRuntime::GetAtom(std::string& val)
{
	std::map<std::string, JSAtom>::iterator it = _atoms.find(val);
	if (it != _atoms.end())
	{
		uint32_t id = it->second;
		JSAtomRef& valueRef =_atomRefs[id];
		++valueRef._references;
		return id;
	}
	int len = (int)val.length();
	v8::MaybeLocal<v8::String> str = v8::String::NewFromUtf8(_isolate, val.c_str(), v8::NewStringType::kInternalized, len);
	if (str.IsEmpty())
	{
		return {};
	}

	JSAtom atom;
	_aliveAtomNum++;
	if (_freeAtomSlot == 0)
	{
		JSAtomRef atomRef(_isolate, str.ToLocalChecked());
		atomRef._references = 1;
		atom = static_cast<uint32_t>(_atomRefs.size());
		_atomRefs.emplace_back(atomRef);
	}
	else
	{
		atom = _freeAtomSlot;
		JSAtomRef& valueRef = _atomRefs[atom];
		_freeAtomSlot = valueRef._next;
		valueRef._next = 0;
		valueRef._value.Reset(_isolate, str.ToLocalChecked());
		valueRef._references = 1;
	}
	_atoms[val] = atom;
	return atom;
}

v8::MaybeLocal<v8::String> JSRuntime::GetAtomValue(JSAtom atom)
{
	if (atom > 0 && atom < _atomRefs.size())
	{
		JSAtomRef& valueRef = _atomRefs[atom];
		return valueRef._value.Get(_isolate);
	}
	return {};
}

JSAtom JSRuntime::DupAtom(JSAtom atom)
{
	uint32_t id = atom;
	if (id > 0 && id < _atomRefs.size())
	{
		JSAtomRef& valueRef = _atomRefs[id];
		++valueRef._references;
	}
	return atom;
}

void JSRuntime::FreeAtom(JSAtom atom)
{
	uint32_t id = atom;
	if (id > 0 && id < _atomRefs.size())
	{
		JSAtomRef& atomRef = _atomRefs[id];
		assert(valueRef._references > 0);
		if (--atomRef._references == 0)
		{
			v8::Isolate::Scope isolateScope(_isolate);
			v8::HandleScope handleScope(_isolate);

			atomRef._next = _freeAtomSlot;
			_freeAtomSlot = id;
			v8::Local<v8::String> str = atomRef._value.Get(_isolate);
			atomRef._value.Reset();
			--_aliveAtomNum;
			v8::String::Utf8Value str_str(_isolate, str);
			std::string c_str(*str_str, str_str.length());
			_atoms.erase(c_str);
		}
	}
}

JSClassID JSRuntime::NewClass(JSClassID class_id, const char* class_name, JSGCObjectFinalizer* finalizer)
{
	if (_classes.contains(class_id))
	{
		return 0;
	}

	v8::MaybeLocal<v8::String> className = v8::String::NewFromUtf8(_isolate, class_name, v8::NewStringType::kInternalized);
	if (className.IsEmpty())
	{
		return 0;
	}
	JSClassDef* def = static_cast<JSClassDef*>(mem_alloc(nullptr, sizeof(JSClassDef)));

	def->_classID = class_id;
	def->_finalizer = finalizer;
	memset(&(def->_class), 0, sizeof(v8::Global<v8::FunctionTemplate>));
	v8::Local<v8::FunctionTemplate> func_template = v8::FunctionTemplate::New(_isolate);
	func_template->InstanceTemplate()->SetInternalFieldCount(EIFN_FieldCount);
	func_template->SetClassName(className.ToLocalChecked());
	def->_class.Reset(_isolate, func_template);
	_classes[class_id] = def;
	return class_id;
}

JSClassDef* JSRuntime::GetClassDef(JSClassID class_id)
{
	std::map<JSClassID, JSClassDef*>::iterator it = _classes.find(class_id);
	if (it != _classes.end())
	{
		return it->second;
	}
	return nullptr;
}

void JSRuntime::SetHostPromiseRejectionTracker(JSHostPromiseRejectionTracker* cb, void* opaque)
{
	_promiseRejectionTracker = cb;
	_promiseRejectionOpaque = opaque;
}

int JSRuntime::ExecutePendingJob(JSContext** pctx)
{
	_isolate->PerformMicrotaskCheckpoint();
	*pctx = nullptr;
	return 0;
}

void JSRuntime::RunGC()
{
#if defined(JSB_DEBUG) && JSB_DEBUG
	_isolate->RequestGarbageCollectionForTesting(v8::Isolate::kFullGarbageCollection);
#else
	_isolate->LowMemoryNotification();
#endif
}
