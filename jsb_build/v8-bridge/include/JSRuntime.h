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
	size_t _next;

	// when references == 0 and gc_handle == 0, this slot will be recycled
	// if references == 0 and gc_handle != 0, it's still alive until the object is actually garbage collected by v8.
	//     * before gc, it's possible to resurrect this JSValue without changing it slot index
	uint32_t _references;
	size_t _gc_handle;
	v8impl::CopyPersistent<v8::Value> _value;

	JSValueRef() = default;
	JSValueRef(v8::Isolate* isolate, v8::Local<v8::Value> value) : _value(isolate, value), _references(1), _next(0), _gc_handle(0) {}
};

struct JSAtomRef
{
	uint32_t _next;
	uint32_t _references;
	v8impl::CopyPersistent<v8::String> _value;

	JSAtomRef() = default;
	JSAtomRef(v8::Isolate * isolate, v8::Local<v8::String> value) : _value(isolate, value), _references(1), _next(0) {}
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

	// reference to JSValue
	// (keep it alive without strong reference before GC)
	size_t _value_handle;
};

struct JSRuntime
{
	enum EInternalFieldName
	{
		EIFN_Payload,
		EIFN_FieldCount,
	};

	void* _opaque = nullptr;
	v8::Isolate* _isolate = nullptr;
	v8::ArrayBuffer::Allocator* _arrayBufferAllocator = nullptr;
private:
	JSMallocFunctions malloc_functions;
	v8::UniquePersistent<v8::Private> _PrivateCacheIndexKey;

public:
	JSRuntime(JSGCObjectFinalizer* finalizer);
	~JSRuntime();

	JS_BOOL Release();
	void ComputeMemoryUsage(JSMemoryUsage* memoryUsage);

	JSContext* _context;
	void SetContext(JSContext* context);
#pragma region JSValue
private:
	size_t _freeObjectSlot = 0;
	size_t _aliveObjectNum = 0;
	std::vector<JSValueRef> _objectRefs;

public:
	JSValue DupValue(JSValue value);
	void FreeValue(JSValue value);
	JSPayloadHeader FreePayload(JSValue value);

	v8::MaybeLocal<v8::Value> GetReferencedValue(size_t id);
	v8::MaybeLocal<v8::Value> GetValue(JSValue val);

	JSValue AddValue(v8::Local<v8::Context> context, v8::Local<v8::Value> val);
	JSValue AddString(v8::Local<v8::Context> context, v8::Local<v8::String> val);
	JSValue AddSymbol(v8::Local<v8::Context> context, v8::Local<v8::Symbol> val);
	JSValue AddObject(v8::Local<v8::Context> context, v8::Local<v8::Object> val);

	// register a finalization callback and AddObject at the same time
	JSValue AddGCObject(v8::Local<v8::Context> context, v8::Local<v8::Object> obj, JSClassDef* def);
private:
	size_t _AddValueInternal(v8::Local<v8::Context> context, v8::Local<v8::Value> val, size_t gc_handle = 0);
#pragma endregion
	
#pragma region Low Level Memory Management
public:
	void* mem_alloc(JSMallocState* s, size_t size);
	void mem_free(JSMallocState* s, void* ptr);
	void* mem_realloc(JSMallocState* s, void* ptr, size_t size);
	size_t mem_malloc_usable_size(const void* ptr);
#pragma endregion 

#pragma region JSAtom Management
private:
	uint32_t _freeAtomSlot = 0;
	uint32_t _aliveAtomNum = 0;
	std::vector<JSAtomRef> _atomRefs;
	std::map<std::string, JSAtom> _atoms;

public:
	JSAtom GetAtom(std::string& val);
	JSAtom GetAtom(const char* val, size_t len);

	v8::MaybeLocal<v8::String> GetAtomValue(JSAtom atom);
	JSAtom DupAtom(JSAtom atom);
	void FreeAtom(JSAtom atom);
#pragma endregion

#pragma region Class Management
	std::map<JSClassID, JSClassDef*> _classes;
	JSClassID NewClass(JSClassID class_id, const char* class_name, JSGCObjectFinalizer* finalizer);
	JSClassDef* GetClassDef(JSClassID class_id);
#pragma endregion

#pragma region Promise
	JSHostPromiseRejectionTracker* _promiseRejectionTracker;
	void* _promiseRejectionOpaque;

	void SetHostPromiseRejectionTracker(JSHostPromiseRejectionTracker* cb, void* opaque);
	int ExecutePendingJob(JSContext** pctx);
#pragma endregion

#pragma region GC 
	void RunGC();
#pragma endregion

#pragma region GCObject Management
private:
	size_t _freeGCObjectSlot = 0;
	size_t _aliveGCObjectNum = 0;
	std::vector<GCObject*> _gcObjects;
	static void OnGarbadgeCollectCallback(const v8::WeakCallbackInfo<GCObject>& info);
public:
#pragma endregion

#pragma region JSException
	JSValue ThrowException(v8::Local<v8::Context> context, v8::Local<v8::Value> exception);
	JSValue ThrowError(v8::Local<v8::Context> context, const char* exception, int len = -1);
	JSValue ThrowTypeError(v8::Local<v8::Context> context, const char* exception, int len = -1);

	v8::Global<v8::Value> _exception;
#pragma endregion

	friend struct JSContext;
};
