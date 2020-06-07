using AOT;
using QuickJS;
using QuickJS.Binding;
using QuickJS.Native;
using QuickJS.Utils;
using UnityEngine;

namespace jsb
{
    public struct SValue
    {
    }

    public class SValueBinding : Values
    {
        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        private static JSValue BindConstructor(JSContext ctx, JSValue new_target, int argc, JSValue[] argv, int magic)
        {
            JSValue obj = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(float) * 3);
            JSApi.jsb_set_float_3(obj, 1f, 2f, 3f);
            return obj;
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        private static JSValue BindTest(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            var rt = ScriptEngine.GetRuntime(ctx);
            float x, y, z;
            JSApi.jsb_get_float_3(this_obj, out x, out y, out z);
            JSApi.jsb_set_float_3(this_obj, 1f + x, 1f + y, 1f + z);
            Debug.LogFormat("SValue.Test: {0}, {1}, {2}", x, y, z);
            return JSApi.JS_UNDEFINED;
        }
        
        public static void Bind(TypeRegister register)
        {
            var ns = register.CreateNamespace("jsb");
            var cls = ns.CreateClass("SValue", typeof(SValue), BindConstructor);
            cls.AddMethod(false, "Test", BindTest, 0);
            cls.Close();
            ns.Close();
        }
    }
}
