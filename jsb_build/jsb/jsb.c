#include "quickjs.h"
#include "jsb.h"

#include <stdlib.h>
#include <stdio.h>

static struct JSBRuntime *_runtimes = NULL;

static struct JSBRuntime *jsb_get_runtime(JSRuntime *rt) {
    struct JSBRuntime *brt = _runtimes;
    while (brt) {
        if (brt->rt == rt) {
            return brt;
        }
        brt = brt->_next;
    }
    return NULL;
}

static struct JSBRuntime *jsb_remove_runtime(JSRuntime *rt) {
    struct JSBRuntime *brt = _runtimes;
    struct JSBRuntime *prev = NULL;
    while (brt) {
        if (brt->rt == rt) {
            if (prev) {
                prev->_next = brt->_next;
            }
            brt->_next = NULL;
            return brt;
        }
        prev = brt;
        brt = brt->_next;
    }
    return NULL;
}

// static JSValue object_constructor(JSContext *ctx, JSValueConst this_val, int argc, JSValueConst *argv) {
// }

static void origin_finalizer(JSRuntime *rt, JSValue val) {
    struct JSBRuntime *brt = jsb_get_runtime(rt);
    struct JSBValue *bval = JS_GetOpaque(val, brt->origin.class_id);
    if (bval) {
        brt->finalizer(brt, bval);
    }
}

JSB_EXTERNAL struct JSBRuntime *JSB_NewRuntime(JSBClassFinalizer *finalizer) {
    struct JSBRuntime *vm = malloc(sizeof(struct JSBRuntime));
    vm->_next = _runtimes;
    _runtimes = vm;

    vm->rt = JS_NewRuntime();
    vm->ctx = JS_NewContext(vm->rt);
    vm->finalizer = finalizer;
    vm->origin.class_id = 0;
    vm->origin.class_def.class_name = "JSBOrigin";
    vm->origin.class_def.finalizer = origin_finalizer;
    return vm;
}

JSB_EXTERNAL void JSB_FreeRuntime(struct JSBRuntime *brt) {
    if (brt) {
        jsb_remove_runtime(brt->rt);
        JS_FreeRuntime(brt->rt);
        brt->rt = NULL;
    }
}

JSB_EXTERNAL struct JSBClass *JSB_NewClass(struct JSBRuntime *vm, const char *name) {
    struct JSBClass *clz = js_malloc(vm->ctx, sizeof(struct JSBClass));

    // clz->class_def.finalizer = finalizer;
    // JS_NewCFunction(vm->ctx, object_constructor, name, 1);
    JS_NewClassID(&clz->class_id);
    JS_NewClass(vm->rt, clz->class_id, &clz->class_def);
    return clz;
}
