#pragma once
#include <v8.h>
#include <map>
#include <string>
#include "v8impl.h"
#include "QuickJSCompatible.h"

//struct JSValueRef
//{
//	size_t _next = 0;
//	uint32_t _references = 1;
//	v8::Isolate* _isolate = nullptr;
//	v8impl::Persistent<v8::Value> _value;
//
//	JSValueRef() = default;
//	JSValueRef(v8::Isolate* isolate, v8::Local<v8::Value> value) : _isolate(isolate), _value(isolate, value) {}
//	JSValueRef(const JSValueRef& o) : _next(o._next), _references(o._references), _isolate(o._isolate), _value(o._isolate, o._value) {}
//};

struct JSValueRef
{
	size_t _next = 0;
	uint32_t _references = 1;
	v8impl::CopyPersistent<v8::Value> _value;

	JSValueRef() = default;
	JSValueRef(v8::Isolate* isolate, v8::Local<v8::Value> value) : _value(isolate, value) {}
};

struct JSAtomRef
{
	uint32_t _next = 0;
	uint32_t _references = 1;
	v8impl::CopyPersistent<v8::String> _value;

	JSAtomRef() = default;
	JSAtomRef(v8::Isolate * isolate, v8::Local<v8::String> value) : _value(isolate, value) {}
};

struct JSClassDef
{
	JSClassID _classID = 0;
	JSGCObjectFinalizer* _finalizer = nullptr;
	v8::Global<v8::FunctionTemplate> _class;
};

struct GCObject
{
	size_t _next = 0;
	size_t _index = 0; // self index
	JSClassDef* _classDef = nullptr;
	JSRuntime* _runtime = nullptr;
	v8::Global<v8::Object> _obj; // weak 
};

struct JSRuntime
{
	void* _opaque = nullptr;
	v8::Isolate* _isolate = nullptr;
	v8::ArrayBuffer::Allocator* _arrayBufferAllocator = nullptr;
	JSMallocFunctions malloc_functions;

	size_t _freeObjectSlot = 0;
	size_t _usedObjectSlot = 0;
	std::vector<JSValueRef> _objectRefs;

	uint32_t _freeAtomSlot = 0;
	std::vector<JSAtomRef> _atomRefs;
	std::map<std::string, JSAtom> _atoms;

	JSRuntime();
	~JSRuntime();

	JS_BOOL Release();

	JSValue ThrowException(v8::Local<v8::Context> context, v8::Local<v8::Value> exception);
	JSValue ThrowException(v8::Local<v8::Context> context, const char* exception);

	JSValue DupValue(JSValue value);
	void FreeValue(JSValue value);

	JSValue AddValue(v8::Local<v8::Context> context, v8::Local<v8::Value> val);
	JSValue AddString(v8::Local<v8::Context> context, v8::Local<v8::String> val);
	JSValue AddSymbol(v8::Local<v8::Context> context, v8::Local<v8::Symbol> val);
	JSValue AddObject(v8::Local<v8::Context> context, v8::Local<v8::Object> val);
	JSValue AddGCValue(v8::Local<v8::Object> obj, JSClassDef* def);
	JSValue AddExceptionValue(v8::Local<v8::Context> context, v8::Local<v8::Value> val);
	v8::MaybeLocal<v8::Value> GetValue(size_t id);
	v8::MaybeLocal<v8::Value> GetValue(JSValue val);

	JSAtom GetAtom(std::string& val);
	JSAtom GetAtom(const char* val, size_t len);

	v8::MaybeLocal<v8::String> GetAtomValue(JSAtom atom);
	v8::MaybeLocal<v8::String> GetAtomValue(uint32_t atom_id);
	JSAtom DupAtom(JSAtom atom);
	void FreeAtom(JSAtom atom);

	JSClassID NewClass(JSClassID class_id, const char* class_name, JSGCObjectFinalizer* finalizer);
	JSClassDef* GetClassDef(JSClassID class_id);

	int ExecutePendingJob(JSContext** pctx);

	void RunGC();

private:
	size_t _freeGCObjectSlot = 0;
	std::vector<GCObject*> _gcObjects;
	static void OnGarbadgeCollectCallback(const v8::WeakCallbackInfo<GCObject>& info);

	std::map<JSClassID, JSClassDef*> _classes;
	size_t _AddValueInternal(v8::Local<v8::Context> context, v8::Local<v8::Value> val);

	v8::UniquePersistent<v8::Private> _PrivateCacheIndexKey;
};
