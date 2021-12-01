
#include "JSApi.h"
#include <cassert>

#define SO_JSB_VERSION 0xa

#ifdef __cplusplus
extern "C" {
#endif

#define DEF(name, str) \
JSAtom JSB_ATOM_##name() { return JSAtom{ ._value = (uint32_t)JS_ATOM_##name }; }
#include "quickjs-atom.h"
#undef DEF

void JS_AddIntrinsicOperators(JSContext* ctx)
{
}

void JS_RunGC(JSRuntime* rt)
{
	rt->RunGC();
}

int __JSB_Init()
{
	return SO_JSB_VERSION;
}

JSRuntime* JS_NewRuntime()
{
	JSRuntime* rt = new JSRuntime();
	return rt;
}

int JS_FreeRuntime(JSRuntime* rt)
{
	JS_BOOL res = rt->Release();
	delete rt;
	return res;
}

void* JS_GetRuntimeOpaque(JSRuntime* rt)
{
	return rt->_opaque;
}

void JS_SetRuntimeOpaque(JSRuntime* rt, void* opaque)
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

JSClassID JSB_NewClass(JSRuntime* rt, JSClassID class_id, const char* class_name, JSGCObjectFinalizer* finalizer)
{
	return rt->NewClass(class_id, class_name, finalizer);
}

JSValue JS_NewObjectProtoClass(JSContext* ctx, JSValueConst proto, JSClassID class_id)
{
	if (proto.tag == JS_TAG_OBJECT)
	{
		JSClassDef* def = ctx->_runtime->GetClassDef(class_id);
		if (def)
		{
			v8::MaybeLocal<v8::Value> maybe_value = ctx->_runtime->GetValue(proto.u.ptr);
			v8::Local<v8::Value> value;
			if (maybe_value.ToLocal(&value) && value->IsObject())
			{
				v8::Isolate::Scope isolateScope(ctx->GetIsolate());
				v8::HandleScope handleScope(ctx->GetIsolate());

				v8::MaybeLocal<v8::Object> maybe_instance = ctx->NewObjectProtoClass(v8::Local<v8::Object>::Cast(value), def);
				v8::Local<v8::Object> instance;
				if (maybe_instance.ToLocal(&instance))
				{
					return ctx->_runtime->AddGCValue(instance, def);
				}
			}
		}
	}
	return JS_UNDEFINED;
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

JSValue JS_NewObject(JSContext* ctx)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->NewObject();
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

int JS_SetPropertyInternal(JSContext* ctx, JSValueConst this_obj, JSAtom prop, JSValue val, int flags)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->SetPropertyInternal(this_obj, prop, val, flags);
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

JSValue JS_Eval(JSContext* ctx, const char* input, size_t input_len, const char* filename, int eval_flags)
{
	v8::Isolate::Scope isolateScope(ctx->GetIsolate());
	v8::HandleScope handleScope(ctx->GetIsolate());

	return ctx->Eval(input, input_len, filename, eval_flags);
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
		v8::MaybeLocal<v8::Value> maybe_json_object = v8::JSON::Parse(ctx->Get(), json_string.ToLocalChecked());
		v8::Local<v8::Value> json_object;
		if (maybe_json_object.ToLocal(&json_object))
		{
			return ctx->_runtime->AddValue(ctx->Get(), json_object);
		}
	}
	return JS_UNDEFINED;
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

int JS_ExecutePendingJob(JSRuntime* rt, JSContext** pctx)
{
	return rt->ExecutePendingJob(pctx);
}

#ifdef __cplusplus
}
#endif

