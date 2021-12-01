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

	JSContext(JSRuntime* runtime);
	~JSContext();

	std::string GetAtomString(JSAtom atom);
	JSValue GetAtomValue(JSAtom atom);

	v8::Local<v8::Context> Get();
	v8::Isolate* GetIsolate();
	JSValue GetGlobal();

	JSValue NewObject();
	JSValue NewArray();

	v8::MaybeLocal<v8::Object> NewObjectProtoClass(v8::Local<v8::Object> new_target, JSClassDef* classDef);

	JSValue NewCFunctionMagic(JSCFunctionMagic* func, JSAtom atom, int length, JSCFunctionEnum cproto, int magic);
	JSValue NewCFunction(JSCFunction* func, JSAtom atom, int length, JSCFunctionEnum cproto);

	int SetPropertyInternal(JSValueConst this_obj, JSAtom prop, JSValue val, int flags);
	JSValue Eval(const char* input, size_t input_len, const char* filename, int eval_flags);
private:
	std::vector<JSCFunctionMagicWrapper*> _functionMagicWrappers;
};
