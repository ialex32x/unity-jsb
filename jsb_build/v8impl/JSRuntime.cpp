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

JSRuntime::JSRuntime()
{
	malloc_functions.js_malloc = default_malloc;
	malloc_functions.js_free = default_free;
	malloc_functions.js_realloc = default_realloc;
	_arrayBufferAllocator = v8::ArrayBuffer::Allocator::NewDefaultAllocator();
	v8::Isolate::CreateParams create_params;
	create_params.array_buffer_allocator = _arrayBufferAllocator;
	_isolate = v8::Isolate::New(create_params);
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
	}
}

JSRuntime::~JSRuntime()
{

}

JS_BOOL JSRuntime::Release()
{
	_PrivateCacheIndexKey.Reset();
#define DEF(name, str) \
	FreeAtom(JSAtom{ ._value = (uint32_t)JS_ATOM_##name });
#include "quickjs-atom.h"
#undef DEF

	JS_BOOL res = _atomRefs.size() == 0 && _objectRefs.size() == 0 ? 1 : 0;
	_atomRefs.clear();
	_objectRefs.clear();
	for (auto gcObject : _gcObjects)
	{
		if (gcObject)
		{
			gcObject->_obj.Reset();
			malloc_functions.js_free(nullptr, gcObject);
		}
	}
	_gcObjects.clear();
	for (auto pair : _classes)
	{
		JSClassDef* def = pair.second;
		def->_class.Reset();
		malloc_functions.js_free(nullptr, def);
	}
	_classes.clear();
	_isolate->Dispose();
	delete _arrayBufferAllocator;
	//assert(res);
	return res;
}

size_t JSRuntime::_AddValueInternal(v8::Local<v8::Context> context, v8::Local<v8::Value> val)
{
	v8::Local<v8::Object> obj;

	if (val->IsObject())
	{
		obj = v8::Local<v8::Object>::Cast(val);
		v8::MaybeLocal<v8::Value> maybe_cacheIndex = obj->GetPrivate(context, _PrivateCacheIndexKey.Get(_isolate));
		v8::Local<v8::Value> cacheIndex;
		if (maybe_cacheIndex.ToLocal(&cacheIndex) && cacheIndex->IsInt32())
		{
			return (size_t)cacheIndex->ToInt32(context).ToLocalChecked()->Value();
		}
	}

	size_t id;
	++_usedObjectSlot;
	if (_freeObjectSlot == 0)
	{
		JSValueRef valueRef(_isolate, val);
		id = _objectRefs.size();
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
	}

	if (val->IsObject())
	{
		obj->SetPrivate(context, _PrivateCacheIndexKey.Get(_isolate), v8::Int32::New(_isolate, (int32_t)id));
	}
	return id;
}

JSValue JSRuntime::AddExceptionValue(v8::Local<v8::Context> context, v8::Local<v8::Value> val)
{
	JSValue value;
	value.tag = JS_TAG_EXCEPTION;
	value.u.ptr = _AddValueInternal(context, val);
	return value;
}

void JSRuntime::OnGarbadgeCollectCallback(const v8::WeakCallbackInfo<GCObject>& info)
{
	GCObject* gcObject = info.GetParameter();
	JSRuntime* rt = gcObject->_runtime;
	JSGCObjectFinalizer* finalizer = gcObject->_classDef->_finalizer;
	gcObject->_classDef = nullptr;
	gcObject->_next = rt->_freeGCObjectSlot;
	gcObject->_obj.Reset();
	rt->_freeGCObjectSlot = gcObject->_index;

	int32_t type_id = (int32_t)info.GetInternalField(0);
	int32_t value = (int32_t)info.GetInternalField(1);
	void* data = info.GetInternalField(2);

	finalizer(rt, JSPayloadHeader{ .type_id = type_id, .value = value });
	rt->malloc_functions.js_free(nullptr, data);
}

JSValue JSRuntime::AddGCValue(v8::Local<v8::Object> obj, JSClassDef* def)
{
	GCObject* gcObject = nullptr;
	if (_freeGCObjectSlot == 0)
	{
		size_t index = _gcObjects.size();
		gcObject = (GCObject*)malloc_functions.js_malloc(nullptr, sizeof(GCObject));
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

	gcObject->_classDef = def;
	gcObject->_next = 0;
	gcObject->_obj.Reset(_isolate, obj);
	gcObject->_obj.SetWeak<GCObject>(gcObject, OnGarbadgeCollectCallback, v8::WeakCallbackType::kFinalizer);
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
	if (val->IsObject())
	{
		return AddObject(context, v8::Local<v8::Object>::Cast(val));
	}
	//TODO throw exception?
	return JS_UNDEFINED;
}

JSValue JSRuntime::AddString(v8::Local<v8::Context> context, v8::Local<v8::String> val)
{
	return JS_MKPTR(JS_TAG_STRING, _AddValueInternal(context, val));
}

JSValue JSRuntime::AddSymbol(v8::Local<v8::Context> context, v8::Local<v8::Symbol> val)
{
	return JS_MKPTR(JS_TAG_SYMBOL, _AddValueInternal(context, val));
}

JSValue JSRuntime::AddObject(v8::Local<v8::Context> context, v8::Local<v8::Object> val)
{
	JSValue value;
	if (val->IsObject())
	{
		value.tag = JS_TAG_OBJECT;
	}
	else if (val->IsBigInt())
	{
		value.tag = JS_TAG_BIG_INT;
	}
	else
	{
		value.tag = JS_TAG_UNINITIALIZED;
		return value;
	}

	value.u.ptr = _AddValueInternal(context, val);
	return value;
}

JSValue JSRuntime::ThrowException(v8::Local<v8::Context> context, v8::Local<v8::Value> exception)
{
	//TODO setup exception value
	return JS_UNDEFINED;
}

JSValue JSRuntime::ThrowException(v8::Local<v8::Context> context, const char* exception)
{
	//TODO setup exception value
	return JS_UNDEFINED;
}

JSValue JSRuntime::DupValue(JSValue value)
{
	if (JS_TAG_IS_BYREF(value.tag))
	{
		size_t id = value.u.ptr;
		if (id > 0 && id < _objectRefs.size())
		{
			JSValueRef& valueRef = _objectRefs[id];
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
			if (--valueRef._references == 0)
			{
				valueRef._next = _freeObjectSlot;
				_freeObjectSlot = id;
				valueRef._value.Reset();
				--_usedObjectSlot;
			}
		}
	}
}

v8::MaybeLocal<v8::Value> JSRuntime::GetValue(JSValue val)
{
	switch (val.tag)
	{
	case JS_TAG_INT:
		return v8::Int32::New(_isolate, val.u.int32);
	case JS_TAG_FLOAT64:
		return v8::Number::New(_isolate, val.u.float64);
	case JS_TAG_OBJECT:
	case JS_TAG_STRING:
	case JS_TAG_SYMBOL:
		return GetValue(val.u.ptr);
	default:
		return {};
	}
}

v8::MaybeLocal<v8::Value> JSRuntime::GetValue(size_t id)
{
	if (id > 0 && id < _objectRefs.size())
	{
		JSValueRef& valueRef = _objectRefs[id];
		return valueRef._value.Get(_isolate);
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
		return it->second;
	}
	int len = (int)val.length();
	v8::MaybeLocal<v8::String> str = v8::String::NewFromUtf8(_isolate, val.c_str(), v8::NewStringType::kInternalized, len);
	if (str.IsEmpty())
	{
		return { 0 };
	}

	JSAtom atom;
	if (_freeAtomSlot == 0)
	{
		JSAtomRef atomRef(_isolate, str.ToLocalChecked());
		atom._value = (uint32_t)_atomRefs.size();
		_atomRefs.emplace_back(atomRef);
	}
	else
	{
		uint32_t id = _freeAtomSlot;
		JSAtomRef& valueRef = _atomRefs[id];
		_freeAtomSlot = valueRef._next;
		valueRef._next = 0;
		valueRef._value.Reset(_isolate, str.ToLocalChecked());
		valueRef._references = 1;
		atom._value = id;
	}
	_atoms[val] = atom;
	return atom;
}

v8::MaybeLocal<v8::String> JSRuntime::GetAtomValue(JSAtom atom)
{
	uint32_t id = atom._value;
	if (id > 0 && id < _atomRefs.size())
	{
		JSAtomRef& valueRef = _atomRefs[id];
		return valueRef._value.Get(_isolate);
	}
	return {};
}

v8::MaybeLocal<v8::String> JSRuntime::GetAtomValue(uint32_t atom_id)
{
	if (atom_id > 0 && atom_id < _atomRefs.size())
	{
		JSAtomRef& valueRef = _atomRefs[atom_id];
		return valueRef._value.Get(_isolate);
	}
	return {};
}

JSAtom JSRuntime::DupAtom(JSAtom atom)
{
	uint32_t id = atom._value;
	if (id > 0 && id < _atomRefs.size())
	{
		JSAtomRef& valueRef = _atomRefs[id];
		++valueRef._references;
	}
	return atom;
}

void JSRuntime::FreeAtom(JSAtom atom)
{
	uint32_t id = atom._value;
	if (id > 0 && id < _atomRefs.size())
	{
		JSAtomRef& valueRef = _atomRefs[id];
		if (--valueRef._references == 0)
		{
			v8::Isolate::Scope isolateScope(_isolate);
			v8::HandleScope handleScope(_isolate);

			valueRef._next = _freeAtomSlot;
			_freeAtomSlot = id;
			v8::Local<v8::String> str = valueRef._value.Get(_isolate);
			valueRef._value.Reset();
			--_usedObjectSlot;
			v8::String::Utf8Value str_str(_isolate, str);
			std::string cstr(*str_str, str_str.length());
			_atoms.erase(cstr);
		}
	}
}

JSClassID JSRuntime::NewClass(JSClassID class_id, const char* class_name, JSGCObjectFinalizer* finalizer)
{
	if (_classes.find(class_id) != _classes.end())
	{
		return 0;
	}

	v8::MaybeLocal<v8::String> className = v8::String::NewFromUtf8(_isolate, class_name, v8::NewStringType::kInternalized);
	if (className.IsEmpty())
	{
		return 0;
	}
	JSClassDef* def = (JSClassDef*)malloc_functions.js_malloc(nullptr, sizeof(JSClassDef));
	def->_classID = class_id;
	def->_finalizer = finalizer;
	v8::Local<v8::FunctionTemplate> func_template = v8::FunctionTemplate::New(_isolate);
	func_template->InstanceTemplate()->SetInternalFieldCount(3); // = JSPayload { type_id, value, data }
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
