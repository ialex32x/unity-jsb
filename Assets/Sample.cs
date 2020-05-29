using System;
using System.IO;
using System.Text;
using AOT;
using QuickJS;
using QuickJS.Native;

namespace jsb
{
    using UnityEngine;

    public class Sample : MonoBehaviour
    {
        private static JSClassID class_id;
        
        [MonoPInvokeCallback(typeof(JSClassFinalizer))]
        static void finalizer(JSRuntime rt, JSValue value)
        {
            Debug.Log("finalizer");
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        static JSValue foo_ctor(JSContext ctx, JSValue new_target, int argc, JSValue[] argv)
        {
            Debug.Log("foo.ctor");
            var proto = JSApi.JS_GetProperty(ctx, new_target, JSApi.JS_ATOM_prototype);
            var obj = JSApi.JS_NewObjectProtoClass(ctx, proto, class_id);
            JSApi.JS_FreeValue(ctx, proto);
            return obj;
        }

        void Awake()
        {
            var rt = new ScriptEngine();
            var ctx = rt.NewContext();

            ctx.RegisterBuiltins();
            class_id = rt.NewClassID();
            JSApi.JS_NewClass(rt, class_id, "ManagedObject", finalizer);

            var global_object = ctx.GetGlobalObject();
            {
                var foo_proto_val = ctx.NewObject();
                var foo_ctor_val =
                    JSApi.JS_NewCFunction2(ctx, foo_ctor, "Foo", 0, JSCFunctionEnum.JS_CFUNC_constructor, 0);
                JSApi.JS_SetConstructor(ctx, foo_ctor_val, foo_proto_val);
                JSApi.JS_SetClassProto(ctx, class_id, foo_proto_val);
                JSApi.JS_DefinePropertyValueStr(ctx, global_object, "Foo", foo_ctor_val,
                    JSPropFlags.JS_PROP_ENUMERABLE | JSPropFlags.JS_PROP_CONFIGURABLE);
            }
            rt.FreeValue(global_object);

            var fileName = "Assets/test.js";
            var source = File.ReadAllText(fileName);
            var jsval = JSApi.JS_Eval(ctx, source, fileName);
            if (JSApi.JS_IsException(jsval))
            {
                ctx.print_exception();
            }
            rt.FreeValue(jsval);
            
            JSApi.JS_FreeContext(ctx);
            JSApi.JS_FreeRuntime(rt);
        }
    }
}