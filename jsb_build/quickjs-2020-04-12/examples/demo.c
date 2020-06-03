#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include "../quickjs.h"
#include "../quickjs-libc.h"

enum
{
    JS_ATOM_NULL,
#define DEF(name, str) JS_ATOM_##name,
#include "../quickjs-atom.h"
#undef DEF
    JS_ATOM_END,
};

static int running = 1;
static JSClassID unity_object_class_id;

static void foo_finalizer(JSRuntime *rt, JSValue val)
{
    void *data = JS_GetOpaque(val, unity_object_class_id);
    printf("unity_object_class.finalizer (%d)\n", (int)data);
}

static JSValue foo_constructor(JSContext *ctx, JSValueConst new_target, int argc, JSValueConst *argv)
{
    static int iii = 123;
    iii++;
    printf("foo.constructor\n");
    JSValue proto = JS_GetProperty(ctx, new_target, JS_ATOM_prototype);
    JSValue obj = JS_NewObjectProtoClass(ctx, proto, unity_object_class_id);
    JS_SetOpaque(obj, (void *)iii);
    JS_FreeValue(ctx, proto);
    // return new_target;
    return obj;
}

static JSValue goo_constructor(JSContext *ctx, JSValueConst new_target, int argc, JSValueConst *argv)
{
    printf("goo.constructor\n");
    JSValue proto = JS_GetProperty(ctx, new_target, JS_ATOM_prototype);
    JSValue obj = JS_NewObjectProtoClass(ctx, proto, unity_object_class_id);
    JS_SetOpaque(obj, (void *)123);
    JS_FreeValue(ctx, proto);
    // return new_target;
    return obj;
}

static char *read_file(const char *filename)
{
    FILE *fp = fopen(filename, "r");
    if (fp)
    {
        fseek(fp, 0, SEEK_END);
        long length = ftell(fp);
        fseek(fp, 0, SEEK_SET);
        char *buf = malloc(length + 1);
        memset(buf, 0, length + 1);
        fread(buf, length, 1, fp);
        fclose(fp);
        return buf;
    }
    return 0;
}

static void print_exception(JSContext *ctx, JSValueConst e)
{
    JSValue err_msg = JS_GetProperty(ctx, e, JS_ATOM_message);
    size_t len = 0;
    const char *msg_str = JS_ToCStringLen(ctx, &len, err_msg);
    printf("exception: %s\n", msg_str);
    JS_FreeCString(ctx, msg_str);
    JS_FreeValue(ctx, err_msg);
}

static JSValue js_quit(JSContext *ctx, JSValueConst this_val,
                       int argc, JSValueConst *argv)
{
    running = 0;
    return JS_UNDEFINED;
}

static JSValue js_print(JSContext *ctx, JSValueConst this_val,
                        int argc, JSValueConst *argv)
{
    int i;
    const char *str;
    size_t len;

    for (i = 0; i < argc; i++)
    {
        if (i != 0)
            putchar(' ');
        str = JS_ToCStringLen(ctx, &len, argv[i]);
        if (!str)
            return JS_EXCEPTION;
        fwrite(str, 1, len, stdout);
        JS_FreeCString(ctx, str);
    }
    putchar('\n');
    return JS_UNDEFINED;
}

static JSModuleDef *js_module_loader_test(JSContext *ctx,
                                          const char *module_name, void *opaque)
{
    printf("js_module_loader: %s\n", module_name);
    size_t buf_len;
    uint8_t *buf;
    JSModuleDef *m;
    JSValue func_val;

    buf = js_load_file(ctx, &buf_len, module_name);
    if (!buf)
    {
        JS_ThrowReferenceError(ctx, "could not load module filename '%s'",
                               module_name);
        return NULL;
    }

    /* compile the module */
    func_val = JS_Eval(ctx, (char *)buf, buf_len, module_name,
                       JS_EVAL_TYPE_MODULE | JS_EVAL_FLAG_COMPILE_ONLY);
    js_free(ctx, buf);
    if (JS_IsException(func_val))
        return NULL;
    /* the module is already referenced, so we must free it */
    m = JS_VALUE_GET_PTR(func_val);
    JS_FreeValue(ctx, func_val);
    return m;
}

void foo()
{
    JSRuntime *rt = JS_NewRuntime();
    JSContext *ctx = JS_NewContext(rt);

    JS_SetModuleLoaderFunc(rt, NULL, js_module_loader_test, NULL);

    js_init_module_std(ctx, "std");
    js_init_module_os(ctx, "os");

    JSClassID cls_id;
    unity_object_class_id = JS_NewClassID(&cls_id);

    JSClassDef cls_def;
    cls_def.class_name = "UnityObject";
    cls_def.finalizer = foo_finalizer;
    cls_def.exotic = NULL;
    cls_def.gc_mark = NULL;
    cls_def.call = NULL;
    JS_NewClass(rt, cls_id, &cls_def);

    JSValue global_obj = JS_GetGlobalObject(ctx);

    JS_SetPropertyStr(ctx, global_obj, "print", JS_NewCFunction(ctx, js_print, "print", 1));
    JS_SetPropertyStr(ctx, global_obj, "quit", JS_NewCFunction(ctx, js_quit, "quit", 0));

    JSValue foo_proto_val = JS_NewObject(ctx);
    JSValue foo_constructor_val = JS_NewCFunction2(ctx, foo_constructor, "Foo", 0, JS_CFUNC_constructor, 0);
    JS_SetConstructor(ctx, foo_constructor_val, foo_proto_val);
    JS_SetClassProto(ctx, cls_id, foo_proto_val);
    // JS_SetPrototype( __this_is_super_base_class__ );
    JS_DefinePropertyValueStr(ctx, global_obj, "Foo", foo_constructor_val, JS_PROP_ENUMERABLE | JS_PROP_CONFIGURABLE);

    JSValue goo_proto_val = JS_NewObject(ctx);
    JSValue goo_constructor_val = JS_NewCFunction2(ctx, goo_constructor, "Goo", 0, JS_CFUNC_constructor, 0);
    JS_SetConstructor(ctx, goo_constructor_val, goo_proto_val);
    JS_SetClassProto(ctx, cls_id, goo_proto_val);
    JS_SetPrototype(ctx, goo_proto_val, foo_proto_val);
    JS_DefinePropertyValueStr(ctx, global_obj, "Goo", goo_constructor_val, JS_PROP_ENUMERABLE | JS_PROP_CONFIGURABLE);

    char *source = read_file("./examples/demo.js");
    JSValue rval = JS_Eval(ctx, source, strlen(source), "eval", JS_EVAL_TYPE_MODULE | JS_EVAL_FLAG_STRICT);
    if (JS_IsException(rval))
    {
        JSValue e = JS_GetException(ctx);
        print_exception(ctx, e);
    }
    free(source);
    JS_FreeValue(ctx, rval);

    // JS_FreeValue(ctx, foo_proto_val);
    // JS_FreeValue(ctx, foo_constructor_val);
    JS_FreeValue(ctx, global_obj);

    while (running)
    {
        js_std_loop(ctx);
    }

    JS_RunGC(rt);
    JS_FreeContext(ctx);
    JS_FreeRuntime(rt);
}

int main()
{
    printf("demo running...\n");
    foo();
    fflush(stdout);
    return 0;
}
