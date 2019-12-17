#include "quickjs.h"
#include "jsb.h"

JSB_EXTERNAL void init() {
    JSRuntime *rt = JS_NewRuntime();
    JS_FreeRuntime(rt);
}

JSB_EXTERNAL int test(int a, int b) {
    return a + b;
}
