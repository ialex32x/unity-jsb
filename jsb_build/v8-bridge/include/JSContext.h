#pragma once

#include <v8.h>
#include <string>
#include <map>
#include "JSRuntime.h"
#include "QuickJSCompatible.h"

struct JSContext
{
	struct JSCFunctionMagicWrapper
	{
		JSContext* _context;
		union {
			JSCFunctionMagic* _funcMagic;
			JSCFunction* _func;

			JSCFunctionSetterMagic* _setterMagic;
			JSCFunctionSetter* _setter;

			JSCFunctionGetterMagic* _getterMagic;
			JSCFunctionGetter* _getter;
		};
		int _magic;
	};

	void* _opaque = nullptr;
	JSRuntime* _runtime = nullptr;
	v8::UniquePersistent<v8::Context> _context;
	
	JSValue _global;
	JSValue _emptyString;

	JSContext(JSRuntime* runtime);
	~JSContext();

	std::string GetAtomString(JSAtom atom);
	JSValue AtomToValue(JSAtom atom);

	v8::Local<v8::Context> Get();
	v8::Isolate* GetIsolate();
	JSValue GetGlobal();
	JSValue GetEmptyString();

	JSValue NewObject();
	JSValue NewArray();

	JSValue NewObjectProtoClass(v8::Local<v8::Object> new_target, JSClassDef* classDef);

	JSValue NewCFunctionMagic(JSCFunctionMagic* func, JSAtom atom, int length, JSCFunctionEnum cproto, int magic);
	JSValue NewCFunction(JSCFunction* func, JSAtom atom, int length, JSCFunctionEnum cproto);

	JSValue GetPropertyStr(JSValueConst this_obj, const char* prop);
	JSValue GetProperty(JSValueConst this_obj, JSAtom prop);
	JSValue GetPropertyUint32(JSValueConst this_obj, uint32_t idx);
	JSValue GetPropertyInternal(JSValueConst this_obj, JSAtom prop, JSValueConst receiver, JS_BOOL throw_ref_error);
	int HasProperty(JSValueConst this_obj, JSAtom prop);
	// will decrease the reference count of val 
	// return TRUE/FALSE or -1 if exception
	int SetPropertyUint32(JSValueConst this_obj, uint32_t idx, JSValue val);
	// will decrease the reference count of val 
	// return TRUE/FALSE or -1 if exception
	int SetPropertyInternal(JSValueConst this_obj, JSAtom prop, JSValue val, int flags);
	int DefineProperty(JSValueConst this_obj, JSAtom prop, JSValueConst getter, JSValueConst setter, int flags);
	int DefinePropertyValue(JSValueConst this_obj, JSAtom prop, JSValue val, int flags);

	JSValue Eval(const char* input, size_t input_len, const char* filename, int eval_flags);
	JSValue CallConstructor(JSValueConst func_obj, int argc, JSValueConst* argv);
	JSValue Call(JSValueConst func_obj, JSValueConst this_obj, int argc, JSValueConst* argv);

	JSValue NewPromiseCapability(JSValue* resolving_funcs);

private:
	std::vector<JSCFunctionMagicWrapper*> _functionMagicWrappers;
};
