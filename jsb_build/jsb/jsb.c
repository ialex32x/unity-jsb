#include "quickjs.h"
#include "jsb.h"

#include <stdlib.h>
#include <stdio.h>

static struct JSBVM *_vms = NULL;

static struct JSBVM *jsb_get_vm(JSRuntime *rt) {
    struct JSBVM *vm = _vms;
    while (vm) {
        if (vm->rt == rt) {
            return vm;
        }
        vm = vm->_next;
    }
    return NULL;
}

static void finalizer(JSRuntime *rt, JSValue val) {
    struct JSBVM *vm = jsb_get_vm(rt);
    struct JSBValue *bval = JS_GetOpaque(val, vm->origin.class_id);
    if (bval) {
        bval->finalizer(vm, bval);
    }
}

JSB_EXTERNAL struct JSBVM *JSB_NewVM() {
    struct JSBVM *vm = malloc(sizeof(struct JSBVM));
    vm->_next = _vms;
    _vms = vm;

    vm->rt = JS_NewRuntime();
    vm->ctx = JS_NewContext(vm->rt);
    vm->origin.class_id = 0;
    vm->origin.class_def.class_name = "JSBOrigin";
    vm->origin.class_def.finalizer = finalizer;
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
