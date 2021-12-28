#pragma once

/*
 * Î»ÖÃ ./Packages/cc.starlessnight.unity-jsb/Source/Native
 * ËÑË÷ extern
 * ÕýÔÚ±à¼­ JS_HasProperty
 */

#include "QuickJSCompatible.h"

#ifdef __cplusplus
extern "C" {
#endif

struct JSContext;
struct JSRuntime;

#define DEF(name, str) \
JS_EXPORT JSAtom JSB_ATOM_##name();
#include "quickjs-atom.h"
#undef DEF

JS_EXPORT void* js_malloc(JSContext* ctx, size_t size);
JS_EXPORT void* js_mallocz(JSContext* ctx, size_t size);
JS_EXPORT void js_free(JSContext* ctx, void* ptr);
JS_EXPORT IntPtr js_strndup(JSContext* ctx, const char* s, size_t n);

//JS_EXPORT JSValue JS_GetImportMeta(JSContext ctx, JSModuleDef m);

JS_EXPORT void JS_AddIntrinsicOperators(JSContext* ctx);
JS_EXPORT void JS_RunGC(JSRuntime* rt);

JS_EXPORT int JSB_Init();
JS_EXPORT JSClassID JSB_GetBridgeClassID();

JS_EXPORT JSRuntime* JSB_NewRuntime(JSGCObjectFinalizer* finalizer);
JS_EXPORT int JSB_FreeRuntime(JSRuntime* rt);
JS_EXPORT void* JSB_GetRuntimeOpaque(JSRuntime* rt);
JS_EXPORT void JSB_SetRuntimeOpaque(JSRuntime* rt, void* opaque);
JS_EXPORT void* JS_GetContextOpaque(JSContext* ctx);
JS_EXPORT void JS_SetContextOpaque(JSContext* ctx, void* opaque);
JS_EXPORT JSContext* JS_NewContext(JSRuntime* rt);
JS_EXPORT void JS_FreeContext(JSContext* ctx);
JS_EXPORT JSRuntime* JS_GetRuntime(JSContext* ctx);
JS_EXPORT JSPayloadHeader JSB_FreePayload(JSContext* ctx, JSValue val);

JS_EXPORT JSValue JSB_DupValue(JSContext* ctx, JSValue val);
JS_EXPORT void JSB_FreeValue(JSContext* ctx, JSValue val);
JS_EXPORT void JSB_FreeValueRT(JSRuntime* rt, JSValue val);

JS_EXPORT JSAtom JS_NewAtomLen(JSContext* ctx, const char* str, size_t len);
JS_EXPORT JSAtom JS_DupAtom(JSContext* ctx, JSAtom v);
JS_EXPORT void JS_FreeAtom(JSContext* ctx, JSAtom v);
JS_EXPORT JSValue JS_AtomToString(JSContext* ctx, JSAtom atom);

JS_EXPORT int JS_ToBool(JSContext* ctx, JSValueConst val);
JS_EXPORT int JS_ToInt32(JSContext* ctx, int* pres, JSValue val);
JS_EXPORT int JS_ToInt64(JSContext* ctx, int64_t* pres, JSValue val);
JS_EXPORT int JS_ToFloat64(JSContext* ctx, double* pres, JSValueConst val);
JS_EXPORT int JS_ToBigInt64(JSContext* ctx, int64_t* pres, JSValue val);
JS_EXPORT int JS_ToIndex(JSContext* ctx, uint64_t* plen, JSValueConst val);
//WIP: JS_ToFloat64
JS_EXPORT const char* JS_ToCStringLen(JSContext* ctx, size_t* plen, JSValueConst val1);
JS_EXPORT const char* JS_ToCStringLen2(JSContext* ctx, size_t* plen, JSValueConst val1, JS_BOOL cesu8);
JS_EXPORT void JS_FreeCString(JSContext* ctx, const char* ptr);

JS_EXPORT int JS_IsInstanceOf(JSContext* ctx, JSValueConst val, JSValueConst obj);
JS_EXPORT JS_BOOL JS_IsException(JSValueConst val);
JS_EXPORT JSValue JS_GetException(JSContext* ctx);
JS_EXPORT JS_BOOL JS_IsError(JSContext* ctx, JSValueConst val);
JS_EXPORT JSValue JSB_ThrowError(JSContext* ctx, const char* buf, size_t buf_len);
JS_EXPORT JSValue JSB_ThrowTypeError(JSContext* ctx, const char* buf);
JS_EXPORT JSValue JSB_ThrowInternalError(JSContext* ctx, const char* buf);
JS_EXPORT JSValue JSB_ThrowRangeError(JSContext* ctx, const char* buf);
JS_EXPORT JSValue JSB_ThrowReferenceError(JSContext* ctx, const char* buf);

//JS_EXPORT JSClassID JSB_NewClass(JSRuntime* rt, JSClassID class_id, const char* class_name, JSGCObjectFinalizer* finalizer);

JS_EXPORT JSValue JS_NewObjectProtoClass(JSContext* ctx, JSValueConst proto, JSClassID class_id);

JS_EXPORT JSValue JSB_NewCFunction(JSContext* ctx, JSCFunction* func, JSAtom atom, int length, JSCFunctionEnum cproto, int magic);
JS_EXPORT JSValue JSB_NewCFunctionMagic(JSContext* ctx, JSCFunctionMagic* func, JSAtom atom, int length, JSCFunctionEnum cproto, int magic);
JS_EXPORT JSValue JS_GetGlobalObject(JSContext* ctx);
JS_EXPORT JSValue JS_NewObject(JSContext* ctx);
JS_EXPORT JSValue JSB_NewEmptyString(JSContext* ctx);
JS_EXPORT JSValue JS_NewString(JSContext* ctx, const char* str);
JS_EXPORT JSValue JS_NewStringLen(JSContext* ctx, const char* buf, size_t buf_len);
JS_EXPORT JSValue JSB_NewInt64(JSContext* ctx, int64_t val);
JS_EXPORT JSValue JSB_NewFloat64(JSContext* ctx, double d);
JS_EXPORT IntPtr JS_GetArrayBuffer(JSContext* ctx, size_t* psize, JSValueConst obj);
JS_EXPORT JSValue JS_NewArrayBufferCopy(JSContext* ctx, const char* buf, size_t len);

JS_EXPORT void* JSB_GetOpaque(JSContext* ctx, JSValue val, JSClassID class_id);
JS_EXPORT void JSB_SetOpaque(JSContext* ctx, JSValue val, void* data);

JS_EXPORT JSValue JS_NewArray(JSContext* ctx);
JS_EXPORT int JS_IsArray(JSContext* ctx, JSValueConst val);

JS_EXPORT JS_BOOL JS_IsFunction(JSContext* ctx, JSValueConst val);
JS_EXPORT JS_BOOL JS_IsConstructor(JSContext* ctx, JSValueConst val);

JS_EXPORT JSValue JS_GetPropertyStr(JSContext* ctx, JSValueConst this_obj, const char* prop);
JS_EXPORT JSValue JS_GetProperty(JSContext* ctx, JSValueConst this_obj, JSAtom prop);
JS_EXPORT JSValue JS_GetPropertyUint32(JSContext* ctx, JSValueConst this_obj, uint32_t idx);
JS_EXPORT JSValue JS_GetPropertyInternal(JSContext* ctx, JSValueConst this_obj, JSAtom prop, JSValueConst receiver, JS_BOOL throw_ref_error);
JS_EXPORT int JS_HasProperty(JSContext* ctx, JSValueConst this_obj, JSAtom prop);
JS_EXPORT int JS_SetPropertyUint32(JSContext* ctx, JSValueConst this_obj, uint32_t idx, JSValue val);
JS_EXPORT int JS_SetPropertyInternal(JSContext* ctx, JSValueConst this_obj, JSAtom prop, JSValue val, int flags);

JS_EXPORT int JS_DefineProperty(JSContext* ctx, JSValueConst this_obj, JSAtom prop, JSValueConst val, JSValueConst getter, JSValueConst setter, int flags); // val is unused
JS_EXPORT int JS_DefinePropertyValue(JSContext* ctx, JSValueConst this_obj, JSAtom prop, JSValue val, int flags);

JS_EXPORT int JS_SetPrototype(JSContext* ctx, JSValueConst obj, JSValueConst proto_val);
JS_EXPORT void JS_SetConstructor(JSContext* ctx, JSValueConst func_obj, JSValueConst proto);

JS_EXPORT JSValue JS_Eval(JSContext* ctx, const char* input, size_t input_len, const char* filename, int eval_flags);
JS_EXPORT JSValue JS_CallConstructor(JSContext* ctx, JSValueConst func_obj, int argc, JSValueConst* argv);
JS_EXPORT JSValue JS_Call(JSContext* ctx, JSValueConst func_obj, JSValueConst this_obj, int argc, JSValueConst* argv);
JS_EXPORT JSValue JS_Invoke(JSContext* ctx, JSValueConst this_val, JSAtom atom, int argc, JSValueConst* argv);

JS_EXPORT JSValue JSB_GetGlobalObject(JSContext* ctx);

JS_EXPORT JSValue JS_ParseJSON(JSContext* ctx, const char* buf, size_t buf_len, const char* filename);
JS_EXPORT JSValue JS_JSONStringify(JSContext* ctx, JSValueConst obj, JSValueConst replacer, JSValueConst space0);

JS_EXPORT JSValue JS_NewPromiseCapability(JSContext* ctx, JSValue* resolving_funcs);
JS_EXPORT void JS_SetHostPromiseRejectionTracker(JSRuntime* rt, JSHostPromiseRejectionTracker* cb, void* opaque);

JS_EXPORT int JS_ExecutePendingJob(JSRuntime* rt, JSContext** pctx);
JS_EXPORT void JS_SetInterruptHandler(JSRuntime* rt, JSInterruptHandler* cb, IntPtr opaque);
JS_EXPORT void JS_ComputeMemoryUsage(JSRuntime* rt, JSMemoryUsage* s);

// declaration only for local test
#if defined(JSB_EXEC_TEST)
JS_EXPORT JSValue JSB_NewBridgeClassObject(JSContext* ctx, JSValue new_target, int32_t object_id/*, int32_t type_id = JS_BO_OBJECT */);
JS_EXPORT JSValue jsb_construct_bridge_object(JSContext* ctx, JSValue proto, int32_t object_id);
#endif

#ifdef __cplusplus
}
#endif
