#include "JSContext.h"
#include "JSApi.h"

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
	JSValue* argv = nullptr;

	if (argc > 0)
	{
		argv = (JSValue*)alloca(sizeof(JSValue) * argc);
		for (int i = 0; i < argc; i++)
		{
			argv[i] = runtime->AddValue(context, info[i]);
		}
	}

	JSValue rval = wrapper->_funcMagic(wrapper->_context, this_obj, argc, argv, wrapper->_magic);
	runtime->FreeValue(this_obj);
	for (int i = 0; i < argc; i++)
	{
		runtime->FreeValue(argv[i]);
	}

	v8::MaybeLocal<v8::Value> maybe_returnValue = runtime->GetValue(rval);
	v8::Local<v8::Value> returnValue;

	if (maybe_returnValue.ToLocal(&returnValue))
	{
		runtime->FreeValue(rval);
		if (rval.tag == JS_TAG_EXCEPTION)
		{
			runtime->_isolate->ThrowException(returnValue);
		}
		else
		{
			info.GetReturnValue().Set(returnValue);
		}
	}
	else
	{
		runtime->FreeValue(rval);
	}
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
	JSValue* argv = nullptr;

	if (argc > 0)
	{
		argv = (JSValue*)alloca(sizeof(JSValue) * argc);
		for (int i = 0; i < argc; i++)
		{
			argv[i] = runtime->AddValue(context, info[i]);
		}
	}

	JSValue rval = wrapper->_funcMagic(wrapper->_context, this_obj, argc, argv, wrapper->_magic);
	runtime->FreeValue(this_obj);
	for (int i = 0; i < argc; i++)
	{
		runtime->FreeValue(argv[i]);
	}

	v8::MaybeLocal<v8::Value> maybe_returnValue = runtime->GetValue(rval);
	v8::Local<v8::Value> returnValue;

	if (maybe_returnValue.ToLocal(&returnValue))
	{
		runtime->FreeValue(rval);
		if (rval.tag == JS_TAG_EXCEPTION)
		{
			runtime->_isolate->ThrowException(returnValue);
		}
		else
		{
			info.GetReturnValue().Set(returnValue);
		}
	}
	else
	{
		runtime->FreeValue(rval);
	}
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
	JSValue* argv = nullptr;

	if (argc > 0)
	{
		argv = (JSValue*)alloca(sizeof(JSValue) * argc);
		for (int i = 0; i < argc; i++)
		{
			argv[i] = runtime->AddValue(context, info[i]);
		}
	}

	JSValue rval = wrapper->_func(wrapper->_context, this_obj, argc, argv);
	runtime->FreeValue(this_obj);
	for (int i = 0; i < argc; i++)
	{
		runtime->FreeValue(argv[i]);
	}

	v8::MaybeLocal<v8::Value> maybe_returnValue = runtime->GetValue(rval);
	v8::Local<v8::Value> returnValue;

	if (maybe_returnValue.ToLocal(&returnValue))
	{
		runtime->FreeValue(rval);
		if (rval.tag == JS_TAG_EXCEPTION)
		{
			runtime->_isolate->ThrowException(returnValue);
		}
		else
		{
			info.GetReturnValue().Set(returnValue);
		}
	}
	else
	{
		runtime->FreeValue(rval);
	}
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

	v8::MaybeLocal<v8::Value> maybe_returnValue = runtime->GetValue(rval);
	v8::Local<v8::Value> returnValue;

	if (maybe_returnValue.ToLocal(&returnValue))
	{
		runtime->FreeValue(rval);
		if (rval.tag == JS_TAG_EXCEPTION)
		{
			runtime->_isolate->ThrowException(returnValue);
		}
		else
		{
			info.GetReturnValue().Set(returnValue);
		}
	}
	else
	{
		runtime->FreeValue(rval);
	}
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

	v8::MaybeLocal<v8::Value> maybe_returnValue = runtime->GetValue(rval);
	v8::Local<v8::Value> returnValue;

	if (maybe_returnValue.ToLocal(&returnValue))
	{
		runtime->FreeValue(rval);
		if (rval.tag == JS_TAG_EXCEPTION)
		{
			runtime->_isolate->ThrowException(returnValue);
		}
		else
		{
			info.GetReturnValue().Set(returnValue);
		}
	}
	else
	{
		runtime->FreeValue(rval);
	}
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

	v8::MaybeLocal<v8::Value> maybe_returnValue = runtime->GetValue(rval);
	v8::Local<v8::Value> returnValue;

	if (maybe_returnValue.ToLocal(&returnValue))
	{
		runtime->FreeValue(rval);
		if (rval.tag == JS_TAG_EXCEPTION)
		{
			runtime->_isolate->ThrowException(returnValue);
		}
	}
	else
	{
		runtime->FreeValue(rval);
	}
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

	v8::MaybeLocal<v8::Value> maybe_returnValue = runtime->GetValue(rval);
	v8::Local<v8::Value> returnValue;

	if (maybe_returnValue.ToLocal(&returnValue))
	{
		runtime->FreeValue(rval);
		if (rval.tag == JS_TAG_EXCEPTION)
		{
			runtime->_isolate->ThrowException(returnValue);
		}
	}
	else
	{
		runtime->FreeValue(rval);
	}
}

JSContext::JSContext(JSRuntime* runtime)
{
	_runtime = runtime;

	v8::Local<v8::Context> context = v8::Context::New(_runtime->_isolate);
	_context.Reset(_runtime->_isolate, context); // = v8::UniquePersistent<v8::Context>(_runtime->_isolate, context);

	v8::Local<v8::Object> global = context->Global();
	_global = _runtime->AddObject(context, global);
}

JSContext::~JSContext()
{
	size_t size = _functionMagicWrappers.size();
	for (size_t i = 0; i < size; ++i)
	{
		delete _functionMagicWrappers[i];
	}
	_runtime->FreeValue(_global);
	_context.Reset();
}

std::string JSContext::GetAtomString(JSAtom atom)
{
	v8::MaybeLocal<v8::Value> maybe_value = _runtime->GetValue(atom._value);
	v8::Local<v8::Value> value;
	if (maybe_value.ToLocal(&value) && value->IsString())
	{
		value->ToString(_context.Get(_runtime->_isolate));
	}
	return std::string();
}

JSValue JSContext::GetAtomValue(JSAtom atom)
{
	v8::MaybeLocal<v8::Value> maybe_value = _runtime->GetValue(atom._value);
	v8::Local<v8::Value> value;
	if (maybe_value.ToLocal(&value) && value->IsString())
	{
		return JS_MKPTR(JS_TAG_STRING, (size_t)atom._value);
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

JSValue JSContext::NewArray()
{
	v8::Local<v8::Array> array_value = v8::Array::New(GetIsolate());
	return _runtime->AddObject(Get(), array_value);
}

JSValue JSContext::NewObject()
{
	v8::Local<v8::Object> object_value = v8::Object::New(GetIsolate());
	return _runtime->AddObject(Get(), object_value);
}

v8::MaybeLocal<v8::Object> JSContext::NewObjectProtoClass(v8::Local<v8::Object> new_target, JSClassDef* classDef)
{
	v8::Local<v8::Context> context = Get();
	v8::Local<v8::FunctionTemplate> _class = classDef->_class.Get(GetIsolate());
	v8::Local<v8::String> prototype_str = _runtime->GetAtomValue(JS_ATOM_prototype).ToLocalChecked();
	v8::MaybeLocal<v8::Value> prototype_maybe = new_target->Get(context, prototype_str);
	v8::Local<v8::Value> prototype_value;
	if (prototype_maybe.ToLocal(&prototype_value) && prototype_value->IsObject())
	{
		v8::Local<v8::Object> prototype = v8::Local<v8::Object>::Cast(prototype_value);
		v8::Local<v8::Object> instance = _class->GetFunction(context).ToLocalChecked()->NewInstance(context).ToLocalChecked();
		instance->SetPrototype(context, prototype).Check();
		return instance;
	}

	//TODO throw exception here
	return {};
}

JSValue JSContext::NewCFunctionMagic(JSCFunctionMagic* func, JSAtom atom, int length, JSCFunctionEnum cproto, int magic)
{
	JSCFunctionMagicWrapper* wrapper = new JSCFunctionMagicWrapper{ this, {._funcMagic = func}, magic };
	_functionMagicWrappers.push_back(wrapper);
	v8::Local<v8::External> data = v8::External::New(_runtime->_isolate, wrapper);
	
	switch (cproto)
	{
		case JS_CFUNC_constructor_magic:
		{
			v8::Local<v8::Context> context = _context.Get(_runtime->_isolate);
			v8::Local<v8::FunctionTemplate> func_t = v8::FunctionTemplate::New(_runtime->_isolate, JSCFunctionConstructorMagicCallback, data, v8::Local<v8::Signature>(), length);
			func_t->InstanceTemplate()->SetInternalFieldCount(1);
			//func_t->Inherit()
			v8::MaybeLocal<v8::Function> func_o = func_t->GetFunction(context);
			v8::Local<v8::Function> func_v;
			//func_o->SetPrototype()
			if (func_o.ToLocal(&func_v))
			{
				return _runtime->AddObject(context, func_v);
			}
			break;
		}
		case JS_CFUNC_generic_magic:
		{
			v8::MaybeLocal<v8::Function> func_o = v8::Function::New(Get(), JSCFunctionMagicCallback, data, length);
			v8::Local<v8::Function> func_v;
			if (func_o.ToLocal(&func_v))
			{
				v8::Local<v8::Context> context = _context.Get(_runtime->_isolate);
				return _runtime->AddObject(context, func_v);
			}
			break;
		}
		case JS_CFUNC_getter_magic:
		{
			v8::MaybeLocal<v8::Function> func_o = v8::Function::New(Get(), JSCFunctionGetterMagicCallback, data, length);
			v8::Local<v8::Function> func_v;
			if (func_o.ToLocal(&func_v))
			{
				v8::Local<v8::Context> context = _context.Get(_runtime->_isolate);
				return _runtime->AddObject(context, func_v);
			}
			break;
		}
		case JS_CFUNC_setter_magic:
		{
			v8::MaybeLocal<v8::Function> func_o = v8::Function::New(Get(), JSCFunctionSetterMagicCallback, data, length);
			v8::Local<v8::Function> func_v;
			if (func_o.ToLocal(&func_v))
			{
				v8::Local<v8::Context> context = _context.Get(_runtime->_isolate);
				return _runtime->AddObject(context, func_v);
			}
			break;
		}
		default: 
		{
			//TODO throw exception: unsupported function enum
			break;
		}
	}

	//TODO throw exception here
	return JS_UNDEFINED;
	
}

JSValue JSContext::NewCFunction(JSCFunction* func, JSAtom atom, int length, JSCFunctionEnum cproto)
{
	JSCFunctionMagicWrapper* wrapper = new JSCFunctionMagicWrapper{ this, {._func = func} };
	_functionMagicWrappers.push_back(wrapper);
	v8::Local<v8::External> data = v8::External::New(_runtime->_isolate, wrapper);

	switch (cproto)
	{
	case JS_CFUNC_generic:
	{
		v8::MaybeLocal<v8::Function> func_o = v8::Function::New(Get(), JSCFunctionCallback, data, length);
		v8::Local<v8::Function> func_v;
		if (func_o.ToLocal(&func_v))
		{
			v8::Local<v8::Context> context = _context.Get(_runtime->_isolate);
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
			v8::Local<v8::Context> context = _context.Get(_runtime->_isolate);
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
			v8::Local<v8::Context> context = _context.Get(_runtime->_isolate);
			return _runtime->AddObject(context, func_v);
		}
		break;
	}
	default:
	{
		//TODO throw exception: unsupported function enum
		break;
	}
	}

	//TODO throw exception here
	return JS_UNDEFINED;
}

int JSContext::SetPropertyInternal(JSValueConst this_obj, JSAtom prop, JSValue val, int flags)
{
	if (this_obj.tag == JS_TAG_OBJECT)
	{
		v8::MaybeLocal<v8::Value> maybe_value = _runtime->GetValue(this_obj.u.ptr);
		v8::Local<v8::Value> value;
		if (maybe_value.ToLocal(&value) && value->IsObject())
		{
			v8::MaybeLocal<v8::String> pkey = _runtime->GetAtomValue(prop);
			if (!pkey.IsEmpty())
			{
				v8::Local<v8::Object> obj = v8::Local<v8::Object>::Cast(value);
				v8::MaybeLocal<v8::Value> maybe_pvalue = _runtime->GetValue(val);
				v8::Local<v8::Value> pvalue;
				if (maybe_pvalue.ToLocal(&pvalue))
				{
					obj->Set(_context.Get(_runtime->_isolate), pkey.ToLocalChecked(), pvalue);
					_runtime->FreeValue(val);
					return 1;
				}
			}
		}
	}
	_runtime->FreeValue(val);
	return 0;
}

JSValue JSContext::Eval(const char* input, size_t input_len, const char* filename, int eval_flags)
{
	v8::Isolate* isolate = _runtime->_isolate;
	v8::Local<v8::Context> context = _context.Get(isolate);

	v8::TryCatch try_catch(isolate);
	v8::MaybeLocal<v8::String> source = v8::String::NewFromUtf8(isolate, input, v8::NewStringType::kNormal, (int)input_len);
	v8::MaybeLocal<v8::Script> script = v8::Script::Compile(context, source.ToLocalChecked());
	if (!script.IsEmpty())
	{
		v8::MaybeLocal<v8::Value> maybe_value = script.ToLocalChecked()->Run(context);
		if (try_catch.HasCaught())
		{
			return _runtime->ThrowException(context, try_catch.Exception());
		}
		else
		{
			v8::Local<v8::Value> value;
			if (maybe_value.ToLocal(&value))
			{
				return _runtime->AddValue(context, value);
			}
			return JS_UNDEFINED;
		}
	}

	if (try_catch.HasCaught())
	{
		//TODO 专门定义一个 UniquePersistent for exception
		return _runtime->ThrowException(context, try_catch.Exception());
	}

	return _runtime->ThrowException(context, "failed to compile");
}
