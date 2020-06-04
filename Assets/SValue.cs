using AOT;
using QuickJS;
using QuickJS.Binding;
using QuickJS.Native;
using QuickJS.Utils;
using UnityEngine;

namespace jsb
{
    public class Vector3Binding : Values
    {
        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        private static JSValue BindConstructor(JSContext ctx, JSValue new_target, int argc, JSValue[] argv, int magic)
        {
            var rt = ScriptEngine.GetRuntime(ctx);
            var cache = rt.GetObjectCache();
            JSValue proto = JSApi.JS_GetProperty(ctx, new_target, JSApi.JS_ATOM_prototype);
            JSValue obj = JSApi.JS_NewObjectProtoClass(ctx, proto, rt._def_struct_id);
            var sv = JSApi.JSB_NewStructPayload(ctx, obj, rt._def_struct_id, 0, 0, sizeof(float) * 3);
            JSApi.jsb_set_float_3(sv, 1f, 2f, 3f);
            JSApi.JS_FreeValue(ctx, proto);
            return obj;
        }

        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        private static JSValue BindTest(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv, int magic)
        {
            var rt = ScriptEngine.GetRuntime(ctx);
            var cache = rt.GetObjectCache();
            var payload = JSApi.jsb_get_payload(this_obj, rt._def_struct_id);

            float x, y, z;
            JSApi.jsb_get_float_3(payload, out x, out y, out z);
            JSApi.jsb_set_float_3(payload, 1f + x, 1f + y, 1f + z);
            Debug.LogFormat("Vector3.Test: {0}, {1}, {2}", x, y, z);
            return JSApi.JS_UNDEFINED;
        }
        
        public static void Bind(TypeRegister register)
        {
            var ns = register.CreateNamespace("jsb");
            var cls = ns.CreateClass("Vector3", typeof(Vector3), BindConstructor);
            cls.AddMethod("Test", BindTest, 0, false);
            cls.Close();
            ns.Close();
        }
    }
}
