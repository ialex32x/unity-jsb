#include "quickjs.h"
#include "jsb.h"

#include <stdlib.h>
#include <stdio.h>

JSB_EXTERNAL struct JSBVM *JSB_NewVM() {
    struct JSBVM *vm = malloc(sizeof(struct JSBVM));
    vm->rt = JS_NewRuntime();
    vm->ctx = JS_NewContext(vm->rt);
    return vm;
}

JSB_EXTERNAL void JSB_FreeVM(struct JSBVM *vm) {
    if (vm) {
        JS_FreeRuntime(vm->rt);
        vm->rt = NULL;
    }
}

JSB_EXTERNAL struct JSBClass *JSB_NewClass(struct JSBVM *vm, const char *name, JSClassFinalizer *finalizer) {
    struct JSBClass *clz = js_malloc(vm->ctx, sizeof(struct JSBClass));

    clz->class_def.finalizer = finalizer;
    // JS_NewCFunction2(vm->ctx, constructor, name, )
    JS_NewClassID(&clz->class_id);
    JS_NewClass(vm->rt, clz->class_id, &clz->class_def);
    return clz;
}
