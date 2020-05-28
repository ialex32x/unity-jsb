using System;
using AOT;

namespace jsb
{
    using UnityEngine;

    public class Sample : MonoBehaviour
    {
        [MonoPInvokeCallback(typeof(JSClassFinalizer))]
        static void finalizer(IntPtr rt, JSValue value)
        {
            Debug.Log("finalizer");
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        static JSValue _print(IntPtr ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            Debug.Log("_print");
            return JSNative.JS_UNDEFINED;
        }

        void Awake()
        {
            Debug.Log(unchecked((ulong)-11));
            Debug.Log(unchecked((ulong)-1));
            var rt = JSNative.JS_NewRuntime();
            var ctx = JSNative.JS_NewContext(rt);

            var class_id = JSNative.JS_NewClassID();
            JSNative.JS_NewClass(rt, class_id, "Foo", finalizer);

            var global_object = JSNative.JS_GetGlobalObject(ctx);
            // var new_object = JSNative.JS_NewObject(ctx);
            JSNative.JS_SetPropertyStr(ctx, global_object, "print", JSNative.JS_NewCFunction(ctx, _print, "print", 1));

            JSNative.JS_FreeValue(ctx, global_object);

            var jsval = JSNative.JS_Eval(ctx, "print(123); 2+2", "eval");
            int rval;
            JSNative.JS_ToInt32(ctx, out rval, jsval);
            Debug.LogFormat("2+2={0}", rval);
            JSNative.JS_FreeContext(ctx);
            JSNative.JS_FreeRuntime(rt);
        }
    }
}