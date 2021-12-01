#pragma once

#include <v8.h>
#include "QuickJSCompatible.h"
#include "JSRuntime.h"
#include "JSContext.h"

#ifdef __cplusplus
extern "C" {
#endif

#define DEF(name, str) \
V8_EXPORT JSAtom JSB_ATOM_##name();
#include "quickjs-atom.h"
#undef DEF

V8_EXPORT void JS_AddIntrinsicOperators(JSContext* ctx);

V8_EXPORT void JS_RunGC(JSRuntime* rt);

V8_EXPORT int __JSB_Init();

V8_EXPORT JSRuntime* JS_NewRuntime();

V8_EXPORT int JS_FreeRuntime(JSRuntime* rt);

V8_EXPORT void* JS_GetRuntimeOpaque(JSRuntime* rt);

V8_EXPORT void JS_SetRuntimeOpaque(JSRuntime* rt, void* opaque);

V8_EXPORT void* JS_GetContextOpaque(JSContext* ctx);

V8_EXPORT void JS_SetContextOpaque(JSContext* ctx, void* opaque);

V8_EXPORT JSContext* JS_NewContext(JSRuntime* rt);

V8_EXPORT void JS_FreeContext(JSContext* ctx);

V8_EXPORT JSRuntime* JS_GetRuntime(JSContext* ctx);

V8_EXPORT JSValue JSB_DupValue(JSContext* ctx, JSValue val);

V8_EXPORT void JSB_FreeValue(JSContext* ctx, JSValue val);

V8_EXPORT void JSB_FreeValueRT(JSRuntime* rt, JSValue val);

V8_EXPORT JSAtom JS_NewAtomLen(JSContext* ctx, const char* str, size_t len);

V8_EXPORT JSAtom JS_DupAtom(JSContext* ctx, JSAtom v);

V8_EXPORT void JS_FreeAtom(JSContext* ctx, JSAtom v);

V8_EXPORT JSValue JS_AtomToString(JSContext* ctx, JSAtom atom);

V8_EXPORT const char* JS_ToCStringLen2(JSContext* ctx, size_t* plen, JSValueConst val1, JS_BOOL cesu8);
V8_EXPORT void JS_FreeCString(JSContext* ctx, const char* ptr);

V8_EXPORT int JS_IsInstanceOf(JSContext* ctx, JSValueConst val, JSValueConst obj);

V8_EXPORT JSClassID JSB_NewClass(JSRuntime* rt, JSClassID class_id, const char* class_name, JSGCObjectFinalizer* finalizer);

V8_EXPORT JSValue JS_NewObjectProtoClass(JSContext* ctx, JSValueConst proto, JSClassID class_id);

V8_EXPORT JSValue JSB_NewCFunction(JSContext* ctx, JSCFunction* func, JSAtom atom, int length, JSCFunctionEnum cproto, int magic);

V8_EXPORT JSValue JSB_NewCFunctionMagic(JSContext* ctx, JSCFunctionMagic* func, JSAtom atom, int length, JSCFunctionEnum cproto, int magic);

V8_EXPORT JSValue JS_NewObject(JSContext* ctx);

V8_EXPORT JSValue JS_NewArray(JSContext* ctx);

V8_EXPORT int JS_IsArray(JSContext* ctx, JSValueConst val);

V8_EXPORT JS_BOOL JS_IsFunction(JSContext* ctx, JSValueConst val);

V8_EXPORT JS_BOOL JS_IsConstructor(JSContext* ctx, JSValueConst val);

V8_EXPORT int JS_SetPropertyInternal(JSContext* ctx, JSValueConst this_obj, JSAtom prop, JSValue val, int flags);

V8_EXPORT int JS_SetPrototype(JSContext* ctx, JSValueConst obj, JSValueConst proto_val);

V8_EXPORT JSValue JS_Eval(JSContext* ctx, const char* input, size_t input_len, const char* filename, int eval_flags);

V8_EXPORT JSValue JSB_GetGlobalObject(JSContext* ctx);

V8_EXPORT JSValue JS_ParseJSON(JSContext* ctx, const char* buf, size_t buf_len, const char* filename);
V8_EXPORT JSValue JS_JSONStringify(JSContext* ctx, JSValueConst obj, JSValueConst replacer, JSValueConst space0);


V8_EXPORT int JS_ExecutePendingJob(JSRuntime* rt, JSContext** pctx);

#ifdef __cplusplus
}
#endif
