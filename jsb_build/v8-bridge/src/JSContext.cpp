#include "JSContext.h"
#include <cassert>

static void _SetReturnValue(JSRuntime* runtime, const v8::FunctionCallbackInfo<v8::Value>& info, JSValue rval)
{
	switch (rval.tag)
	{
	case JS_TAG_UNDEFINED:
		break;
	case JS_TAG_EXCEPTION:
	{
		//v8::Local<v8::Value> e = runtime->_exception.Get(runtime->_isolate);
		//if (!e.IsEmpty())
		//{
		//	runtime->_exception.Reset();
		//	runtime->_isolate->ThrowException(e);
		//}
		//else
		//{
		//	runtime->_isolate->ThrowError("unexpected exception");
		//}
		break;
	}
	default:
	{
		v8::MaybeLocal<v8::Value> maybe_returnValue = runtime->GetValue(rval);
		v8::Local<v8::Value> returnValue;
		runtime->FreeValue(rval);
		if (maybe_returnValue.ToLocal(&returnValue))
		{
			info.GetReturnValue().Set(returnValue);
		}
		else
		{
			runtime->_isolate->ThrowError("failed to translate return value from JSValue");
		}
		break;
	}
	}
}

static void JSCFunctionConstructorMagicCallback(const v8::FunctionCallbackInfo<v8::Value>& info)
{
	v8::Isolate* _isolate = info.GetIsolate();
	v8::Isolate::Scope isolateScope(_isolate);
	v8::HandleScope handleScope(_isolate);

	JSContext::JSCFunctionMagicWrapper* wrapper = reinterpret_cast<JSContext::JSCFunctionMagicWrapper*>(v8::Local<v8::External>::Cast(info.Data())->Value());

	v8::Local<v8::Context> context = wrapper->_context->Get();
	JSRuntime* runtime = wrapper->_context->_runtime;
	JSValue this_obj = runtime->AddObject(context, info.This());
	int argc = info.Length();
	JSValue* argv = argc > 0 ? (JSValue*)alloca(sizeof(JSValue) * argc) : nullptr;

	for (int i = 0; i < argc; i++)
	{
		argv[i] = runtime->AddValue(context, info[i]);
	}

	JSValue rval = wrapper->_funcMagic(wrapper->_context, this_obj, argc, argv, wrapper->_magic);
	runtime->FreeValue(this_obj);
	for (int i = 0; i < argc; i++)
	{
		runtime->FreeValue(argv[i]);
	}
	_SetReturnValue(runtime, info, rval);
}

static void JSCFunctionMagicCallback(const v8::FunctionCallbackInfo<v8::Value>& info)
{
	v8::Isolate* _isolate = info.GetIsolate();
	v8::Isolate::Scope isolateScope(_isolate);
	v8::HandleScope handleScope(_isolate);

	JSContext::JSCFunctionMagicWrapper* wrapper = reinterpret_cast<JSContext::JSCFunctionMagicWrapper*>(v8::Local<v8::External>::Cast(info.Data())->Value());

	v8::Local<v8::Context> context = wrapper->_context->Get();
	JSRuntime* runtime = wrapper->_context->_runtime;
	JSValue this_obj = runtime->AddObject(context, info.This());
	int argc = info.Length();
	JSValue* argv = argc > 0 ? (JSValue*)alloca(sizeof(JSValue) * argc) : nullptr;

	for (int i = 0; i < argc; i++)
	{
		argv[i] = runtime->AddValue(context, info[i]);
	}

	JSValue rval = wrapper->_funcMagic(wrapper->_context, this_obj, argc, argv, wrapper->_magic);
	runtime->FreeValue(this_obj);
	for (int i = 0; i < argc; i++)
	{
		runtime->FreeValue(argv[i]);
	}
	_SetReturnValue(runtime, info, rval);
}

static void JSCFunctionCallback(const v8::FunctionCallbackInfo<v8::Value>& info)
{
	v8::Isolate* _isolate = info.GetIsolate();
	v8::Isolate::Scope isolateScope(_isolate);
	v8::HandleScope handleScope(_isolate);

	JSContext::JSCFunctionMagicWrapper* wrapper = reinterpret_cast<JSContext::JSCFunctionMagicWrapper*>(v8::Local<v8::External>::Cast(info.Data())->Value());

	v8::Local<v8::Context> context = wrapper->_context->Get();
	JSRuntime* runtime = wrapper->_context->_runtime;
	JSValue this_obj = runtime->AddObject(context, info.This());
	int argc = info.Length();
	JSValue* argv = argc > 0 ? (JSValue*)alloca(sizeof(JSValue) * argc) : nullptr;

	for (int i = 0; i < argc; i++)
	{
		argv[i] = runtime->AddValue(context, info[i]);
	}

	JSValue rval = wrapper->_func(wrapper->_context, this_obj, argc, argv);
	runtime->FreeValue(this_obj);
	for (int i = 0; i < argc; i++)
	{
		runtime->FreeValue(argv[i]);
	}
	_SetReturnValue(runtime, info, rval);
}

static void JSCFunctionGetterMagicCallback(const v8::FunctionCallbackInfo<v8::Value>& info)
{
	v8::Isolate* _isolate = info.GetIsolate();
	v8::Isolate::Scope isolateScope(_isolate);
	v8::HandleScope handleScope(_isolate);

	JSContext::JSCFunctionMagicWrapper* wrapper = reinterpret_cast<JSContext::JSCFunctionMagicWrapper*>(v8::Local<v8::External>::Cast(info.Data())->Value());

	v8::Local<v8::Context> context = wrapper->_context->Get();
	JSRuntime* runtime = wrapper->_context->_runtime;
	JSValue this_obj = runtime->AddObject(context, info.This());

	JSValue rval = wrapper->_getterMagic(wrapper->_context, this_obj, wrapper->_magic);
	runtime->FreeValue(this_obj);
	_SetReturnValue(runtime, info, rval);
}

static void JSCFunctionGetterCallback(const v8::FunctionCallbackInfo<v8::Value>& info)
{
	v8::Isolate* _isolate = info.GetIsolate();
	v8::Isolate::Scope isolateScope(_isolate);
	v8::HandleScope handleScope(_isolate);

	JSContext::JSCFunctionMagicWrapper* wrapper = reinterpret_cast<JSContext::JSCFunctionMagicWrapper*>(v8::Local<v8::External>::Cast(info.Data())->Value());

	v8::Local<v8::Context> context = wrapper->_context->Get();
	JSRuntime* runtime = wrapper->_context->_runtime;
	JSValue this_obj = runtime->AddObject(context, info.This());

	JSValue rval = wrapper->_getter(wrapper->_context, this_obj);
	runtime->FreeValue(this_obj);
	_SetReturnValue(runtime, info, rval);
}

static void JSCFunctionSetterMagicCallback(const v8::FunctionCallbackInfo<v8::Value>& info)
{
	v8::Isolate* _isolate = info.GetIsolate();
	v8::Isolate::Scope isolateScope(_isolate);
	v8::HandleScope handleScope(_isolate);

	JSContext::JSCFunctionMagicWrapper* wrapper = reinterpret_cast<JSContext::JSCFunctionMagicWrapper*>(v8::Local<v8::External>::Cast(info.Data())->Value());

	v8::Local<v8::Context> context = wrapper->_context->Get();
	JSRuntime* runtime = wrapper->_context->_runtime;
	JSValue this_obj = runtime->AddObject(context, info.This());
	JSValue val = info.Length() == 1 ? runtime->AddValue(context, info[0]) : JS_UNDEFINED;

	JSValue rval = wrapper->_setterMagic(wrapper->_context, this_obj, val, wrapper->_magic);
	runtime->FreeValue(this_obj);
	runtime->FreeValue(val);

	_SetReturnValue(runtime, info, rval);
}

static void JSCFunctionSetterCallback(const v8::FunctionCallbackInfo<v8::Value>& info)
{
	v8::Isolate* _isolate = info.GetIsolate();
	v8::Isolate::Scope isolateScope(_isolate);
	v8::HandleScope handleScope(_isolate);

	JSContext::JSCFunctionMagicWrapper* wrapper = reinterpret_cast<JSContext::JSCFunctionMagicWrapper*>(v8::Local<v8::External>::Cast(info.Data())->Value());

	v8::Local<v8::Context> context = wrapper->_context->Get();
	JSRuntime* runtime = wrapper->_context->_runtime;
	JSValue this_obj = runtime->AddObject(context, info.This());
	JSValue val = info.Length() == 1 ? runtime->AddValue(context, info[0]) : JS_UNDEFINED;

	JSValue rval = wrapper->_setter(wrapper->_context, this_obj, val);
	runtime->FreeValue(this_obj);
	runtime->FreeValue(val);

	_SetReturnValue(runtime, info, rval);
}

JSContext::JSContext(JSRuntime* runtime)
{
	_runtime = runtime;

	v8::Local<v8::Context> context = v8::Context::New(_runtime->_isolate);
	v8::Context::Scope contextScope(context);
	context->SetAlignedPointerInEmbedderData(JS_CONTEXT_DATA_SELF, this);
	_context.Reset(_runtime->_isolate, context); // = v8::UniquePersistent<v8::Context>(_runtime->_isolate, context);

	v8::Local<v8::Object> global = context->Global();
	_global = _runtime->AddObject(context, global);
	_emptyString = _runtime->AddString(context, v8::String::NewFromUtf8Literal(_runtime->_isolate, ""));
}

JSContext::~JSContext()
{
	size_t size = _functionMagicWrappers.size();
	for (size_t i = 0; i < size; ++i)
	{
		_runtime->mem_free(nullptr, _functionMagicWrappers[i]);
	}
	_runtime->FreeValue(_emptyString);
	_runtime->FreeValue(_global);
	_context.Reset();
}

std::string JSContext::GetAtomString(JSAtom atom)
{
	v8::MaybeLocal<v8::String> maybe_value = _runtime->GetAtomValue(atom);
	v8::Local<v8::String> value;
	if (maybe_value.ToLocal(&value))
	{
		v8::String::Utf8Value utf8_value(_runtime->_isolate, value);
		return *utf8_value;
	}
	return {};
}

JSValue JSContext::AtomToValue(JSAtom atom)
{
	v8::MaybeLocal<v8::String> maybe_value = _runtime->GetAtomValue(atom);
	v8::Local<v8::String> value;
	if (maybe_value.ToLocal(&value))
	{
		return _runtime->AddString(Get(), value);
		//return JS_MKPTR(JS_TAG_STRING, (size_t)atom);
	}
	return JS_UNDEFINED;
}

v8::Local<v8::Context> JSContext::Get()
{
	return _context.Get(_runtime->_isolate);
}

v8::Isolate* JSContext::GetIsolate()
{
	return _runtime->_isolate;
}

JSValue JSContext::GetGlobal()
{
	return _runtime->DupValue(_global);
}

JSValue JSContext::GetEmptyString()
{
	return _runtime->DupValue(_emptyString);
}

JSValue JSContext::NewArray()
{
	v8::Context::Scope contextScope(Get());
	v8::Local<v8::Array> array_value = v8::Array::New(GetIsolate());
	return _runtime->AddObject(Get(), array_value);
}

JSValue JSContext::NewObject()
{
	v8::Context::Scope contextScope(Get());
	v8::Local<v8::Object> object_value = v8::Object::New(GetIsolate());
	return _runtime->AddObject(Get(), object_value);
}

JSValue JSContext::NewObjectProtoClass(v8::Local<v8::Object> new_target, JSClassDef* classDef)
{
	v8::Local<v8::Context> context = Get();
	v8::Context::Scope contextScope(context);
	v8::Local<v8::FunctionTemplate> _class = classDef->_class.Get(GetIsolate());
	
	v8::Local<v8::Object> prototype = new_target;
	v8::Local<v8::Object> instance = _class->GetFunction(context).ToLocalChecked()->NewInstance(context).ToLocalChecked();
	instance->SetPrototype(context, prototype).Check();
	return _runtime->AddGCObject(context, instance, classDef);
}

JSValue JSContext::NewCFunctionMagic(JSCFunctionMagic* func, JSAtom atom, int length, JSCFunctionEnum cproto, int magic)
{
	JSCFunctionMagicWrapper* wrapper = (JSCFunctionMagicWrapper*)_runtime->mem_alloc(nullptr, sizeof(JSCFunctionMagicWrapper));
	wrapper->_context = this;
	wrapper->_magic = magic;
	wrapper->_funcMagic = func;
	_functionMagicWrappers.push_back(wrapper);
	v8::Local<v8::Context> context = Get();
	v8::Context::Scope contextScope(context);
	v8::Local<v8::External> data = v8::External::New(_runtime->_isolate, wrapper);
	
	switch (cproto)
	{
		case JS_CFUNC_constructor_magic:
		{
			v8::Local<v8::FunctionTemplate> function_template = v8::FunctionTemplate::New(GetIsolate(), JSCFunctionConstructorMagicCallback, data, {}, length);
			v8::MaybeLocal<v8::Function> func_o = function_template->GetFunction(context);
			// v8::MaybeLocal<v8::Function> func_o = v8::Function::New(context, JSCFunctionConstructorMagicCallback, data, length);
			v8::Local<v8::Function> func_v;
			if (func_o.ToLocal(&func_v))
			{
				return _runtime->AddObject(context, func_v);
			}
			break;
		}
		case JS_CFUNC_generic_magic:
		{
			v8::MaybeLocal<v8::Function> func_o = v8::Function::New(context, JSCFunctionMagicCallback, data, length);
			v8::Local<v8::Function> func_v;
			if (func_o.ToLocal(&func_v))
			{
				return _runtime->AddObject(context, func_v);
			}
			break;
		}
		case JS_CFUNC_getter_magic:
		{
			v8::MaybeLocal<v8::Function> func_o = v8::Function::New(context, JSCFunctionGetterMagicCallback, data, length);
			v8::Local<v8::Function> func_v;
			if (func_o.ToLocal(&func_v))
			{
				return _runtime->AddObject(context, func_v);
			}
			break;
		}
		case JS_CFUNC_setter_magic:
		{
			v8::MaybeLocal<v8::Function> func_o = v8::Function::New(context, JSCFunctionSetterMagicCallback, data, length);
			v8::Local<v8::Function> func_v;
			if (func_o.ToLocal(&func_v))
			{
				return _runtime->AddObject(context, func_v);
			}
			break;
		}
		default: 
		{
			return _runtime->ThrowError(context, "unsupported function enum");
		}
	}

	return _runtime->ThrowError(context, "failed to make function");
	
}

JSValue JSContext::NewCFunction(JSCFunction* func, JSAtom atom, int length, JSCFunctionEnum cproto)
{
	v8::Context::Scope contextScope(Get());
	JSCFunctionMagicWrapper* wrapper = (JSCFunctionMagicWrapper*)_runtime->mem_alloc(nullptr, sizeof(JSCFunctionMagicWrapper));
	wrapper->_context = this;
	wrapper->_func = func;
	wrapper->_magic = 0;
	_functionMagicWrappers.push_back(wrapper);
	v8::Local<v8::External> data = v8::External::New(_runtime->_isolate, wrapper);
	v8::Local<v8::Context> context = Get();

	switch (cproto)
	{
	case JS_CFUNC_generic:
	{
		v8::MaybeLocal<v8::Function> func_o = v8::Function::New(Get(), JSCFunctionCallback, data, length);
		v8::Local<v8::Function> func_v;
		if (func_o.ToLocal(&func_v))
		{
			return _runtime->AddObject(context, func_v);
		}
		break;
	}
	case JS_CFUNC_getter:
	{
		v8::MaybeLocal<v8::Function> func_o = v8::Function::New(Get(), JSCFunctionGetterCallback, data, length);
		v8::Local<v8::Function> func_v;
		if (func_o.ToLocal(&func_v))
		{
			return _runtime->AddObject(context, func_v);
		}
		break;
	}
	case JS_CFUNC_setter:
	{
		v8::MaybeLocal<v8::Function> func_o = v8::Function::New(Get(), JSCFunctionSetterCallback, data, length);
		v8::Local<v8::Function> func_v;
		if (func_o.ToLocal(&func_v))
		{
			return _runtime->AddObject(context, func_v);
		}
		break;
	}
	default:
	{
		return _runtime->ThrowError(context, "unsupported function enum");
	}
	}

	return _runtime->ThrowError(context, "failed to make function");
}

JSValue JSContext::GetPropertyStr(JSValueConst this_obj, const char* prop)
{
	v8::MaybeLocal<v8::Value> maybe_value = _runtime->GetValue(this_obj);
	v8::Local<v8::Value> value;
	v8::Local<v8::Context> context = Get();

	if (maybe_value.ToLocal(&value) && value->IsObject())
	{
		v8::Local<v8::Object> obj = v8::Local<v8::Object>::Cast(value);
		v8::MaybeLocal<v8::String> pkey = v8::String::NewFromUtf8(GetIsolate(), prop);
		if (!pkey.IsEmpty())
		{
			v8::MaybeLocal<v8::Value> maybe_res = obj->Get(context, pkey.ToLocalChecked());
			v8::Local<v8::Value> res;
			if (maybe_res.ToLocal(&res))
			{
				return _runtime->AddValue(context, res);
			}
			return _runtime->ThrowError(context, "failed to call Object::Has()");
		}
		return _runtime->ThrowError(context, "GetPropertyStr: no such JSAtom");
	}
	return _runtime->ThrowError(context, "not an object");
}

JSValue JSContext::GetProperty(JSValueConst this_obj, JSAtom prop)
{
	return GetPropertyInternal(this_obj, prop, this_obj, 0);
}

JSValue JSContext::GetPropertyUint32(JSValueConst this_obj, uint32_t idx)
{
	v8::MaybeLocal<v8::Value> maybe_value = _runtime->GetValue(this_obj);
	v8::Local<v8::Value> value;
	v8::Local<v8::Context> context = Get();

	if (maybe_value.ToLocal(&value) && value->IsObject())
	{
		v8::Local<v8::Object> obj = v8::Local<v8::Object>::Cast(value);
		v8::MaybeLocal<v8::Value> maybe_res = obj->Get(context, idx);
		v8::Local<v8::Value> res;
		if (maybe_res.ToLocal(&res))
		{
			return _runtime->AddValue(context, res);
		}
		return _runtime->ThrowError(context, "failed to call Object::Has()");
	}
	return _runtime->ThrowError(context, "not an object");
}

JSValue JSContext::GetPropertyInternal(JSValueConst this_obj, JSAtom prop, JSValueConst receiver, JS_BOOL throw_ref_error)
{
	v8::MaybeLocal<v8::Value> maybe_value = _runtime->GetValue(this_obj);
	v8::Local<v8::Value> value;
	v8::Local<v8::Context> context = Get();

	if (maybe_value.ToLocal(&value) && value->IsObject())
	{
		v8::Local<v8::Object> obj = v8::Local<v8::Object>::Cast(value);
		v8::MaybeLocal<v8::String> pkey = _runtime->GetAtomValue(prop);
		if (!pkey.IsEmpty())
		{
			v8::MaybeLocal<v8::Value> maybe_res = obj->Get(context, pkey.ToLocalChecked());
			v8::Local<v8::Value> res;
			if (maybe_res.ToLocal(&res))
			{
				return _runtime->AddValue(context, res);
			}
			return _runtime->ThrowError(context, "failed to call Object::Has()");
		}
		return _runtime->ThrowError(context, "GetPropertyInternal: no such JSAtom");
	}
	return _runtime->ThrowError(context, "not an object");
}

int JSContext::HasProperty(JSValueConst this_obj, JSAtom prop)
{
	v8::MaybeLocal<v8::Value> maybe_value = _runtime->GetValue(this_obj);
	v8::Local<v8::Value> value;
	v8::Local<v8::Context> context = Get();

	if (maybe_value.ToLocal(&value) && value->IsObject())
	{
		v8::Local<v8::Object> obj = v8::Local<v8::Object>::Cast(value);
		v8::MaybeLocal<v8::String> pkey = _runtime->GetAtomValue(prop);
		if (!pkey.IsEmpty())
		{
			v8::Maybe<bool> maybe_res = obj->Has(context, pkey.ToLocalChecked());
			return maybe_res.FromMaybe(false) ? 1 : 0;
		}
		else
		{
			_runtime->ThrowError(context, "HasProperty: no such JSAtom");
		}
	}
	else
	{
		_runtime->ThrowError(context, "not an object");
	}
	return -1;
}

int JSContext::SetPropertyUint32(JSValueConst this_obj, uint32_t idx, JSValue val)
{
	v8::MaybeLocal<v8::Value> maybe_value = _runtime->GetValue(this_obj);
	v8::Local<v8::Value> value;
	v8::Local<v8::Context> context = Get();

	if (maybe_value.ToLocal(&value) && value->IsObject())
	{
		v8::Local<v8::Object> obj = v8::Local<v8::Object>::Cast(value);
		v8::MaybeLocal<v8::Value> maybe_pvalue = _runtime->GetValue(val);
		v8::Local<v8::Value> pvalue;
		if (maybe_pvalue.ToLocal(&pvalue))
		{
			v8::Maybe<bool> maybe_res = obj->Set(context, idx, pvalue);

			_runtime->FreeValue(val);
			return maybe_res.FromMaybe(false) ? 1 : 0;
		}
		else
		{
			_runtime->ThrowError(context, "SetPropertyUint32: no such JSAtom");
		}
	}
	else
	{
		_runtime->ThrowError(context, "not an object");
	}
	_runtime->FreeValue(val);
	return -1;
}

int JSContext::SetPropertyInternal(JSValueConst this_obj, JSAtom prop, JSValue val, int flags)
{
	v8::MaybeLocal<v8::Value> maybe_this = _runtime->GetValue(this_obj);
	v8::Local<v8::Context> context = Get();
	v8::Context::Scope contextScope(context);

	v8::Local<v8::Value> thiz;
	if (maybe_this.ToLocal(&thiz) && thiz->IsObject())
	{
		v8::MaybeLocal<v8::String> maybe_key = _runtime->GetAtomValue(prop);
		v8::Local<v8::String> key;
		if (maybe_key.ToLocal(&key))
		{
			v8::Local<v8::Object> obj = v8::Local<v8::Object>::Cast(thiz);
			v8::MaybeLocal<v8::Value> maybe_pvalue = _runtime->GetValue(val);
			v8::Local<v8::Value> pvalue;
			if (maybe_pvalue.ToLocal(&pvalue))
			{
				v8::Maybe<bool> maybe_res = obj->Set(context, key, pvalue);
				_runtime->FreeValue(val);
				return maybe_res.FromMaybe(false) ? 1 : 0;
		}
			else 
			{
				_runtime->ThrowError(context, "failed to get value");
			}
		}
		else
		{
			_runtime->ThrowError(context, "SetPropertyInternal: no such JSAtom");
		}
	}
	else
	{
		_runtime->ThrowError(context, "not an object");
	}
	_runtime->FreeValue(val);
	return -1;
}

static v8::Local<v8::Function> GetFunction(v8::MaybeLocal<v8::Value> value)
{
	v8::Local<v8::Value> t;
	if (value.ToLocal(&t) && t->IsFunction())
	{
		return v8::Local<v8::Function>::Cast(t);
	}
	return {};
}

int JSContext::DefineProperty(JSValueConst this_obj, JSAtom prop, JSValueConst getter, JSValueConst setter, int flags)
{
	v8::MaybeLocal<v8::Value> maybe_value = _runtime->GetValue(this_obj);
	v8::Local<v8::Value> value;
	v8::Local<v8::Context> context = Get();

	if (maybe_value.ToLocal(&value) && value->IsObject())
	{
		v8::MaybeLocal<v8::String> pkey = _runtime->GetAtomValue(prop);
		if (!pkey.IsEmpty())
		{
			v8::Local<v8::Object> obj = v8::Local<v8::Object>::Cast(value);
			v8::MaybeLocal<v8::Value> maybe_getter = _runtime->GetValue(getter);
			v8::MaybeLocal<v8::Value> maybe_setter = _runtime->GetValue(setter);

			int attr = v8::None;
			if (!(flags & JS_PROP_CONFIGURABLE))
			{
				attr |= v8::DontDelete;
			}
			if (!(flags & JS_PROP_ENUMERABLE))
			{
				attr |= v8::DontEnum;
			}
			obj->SetAccessorProperty(pkey.ToLocalChecked(), GetFunction(maybe_getter), GetFunction(maybe_setter), 	(v8::PropertyAttribute)attr);
			return 1;
		}
		else
		{
			_runtime->ThrowError(context, "DefineProperty: no such JSAtom");
		}
	}
	else
	{
		_runtime->ThrowError(context, "not an object");
	}
	return -1;
}

int JSContext::DefinePropertyValue(JSValueConst this_obj, JSAtom prop, JSValue val, int flags)
{
	return SetPropertyInternal(this_obj, prop, val, flags);
}

JSValue JSContext::Eval(const char* input, size_t input_len, const char* filename, int eval_flags)
{
	v8::Isolate* isolate = _runtime->_isolate;
	v8::Local<v8::Context> context = _context.Get(isolate);
	v8::Context::Scope contextScope(context);

	v8::TryCatch try_catch(isolate);
	v8::ScriptOrigin origin(isolate, v8::String::NewFromUtf8(isolate, filename).ToLocalChecked());
	v8::MaybeLocal<v8::String> source = v8::String::NewFromUtf8(isolate, input, v8::NewStringType::kNormal, (int)input_len);
	v8::MaybeLocal<v8::Script> script = v8::Script::Compile(context, source.ToLocalChecked(), &origin);
	if (!script.IsEmpty())
	{
		v8::MaybeLocal<v8::Value> maybe_value = script.ToLocalChecked()->Run(context);
		if (try_catch.HasCaught())
		{
			return _runtime->ThrowException(context, try_catch.Exception());
		}
		v8::Local<v8::Value> value;
		if (maybe_value.ToLocal(&value))
		{
			return _runtime->AddValue(context, value);
		}
		return JS_UNDEFINED;
	}

	if (try_catch.HasCaught())
	{
		return _runtime->ThrowException(context, try_catch.Exception());
	}

	return _runtime->ThrowError(context, "failed to compile");
}

JSValue JSContext::CallConstructor(JSValueConst func_obj, int argc, JSValueConst* argv)
{
	v8::Local<v8::Context> context = Get();
	v8::MaybeLocal<v8::Value> maybe_value = _runtime->GetValue(func_obj);
	v8::Local<v8::Value> value;
	if (maybe_value.ToLocal(&value) && value->IsFunction())
	{
		v8::TryCatch try_catch(_runtime->_isolate);
		v8::Local<v8::Function> func_value = v8::Local<v8::Function>::Cast(value);

		std::vector<v8::Local<v8::Value>> _args_;
		for (int i = 0; i < argc; ++i)
		{
			v8::MaybeLocal<v8::Value> maybe_arg_value = _runtime->GetValue(argv[i]);
			v8::Local<v8::Value> arg_value;
			if (!maybe_arg_value.ToLocal(&arg_value))
			{
				static char errmsg[256];
				int errmsg_len = sprintf_s(errmsg, "CallConstructor: translate failed %d:%lld:%zd", i, argv[i].tag, argv[i].u.ptr);
				return _runtime->ThrowError(context, errmsg, errmsg_len);
			}
			_args_.push_back(arg_value);
		}
		v8::MaybeLocal<v8::Value> func_retValues = func_value->CallAsConstructor(context, argc, _args_.data());
		if (try_catch.HasCaught())
		{
			return _runtime->ThrowException(context, try_catch.Exception());
		}
		if (!func_retValues.IsEmpty())
		{
			return _runtime->AddValue(context, func_retValues.ToLocalChecked());
		}
		return _runtime->ThrowError(context, "CallConstructor: failed to call");
	}

	return _runtime->ThrowError(context, "CallConstructor: not a constructor function");
}

JSValue JSContext::Call(JSValueConst func_obj, JSValueConst this_obj, int argc, JSValueConst* argv)
{
	v8::Local<v8::Context> context = Get();
	v8::Context::Scope contextScope(context);
	v8::MaybeLocal<v8::Value> maybe_value = _runtime->GetValue(func_obj);
	v8::Local<v8::Value> value;
	if (maybe_value.ToLocal(&value) && value->IsFunction())
	{
		v8::TryCatch try_catch(_runtime->_isolate);
		v8::Local<v8::Function> func_value = v8::Local<v8::Function>::Cast(value);
		std::vector<v8::Local<v8::Value>> _args_;
		for (int i = 0; i < argc; ++i)
		{
			v8::MaybeLocal<v8::Value> maybe_arg_value = _runtime->GetValue(argv[i]);
			v8::Local<v8::Value> arg_value;
			if (!maybe_arg_value.ToLocal(&arg_value))
			{
				static char errmsg[256];
				int errmsg_len = sprintf_s(errmsg, "Call: translate failed %d:%lld:%zd", i, argv[i].tag, argv[i].u.ptr);
				return _runtime->ThrowError(context, errmsg, errmsg_len);
			}
			_args_.push_back(arg_value);
		}
		v8::Local<v8::Value> this_val = _runtime->GetValue(this_obj).ToLocalChecked();
		v8::MaybeLocal<v8::Value> func_retValues = func_value->Call(context, this_val, argc, _args_.data());
		if (try_catch.HasCaught())
		{
			return _runtime->ThrowException(context, try_catch.Exception());
		}
		if (!func_retValues.IsEmpty())
		{
			return _runtime->AddValue(context, func_retValues.ToLocalChecked());
		}
		return JS_UNDEFINED;
	}
	return _runtime->ThrowError(context, "Call: not a function");
}

static void _JSPromiseResolveCallback(const v8::FunctionCallbackInfo<v8::Value>& info)
{
	v8::Isolate* _isolate = info.GetIsolate();
	v8::Isolate::Scope isolateScope(_isolate);
	v8::HandleScope handleScope(_isolate);

	v8::Local<v8::Context> context = info.GetIsolate()->GetCurrentContext();
	v8::Local<v8::Value> value = info.Data();
	v8::Local<v8::Promise::Resolver> resolver = v8::Local<v8::Promise::Resolver>::Cast(value);
	if (info.Length() > 0)
	{
		v8::Maybe<bool> res = resolver->Resolve(context, info[0]);
		if (res.IsJust())
		{
		}
	}
	else
	{
		v8::Maybe<bool> res = resolver->Resolve(context, v8::Undefined(_isolate));
		if (res.IsJust())
		{
		}
	}
}

static void _JSPromiseRejectCallback(const v8::FunctionCallbackInfo<v8::Value>& info)
{
	v8::Isolate* _isolate = info.GetIsolate();
	v8::Isolate::Scope isolateScope(_isolate);
	v8::HandleScope handleScope(_isolate);

	v8::Local<v8::Context> context = info.GetIsolate()->GetCurrentContext();
	v8::Local<v8::Value> value = info.Data();
	v8::Local<v8::Promise::Resolver> resolver = v8::Local<v8::Promise::Resolver>::Cast(value);
	
	if (info.Length() > 0)
	{
		v8::Maybe<bool> res = resolver->Reject(context, info[0]);
		if (res.IsJust())
		{
		}
	}
	else
	{
		v8::Maybe<bool> res = resolver->Reject(context, v8::Undefined(_isolate));
		if (res.IsJust())
		{
		}
	}
}

JSValue JSContext::NewPromiseCapability(JSValue* resolving_funcs)
{
	v8::Local<v8::Context> context = Get();
	v8::Context::Scope contextScope(context);
	v8::MaybeLocal<v8::Promise::Resolver> maybe_resolver = v8::Promise::Resolver::New(context);
	if (maybe_resolver.IsEmpty())
	{
		return _runtime->ThrowError(context, "failed to create promise");
	}
	v8::Local<v8::Promise::Resolver> resolver = maybe_resolver.ToLocalChecked();
	v8::Local<v8::Promise> promise = resolver->GetPromise();

	resolving_funcs[0] = _runtime->AddObject(context, v8::Function::New(context, _JSPromiseResolveCallback, resolver, 1).ToLocalChecked());
	resolving_funcs[1] = _runtime->AddObject(context, v8::Function::New(context, _JSPromiseRejectCallback, resolver, 1).ToLocalChecked());
	return _runtime->AddObject(context, promise);
}
