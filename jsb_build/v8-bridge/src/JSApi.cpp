
#include "JSApi.h"
#include "JSRuntime.h"
#include "JSContext.h"

#include <v8.h>
#include <cassert>

#define SO_JSB_VERSION 0xa

#ifdef __cplusplus
extern "C" {
#endif

#define DEF(name, str) \
JSAtom JSB_ATOM_##name() { return (uint32_t)JS_ATOM_##name; }
#include "quickjs-atom.h"
#undef DEF

#define byte unsigned char
#define JS_FreeValue JSB_FreeValue

static JSClassID js_bridge_class_id = 0;

#define UNITY_EXT_COMPILING
#include "unity_ext.c"
#undef UNITY_EXT_COMPILING

void* js_malloc(JSContext* ctx, size_t size)
{
	return ctx->_runtime->malloc_functions.js_malloc(nullptr, size);
}

void* js_mallocz(JSContext* ctx, size_t size)
{
	void* ptr = ctx->_runtime->malloc_functions.js_malloc(nullptr, size);
	return memset(ptr, 0, size);
}

void js_free(JSContext* ctx, void* ptr)
{
	ctx->_runtime->malloc_functions.js_free(nullptr, ptr);
}

IntPtr js_strndup(JSContext* ctx, const char* s, size_t n)
{
	if (s && n > 0)
	{
		return memcpy(ctx->_runtime->malloc_functions.js_malloc(0, n), s, n);
	}
	return 0;
}

void JS_AddIntrinsicOperators(JSContext* ctx)
{
}

void JS_RunGC(JSRuntime* rt)
{
	rt->RunGC();
}

int JSB_Init()
{
	if (js_bridge_class_id == 0)
	{
		js_bridge_class_id = 1;
	}
	return SO_JSB_VERSION;
}

JSClassID JSB_GetBridgeClassID()
{
	return js_bridge_class_id;
}

static JSValue JS_ThrowError(JSContext* ctx, const char* msg)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->_runtime->ThrowError(ctx->Get(), msg);
}

JSRuntime* JSB_NewRuntime(JSGCObjectFinalizer* finalizer)
{
	JSRuntime* rt = new JSRuntime(finalizer);
	return rt;
}

int JSB_FreeRuntime(JSRuntime* rt)
{
	JS_BOOL res = rt->Release();
	delete rt;
	return res;
}

void* JSB_GetRuntimeOpaque(JSRuntime* rt)
{
	return rt->_opaque;
}

void JSB_SetRuntimeOpaque(JSRuntime* rt, void* opaque)
{
	rt->_opaque = opaque;
}

void* JS_GetContextOpaque(JSContext* ctx)
{
	return ctx->_opaque;
}

void JS_SetContextOpaque(JSContext* ctx, void* opaque)
{
	ctx->_opaque = opaque;
}

JSContext* JS_NewContext(JSRuntime* rt)
{
	v8::Isolate::Scope isolateScope(rt->_isolate);
	v8::HandleScope handleScope(rt->_isolate);

	JSContext* ctx = new JSContext(rt);
	JSValue globalObject = JS_GetGlobalObject(ctx);
	JS_SetPropertyInternal(ctx, globalObject, JSB_ATOM_global(), globalObject, 0);
	return ctx;
}

void JS_FreeContext(JSContext* ctx)
{
	delete ctx;
}

JSRuntime* JS_GetRuntime(JSContext* ctx)
{
	return ctx->_runtime;
}

JSValue JSB_DupValue(JSContext* ctx, JSValue val)
{
	return ctx->_runtime->DupValue(val);
}

void JSB_FreeValue(JSContext* ctx, JSValue val)
{
	ctx->_runtime->FreeValue(val);
}

void JSB_FreeValueRT(JSRuntime* rt, JSValue val)
{
	rt->FreeValue(val);
}

JSAtom JS_NewAtomLen(JSContext* ctx, const char* str, size_t len)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->_runtime->GetAtom(str, len);
}

JSAtom JS_DupAtom(JSContext* ctx, JSAtom v)
{
	return ctx->_runtime->DupAtom(v);
}

void JS_FreeAtom(JSContext* ctx, JSAtom v)
{
	ctx->_runtime->FreeAtom(v);
}

JSValue JS_AtomToString(JSContext* ctx, JSAtom atom)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->GetAtomValue(atom);
}

int JS_ToBool(JSContext* ctx, JSValueConst val)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	v8::MaybeLocal<v8::Value> maybe = ctx->_runtime->GetValue(val);
	v8::Local<v8::Value> value;
	if (maybe.ToLocal(&value))
	{
		return value->ToBoolean(ctx->GetIsolate())->Value() ? TRUE : FALSE;
	}
	return FALSE;
}

int JS_ToInt32(JSContext* ctx, int* pres, JSValue val)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	v8::MaybeLocal<v8::Value> maybe = ctx->_runtime->GetValue(val);
	v8::Local<v8::Value> value;
	if (maybe.ToLocal(&value))
	{
		v8::MaybeLocal<v8::Int32> maybe_int32 = value->ToInt32(ctx->Get());
		v8::Local<v8::Int32> int32;
		if (maybe_int32.ToLocal(&int32))
		{
			*pres = int32->Value();
			return 0;
		}
	}
	*pres = 0;
	return -1;
}

int JS_ToInt64(JSContext* ctx, int64_t* pres, JSValue val)
{
	return JS_ToBigInt64(ctx, pres, val);
}

int JS_ToBigInt64(JSContext* ctx, int64_t* pres, JSValue val)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	v8::MaybeLocal<v8::Value> maybe = ctx->_runtime->GetValue(val);
	v8::Local<v8::Value> value;
	if (maybe.ToLocal(&value))
	{
		v8::MaybeLocal<v8::BigInt> maybe_bigint = value->ToBigInt(ctx->Get());
		v8::Local<v8::BigInt> bigint;
		if (maybe_bigint.ToLocal(&bigint))
		{
			*pres = bigint->Int64Value();
			return 0;
		}
	}
	*pres = 0;
	return -1;
}

int JS_ToIndex(JSContext* ctx, uint64_t* plen, JSValueConst val)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	v8::MaybeLocal<v8::Value> maybe = ctx->_runtime->GetValue(val);
	v8::Local<v8::Value> value;
	if (maybe.ToLocal(&value))
	{
		v8::MaybeLocal<v8::BigInt> maybe_bigint = value->ToBigInt(ctx->Get());
		v8::Local<v8::BigInt> bigint;
		if (maybe_bigint.ToLocal(&bigint))
		{
			*plen = bigint->Uint64Value();
			return 0;
		}
	}
	*plen = 0;
	return -1;
}

const char* JS_ToCStringLen2(JSContext* ctx, size_t* plen, JSValueConst val1, JS_BOOL cesu8)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	v8::MaybeLocal<v8::Value> maybe_value = ctx->_runtime->GetValue(val1);
	v8::Local<v8::Value> value;
	if (maybe_value.ToLocal(&value))
	{
		v8::MaybeLocal<v8::String> maybe_string = value->ToString(ctx->Get());
		v8::Local<v8::String> _string;
		if (maybe_string.ToLocal(&_string) && _string->Length() > 0)
		{
			v8::String::Utf8Value str(ctx->GetIsolate(), _string);
			size_t len = str.length();
			char* pmem = (char*)ctx->_runtime->malloc_functions.js_malloc(nullptr, len + 1);
			if (pmem)
			{
				memcpy(pmem, *str, len);
				pmem[len] = 0;
				if (plen)
				{
					*plen = len;
				}
				return pmem;
			}
		}
	}

	if (plen) 
	{
		*plen = 0;
	}
	return nullptr;
}

void JS_FreeCString(JSContext* ctx, const char* ptr)
{
	ctx->_runtime->malloc_functions.js_free(nullptr, (void*)ptr);
}

int JS_IsInstanceOf(JSContext* ctx, JSValueConst val, JSValueConst obj)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	v8::MaybeLocal<v8::Value> v = ctx->_runtime->GetValue(val);
	v8::MaybeLocal<v8::Value> t = ctx->_runtime->GetValue(obj);
	if (!v.IsEmpty() && !t.IsEmpty())
	{
		v8::Local<v8::Object> t_o = v8::Local<v8::Object>::Cast(t.ToLocalChecked());
		v8::Maybe<bool> res = v.ToLocalChecked()->InstanceOf(ctx->Get(), t_o);
		if (res.IsJust())
		{
			return res.FromJust() ? 1 : 0;
		}
	}
	//TODO throw exception and return -1
	return 0;
}

JS_BOOL JS_IsException(JSValueConst val)
{
	return val.tag == JS_TAG_EXCEPTION ? 1 : 0;
}

JSValue JS_GetException(JSContext* ctx)
{
	if (ctx->_runtime->_exception.IsEmpty())
	{
		return JS_UNDEFINED;
	}
	return JS_EXCEPTION;
}

JS_BOOL JS_IsError(JSContext* ctx, JSValueConst val)
{
	if (val.tag == JS_TAG_EXCEPTION)
	{
		return TRUE;
	}
	if (val.tag == JS_TAG_OBJECT)
	{
		//TODO check Error object
		//v8::MaybeLocal<v8::Value> maybe = ctx->_runtime->GetValue(val.u.ptr);
		//v8::Local<v8::Value> value;
		//if (maybe.ToLocal(&value))
		//{
		//}
	}
	return FALSE;
}

JSValue JSB_ThrowError(JSContext* ctx, const char* buf, size_t buf_len)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->_runtime->ThrowError(ctx->Get(), buf, (int)buf_len);
}

JSValue JSB_ThrowTypeError(JSContext* ctx, const char* buf)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->_runtime->ThrowTypeError(ctx->Get(), buf);
}

JSValue JSB_ThrowInternalError(JSContext* ctx, const char* buf)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->_runtime->ThrowError(ctx->Get(), buf);
}

JSValue JSB_ThrowRangeError(JSContext* ctx, const char* buf)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->_runtime->ThrowError(ctx->Get(), buf);
}

JSValue JSB_ThrowReferenceError(JSContext* ctx, const char* buf)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->_runtime->ThrowError(ctx->Get(), buf);
}

JSClassID JSB_NewClass(JSRuntime* rt, JSClassID class_id, const char* class_name, JSGCObjectFinalizer* finalizer)
{
	v8::Isolate::Scope isolateScope(rt->_isolate);
	v8::HandleScope handleScope(rt->_isolate);

	return rt->NewClass(class_id, class_name, finalizer);
}

JSValue JS_NewObjectProtoClass(JSContext* ctx, JSValueConst proto, JSClassID class_id)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	if (proto.tag == JS_TAG_OBJECT)
	{
		JSClassDef* def = ctx->_runtime->GetClassDef(class_id);
		if (def)
		{
			v8::MaybeLocal<v8::Value> maybe_value = ctx->_runtime->GetValue(proto.u.ptr);
			v8::Local<v8::Value> value;
			if (maybe_value.ToLocal(&value) && value->IsObject())
			{
				return ctx->NewObjectProtoClass(v8::Local<v8::Object>::Cast(value), def);
			}
		}
	}
	return ctx->_runtime->ThrowError(ctx->Get(), "proto is not an object");
}

// DO NOT USE 'magic'
JSValue JSB_NewCFunction(JSContext* ctx, JSCFunction* func, JSAtom atom, int length, JSCFunctionEnum cproto, int magic)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->NewCFunction(func, atom, length, cproto);
}

JSValue JSB_NewCFunctionMagic(JSContext* ctx, JSCFunctionMagic* func, JSAtom atom, int length, JSCFunctionEnum cproto, int magic)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->NewCFunctionMagic(func, atom, length, cproto, magic);
}

JSValue JS_GetGlobalObject(JSContext* ctx)
{
	return ctx->GetGlobal();
}

JSValue JS_NewObject(JSContext* ctx)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->NewObject();
}

JSValue JSB_NewEmptyString(JSContext* ctx)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->GetEmptyString();
}

JSValue JS_NewString(JSContext* ctx, const char* str)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	v8::MaybeLocal<v8::String> maybe = v8::String::NewFromUtf8(ctx->GetIsolate(), str);
	if (maybe.IsEmpty())
	{
		return ctx->_runtime->ThrowError(ctx->Get(), "failed to new string");
	}
	return ctx->_runtime->AddString(ctx->Get(), maybe.ToLocalChecked());
}

JSValue JS_NewStringLen(JSContext* ctx, const char* buf, size_t buf_len)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	v8::MaybeLocal<v8::String> maybe = v8::String::NewFromUtf8(ctx->GetIsolate(), buf, v8::NewStringType::kNormal, (int)buf_len);
	if (maybe.IsEmpty())
	{
		return ctx->_runtime->ThrowError(ctx->Get(), "failed to new string");
	}
	return ctx->_runtime->AddString(ctx->Get(), maybe.ToLocalChecked());
}

JSValue JSB_NewInt64(JSContext* ctx, int64_t val)
{
	JSValue v;
	if (val == (int32_t)val) {
		v = JS_MKINT32(JS_TAG_INT, (int32_t)val);
	}
	else {
		v = JS_MKFLOAT64(JS_TAG_FLOAT64, (double)val);
	}
	return v;
}

JSValue JSB_NewFloat64(JSContext* ctx, double d)
{
	JSValue v;
	int32_t val;
	union {
		double d;
		uint64_t u;
	} u, t;
	u.d = d;
	val = (int32_t)d;
	t.d = val;
	/* -0 cannot be represented as integer, so we compare the bit
		representation */
	if (u.u == t.u) {
		v = JS_MKINT32(JS_TAG_INT, val);
	}
	else {
		v = JS_MKFLOAT64(JS_TAG_FLOAT64, d);
	}
	return v;
}

IntPtr JS_GetArrayBuffer(JSContext* ctx, size_t* psize, JSValueConst obj)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	v8::MaybeLocal<v8::Value> maybe_value = ctx->_runtime->GetValue(obj);
	v8::Local<v8::Value> value;
	if (maybe_value.ToLocal(&value) && value->IsArrayBuffer()) 
	{
		v8::Local<v8::ArrayBuffer> array_buffer = v8::Local<v8::ArrayBuffer>::Cast(value);
		std::shared_ptr<v8::BackingStore> backing_store = array_buffer->GetBackingStore();
		void* buf = backing_store->Data();
		if (buf)
		{
			*psize = backing_store->ByteLength();
		}
		return buf;
	}
	*psize = 0;
	return 0;
}

JSValue JS_NewArrayBufferCopy(JSContext* ctx, const char* buf, size_t len)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::Context::Scope contextScope(ctx->Get());
	v8::HandleScope handleScope(ctx->GetIsolate());

	v8::Local<v8::ArrayBuffer> array_buffer = v8::ArrayBuffer::New(ctx->GetIsolate(), len);
	std::shared_ptr<v8::BackingStore> backing_store = array_buffer->GetBackingStore();
	memcpy(backing_store->Data(), buf, len);
	return ctx->_runtime->AddObject(ctx->Get(), array_buffer);
}

void* JSB_GetOpaque(JSContext* ctx, JSValue val, JSClassID class_id)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	v8::MaybeLocal<v8::Value> maybe_value = ctx->_runtime->GetValue(val);
	v8::Local<v8::Value> value;
	if (maybe_value.ToLocal(&value) && value->IsObject())
	{
		v8::Local<v8::Object> obj = v8::Local<v8::Object>::Cast(value);
		return v8::Local<v8::External>::Cast(obj->GetInternalField(JSRuntime::EIFN_Payload))->Value();
	}
	return nullptr;
}

void JSB_SetOpaque(JSContext* ctx, JSValue val, void* data)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	v8::MaybeLocal<v8::Value> maybe_value = ctx->_runtime->GetValue(val);
	v8::Local<v8::Value> value;
	if (maybe_value.ToLocal(&value) && value->IsObject())
	{
		v8::Local<v8::Object> obj = v8::Local<v8::Object>::Cast(value);
		obj->SetInternalField(JSRuntime::EIFN_Payload, v8::External::New(ctx->GetIsolate(), data));
	}
}

JSValue JS_NewArray(JSContext* ctx)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->NewArray();
}

int JS_IsArray(JSContext* ctx, JSValueConst val)
{
	if (val.tag == JS_TAG_OBJECT)
	{
		v8::Isolate::Scope isolateScope(ctx->GetIsolate());
		v8::HandleScope handleScope(ctx->GetIsolate());

		v8::MaybeLocal<v8::Value> maybe_value = ctx->_runtime->GetValue(val.u.ptr);
		v8::Local<v8::Value> value;
		if (maybe_value.ToLocal(&value) && value->IsArray())
		{
			return TRUE;
		}
	}
	return FALSE;
}

JS_BOOL JS_IsFunction(JSContext* ctx, JSValueConst val)
{
	if (val.tag == JS_TAG_OBJECT)
	{
		v8::Isolate::Scope isolateScope(ctx->GetIsolate());
		v8::HandleScope handleScope(ctx->GetIsolate());

		v8::MaybeLocal<v8::Value> maybe_value = ctx->_runtime->GetValue(val.u.ptr);
		v8::Local<v8::Value> value;
		if (maybe_value.ToLocal(&value) && value->IsFunction())
		{
			return TRUE;
		}
	}
	return FALSE;
}

JS_BOOL JS_IsConstructor(JSContext* ctx, JSValueConst val)
{
	if (val.tag == JS_TAG_OBJECT)
	{
		v8::Isolate::Scope isolateScope(ctx->GetIsolate());
		v8::HandleScope handleScope(ctx->GetIsolate());

		v8::MaybeLocal<v8::Value> maybe_value = ctx->_runtime->GetValue(val.u.ptr);
		v8::Local<v8::Value> value;
		if (maybe_value.ToLocal(&value) && value->IsFunction())
		{
			v8::Local<v8::Function> func_value = v8::Local<v8::Function>::Cast(value);
			return func_value->IsConstructor() ? TRUE : FALSE;
		}
	}
	return FALSE;
}

JSValue JS_GetPropertyStr(JSContext* ctx, JSValueConst this_obj, const char* prop)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->GetPropertyStr(this_obj, prop);
}

JSValue JS_GetProperty(JSContext* ctx, JSValueConst this_obj, JSAtom prop)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->GetProperty(this_obj, prop);
}

JSValue JS_GetPropertyUint32(JSContext* ctx, JSValueConst this_obj, uint32_t idx)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->GetPropertyUint32(this_obj, idx);
}

JSValue JS_GetPropertyInternal(JSContext* ctx, JSValueConst this_obj, JSAtom prop, JSValueConst receiver, JS_BOOL throw_ref_error)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->GetPropertyInternal(this_obj, prop, receiver, throw_ref_error);
}

int JS_HasProperty(JSContext* ctx, JSValueConst this_obj, JSAtom prop)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->HasProperty(this_obj, prop);
}

// return TRUE/FALSE or -1 if exception
int JS_SetPropertyUint32(JSContext* ctx, JSValueConst this_obj, uint32_t idx, JSValue val)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->SetPropertyUint32(this_obj, idx, val);
}

// return TRUE/FALSE or -1 if exception
int JS_SetPropertyInternal(JSContext* ctx, JSValueConst this_obj, JSAtom prop, JSValue val, int flags)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->SetPropertyInternal(this_obj, prop, val, flags);
}

int JS_DefineProperty(JSContext* ctx, JSValueConst this_obj, JSAtom prop, JSValueConst val, JSValueConst getter, JSValueConst setter, int flags)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->DefineProperty(this_obj, prop, getter, setter, flags);
}

int JS_DefinePropertyValue(JSContext* ctx, JSValueConst this_obj, JSAtom prop, JSValue val, int flags)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->DefinePropertyValue(this_obj, prop, val, flags);
}

int JS_SetPrototype(JSContext* ctx, JSValueConst obj, JSValueConst proto_val)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	v8::MaybeLocal<v8::Value> obj_v = ctx->_runtime->GetValue(obj);
	v8::MaybeLocal<v8::Value> proto_val_v = ctx->_runtime->GetValue(proto_val);
	if (!obj_v.IsEmpty() && !proto_val_v.IsEmpty())
	{
		v8::Local<v8::Object> obj_o = v8::Local<v8::Object>::Cast(obj_v.ToLocalChecked());
		v8::Maybe<bool> res = obj_o->SetPrototype(ctx->Get(), proto_val_v.ToLocalChecked());
		if (res.IsJust())
		{
			return res.FromJust() ? 1 : 0;
		}
	}
	//TODO throw exception and return -1
	return 0;
}

void JS_SetConstructor(JSContext* ctx, JSValueConst func_obj, JSValueConst proto)
{
	JS_DefinePropertyValue(ctx, func_obj, JS_ATOM_prototype, JSB_DupValue(ctx, proto), 0);
	JS_DefinePropertyValue(ctx, proto, JS_ATOM_constructor, JSB_DupValue(ctx, func_obj), 0);
}

JSValue JS_Eval(JSContext* ctx, const char* input, size_t input_len, const char* filename, int eval_flags)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->Eval(input, input_len, filename, eval_flags);
}

JSValue JS_CallConstructor(JSContext* ctx, JSValueConst func_obj, int argc, JSValueConst* argv)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->CallConstructor(func_obj, argc, argv);
}

JSValue JS_Call(JSContext* ctx, JSValueConst func_obj, JSValueConst this_obj, int argc, JSValueConst* argv)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->Call(func_obj, this_obj, argc, argv);
}

JSValue JS_Invoke(JSContext* ctx, JSValueConst this_val, JSAtom atom, int argc, JSValueConst* argv)
{
	JSValue v = JS_GetProperty(ctx, this_val, atom);
	if (JS_IsFunction(ctx, v))
	{
		JSValue ret = JS_Call(ctx, v, this_val, argc, argv);
		JS_FreeValue(ctx, v);
		return ret;
	}

	JS_FreeValue(ctx, v);
	return JS_ThrowError(ctx, "not a function");
}

JSValue JSB_GetGlobalObject(JSContext* ctx)
{
	return ctx->GetGlobal();
}

JSValue JS_ParseJSON(JSContext* ctx, const char* buf, size_t buf_len, const char* filename)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	v8::MaybeLocal<v8::String> json_string = v8::String::NewFromUtf8(ctx->_runtime->_isolate, buf, v8::NewStringType::kNormal, (int)buf_len);
	if (!json_string.IsEmpty())
	{
		v8::Local<v8::Context> context = ctx->Get();
		v8::Context::Scope contextScope(context);
		v8::MaybeLocal<v8::Value> maybe_json_object = v8::JSON::Parse(context, json_string.ToLocalChecked());
		v8::Local<v8::Value> json_object;
		if (maybe_json_object.ToLocal(&json_object))
		{
			return ctx->_runtime->AddValue(ctx->Get(), json_object);
		}
	}
	return JS_ThrowError(ctx, "can not parse");
}

JSValue JS_JSONStringify(JSContext* ctx, JSValueConst obj, JSValueConst replacer, JSValueConst space0)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	v8::MaybeLocal<v8::Value> json_object = ctx->_runtime->GetValue(obj);
	if (!json_object.IsEmpty())
	{
		v8::MaybeLocal<v8::String> maybe_json_string = v8::JSON::Stringify(ctx->Get(), json_object.ToLocalChecked());
		v8::Local<v8::String> json_string;
		if (maybe_json_string.ToLocal(&json_string))
		{
			return ctx->_runtime->AddString(ctx->Get(), json_string);
		}
	}
	return JS_UNDEFINED;
}

JSValue JS_NewPromiseCapability(JSContext* ctx, JSValue* resolving_funcs)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->NewPromiseCapability(resolving_funcs);
}

void JS_SetHostPromiseRejectionTracker(JSRuntime* rt, JSHostPromiseRejectionTracker* cb, void* opaque)
{
	return rt->SetHostPromiseRejectionTracker(cb, opaque);
}

int JS_ExecutePendingJob(JSRuntime* rt, JSContext** pctx)
{
	return rt->ExecutePendingJob(pctx);
}

void JS_SetInterruptHandler(JSRuntime* rt, JSInterruptHandler* cb, IntPtr opaque)
{
	//TODO not implemented
}

void JS_ComputeMemoryUsage(JSRuntime* rt, JSMemoryUsage* s)
{
}

#ifdef __cplusplus
}
#endif

