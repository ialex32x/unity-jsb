#include "quickjs.h"
#include "jsb.h"

#include <stdlib.h>
#include <stdio.h>

JSB_EXTERNAL JSRuntime *XJS_NewRuntime() {
    return JS_NewRuntime();
}

JSB_EXTERNAL void XJS_FreeRuntime(JSRuntime *rt) {
    JS_FreeRuntime(rt);
}

JSB_EXTERNAL JSContext *XJS_NewContext(JSRuntime *rt) {
    return JS_NewContext(rt);
}

JSB_EXTERNAL void XJS_FreeContext(JSContext *s) {
    JS_FreeContext(s);
}

JSB_EXTERNAL JSValue XJS_Eval(JSContext *ctx, const char *input, size_t input_len, const char *filename, int eval_flags) {
    return JS_Eval(ctx, input, input_len, filename, eval_flags);
}