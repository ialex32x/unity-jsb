#pragma once

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

#define JS_EXPORT __declspec(dllexport)

/* flags for object properties */
#define JS_PROP_CONFIGURABLE  (1 << 0)
#define JS_PROP_WRITABLE      (1 << 1)
#define JS_PROP_ENUMERABLE    (1 << 2)
#define JS_PROP_C_W_E         (JS_PROP_CONFIGURABLE | JS_PROP_WRITABLE | JS_PROP_ENUMERABLE)
#define JS_PROP_LENGTH        (1 << 3) /* used internally in Arrays */
#define JS_PROP_TMASK         (3 << 4) /* mask for NORMAL, GETSET, VARREF, AUTOINIT */
#define JS_PROP_NORMAL         (0 << 4)
#define JS_PROP_GETSET         (1 << 4)
#define JS_PROP_VARREF         (2 << 4) /* used internally */
#define JS_PROP_AUTOINIT       (3 << 4) /* used internally */

/* flags for JS_DefineProperty */
#define JS_PROP_HAS_SHIFT        8
#define JS_PROP_HAS_CONFIGURABLE (1 << 8)
#define JS_PROP_HAS_WRITABLE     (1 << 9)
#define JS_PROP_HAS_ENUMERABLE   (1 << 10)
#define JS_PROP_HAS_GET          (1 << 11)
#define JS_PROP_HAS_SET          (1 << 12)
#define JS_PROP_HAS_VALUE        (1 << 13)

/* throw an exception if false would be returned
   (JS_DefineProperty/JS_SetProperty) */
#define JS_PROP_THROW            (1 << 14)
   /* throw an exception if false would be returned in strict mode
	  (JS_SetProperty) */
#define JS_PROP_THROW_STRICT     (1 << 15)

#define JS_PROP_NO_ADD           (1 << 16) /* internal use */
#define JS_PROP_NO_EXOTIC        (1 << 17) /* internal use */

/* JS_Eval() flags */
#define JS_EVAL_TYPE_GLOBAL   (0 << 0) /* global code (default) */
#define JS_EVAL_TYPE_MODULE   (1 << 0) /* module code */
#define JS_EVAL_TYPE_DIRECT   (2 << 0) /* direct call (internal use) */
#define JS_EVAL_TYPE_INDIRECT (3 << 0) /* indirect call (internal use) */
#define JS_EVAL_TYPE_MASK     (3 << 0)

#define JS_EVAL_FLAG_STRICT   (1 << 3) /* force 'strict' mode */
#define JS_EVAL_FLAG_STRIP    (1 << 4) /* force 'strip' mode */
/* compile but do not run. The result is an object with a
   JS_TAG_FUNCTION_BYTECODE or JS_TAG_MODULE tag. It can be executed
   with JS_EvalFunction(). */
#define JS_EVAL_FLAG_COMPILE_ONLY (1 << 5)
/* don't include the stack frames before this eval in the Error() backtraces */
#define JS_EVAL_FLAG_BACKTRACE_BARRIER (1 << 6)

enum {
	/* all tags with a reference count are negative */
	JS_TAG_FIRST = -11, /* first negative tag */
	JS_TAG_BIG_DECIMAL = -11,
	JS_TAG_BIG_INT = -10,
	JS_TAG_BIG_FLOAT = -9,
	JS_TAG_SYMBOL = -8,
	JS_TAG_STRING = -7,
	JS_TAG_MODULE = -3, /* used internally */
	JS_TAG_FUNCTION_BYTECODE = -2, /* used internally */
	JS_TAG_OBJECT = -1,

	JS_TAG_INT = 0,
	JS_TAG_BOOL = 1,
	JS_TAG_NULL = 2,
	JS_TAG_UNDEFINED = 3,
	JS_TAG_UNINITIALIZED = 4,
	JS_TAG_CATCH_OFFSET = 5,
	JS_TAG_EXCEPTION = 6,
	JS_TAG_FLOAT64 = 7,
	/* any larger tag is FLOAT64 if JS_NAN_BOXING */
};

enum JSCFunctionEnum {  /* XXX: should rename for namespace isolation */
	JS_CFUNC_generic,
	JS_CFUNC_generic_magic,
	JS_CFUNC_constructor,
	JS_CFUNC_constructor_magic,
	JS_CFUNC_constructor_or_func,
	JS_CFUNC_constructor_or_func_magic,
	JS_CFUNC_f_f,
	JS_CFUNC_f_f_f,
	JS_CFUNC_getter,
	JS_CFUNC_setter,
	JS_CFUNC_getter_magic,
	JS_CFUNC_setter_magic,
	JS_CFUNC_iterator_next,
};

typedef struct JSMemoryUsage {
	int64_t malloc_size, malloc_limit, memory_used_size;
	int64_t malloc_count;
	int64_t memory_used_count;
	int64_t atom_count, atom_size;
	int64_t str_count, str_size;
	int64_t obj_count, obj_size;
	int64_t prop_count, prop_size;
	int64_t shape_count, shape_size;
	int64_t js_func_count, js_func_size, js_func_code_size;
	int64_t js_func_pc2line_count, js_func_pc2line_size;
	int64_t c_func_count, array_count;
	int64_t fast_array_count, fast_array_elements;
	int64_t binary_object_count, binary_object_size;
} JSMemoryUsage;

typedef union JSValueUnion
{
	int32_t int32;
	double float64;
	size_t ptr;
} JSValueUnion;

typedef struct JSValue
{
	JSValueUnion u = { 0 };
	int64_t tag = 0;
} JSValue;

typedef uint32_t JSAtom;

typedef struct JSMallocState {
	size_t malloc_count;
	size_t malloc_size;
	size_t malloc_limit;
	void* opaque; /* user opaque */
} JSMallocState;

typedef struct JSMallocFunctions {
	void* (*js_malloc)(JSMallocState* s, size_t size);
	void (*js_free)(JSMallocState* s, void* ptr);
	void* (*js_realloc)(JSMallocState* s, void* ptr, size_t size);
	size_t(*js_malloc_usable_size)(const void* ptr);
} JSMallocFunctions;

struct JSContext;
struct JSRuntime;

typedef void* IntPtr;
typedef int JS_BOOL;

typedef uint32_t JSClassID;

typedef JSValue JSValueConst;

typedef JSValue JSCFunction(JSContext* ctx, JSValueConst this_val, int argc, JSValueConst* argv);
typedef JSValue JSCFunctionMagic(JSContext* ctx, JSValueConst this_val, int argc, JSValueConst* argv, int magic);

typedef JSValue JSCFunctionSetter(JSContext* ctx, JSValueConst this_val, JSValueConst val);
typedef JSValue JSCFunctionSetterMagic(JSContext* ctx, JSValueConst this_val, JSValueConst val, int magic);

typedef JSValue JSCFunctionGetter(JSContext* ctx, JSValueConst this_val);
typedef JSValue JSCFunctionGetterMagic(JSContext* ctx, JSValueConst this_val, int magic);

typedef void JSClassFinalizer(JSRuntime* rt, JSValue val);
typedef int JSInterruptHandler(JSRuntime* rt, IntPtr opaque);

#define JS_BO_TYPE 1
#define JS_BO_OBJECT 2
#define JS_BO_VALUE 3

typedef struct JSPayloadHeader
{
	int32_t type_id; // JS_BO_*
	int32_t value;
} JSPayloadHeader;

static JSPayloadHeader _null_payload = { .type_id = 0, .value = 0 };

typedef struct JSPayload
{
	JSPayloadHeader header;
	char data[1];
} JSPayload;

typedef void JSGCObjectFinalizer(JSRuntime* rt, JSPayloadHeader val);
typedef void JSHostPromiseRejectionTracker(JSContext* ctx, JSValueConst promise, JSValueConst reason, JS_BOOL is_handled, void* opaque);

#ifndef TRUE
#define TRUE 1
#define FALSE 0
#endif

#define JS_TAG_IS_BYREF(tag) (tag) < 0

//#define JS_MKVAL() JSValue{}
#define JS_MKINT32(tag, v) JSValue{{.int32=(v)},(tag)}
#define JS_MKFLOAT64(tag, v) JSValue{{.float64=(v)},(tag)}
#define JS_MKPTR(tag, v) JSValue{{.ptr=(v)},(tag)}

#define JS_UNDEFINED JSValue{ {0}, JS_TAG_UNDEFINED }
#define JS_NULL JSValue{ {0}, JS_TAG_NULL }
#define JS_EXCEPTION JSValue { {0}, JS_TAG_EXCEPTION }

#if !defined(FORCEINLINE)
#define FORCEINLINE __forceinline
#endif

#if !defined(JSB_DEBUG)
#define JSB_DEBUG 1
#endif

#define JS_ATOM_NULL 0

#define JS_CONTEXT_DATA_SELF 1

#define JS_ISOLATE_DATA_SELF 0

enum
{
	__JS_ATOM_NULL = JS_ATOM_NULL,
#define DEF(name, str) JS_ATOM_##name,
#define SSR
#include "quickjs-atom.h"
#undef DEF
	JS_ATOM_END,
};

#ifdef __cplusplus
}
#endif
