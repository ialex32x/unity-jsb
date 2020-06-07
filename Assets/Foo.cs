using AOT;
using QuickJS;
using QuickJS.Binding;
using QuickJS.Native;
using QuickJS.Utils;
using UnityEngine;

namespace jsb
{
    public class Foo
    {
        public int Test()
        {
            Debug.LogFormat("foo.test in c#");
            return 123;
        }
    }
    
    public class FooBinding : Values
    {
        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        private static JSValue BindConstructor(JSContext ctx, JSValue new_target, int argc, JSValue[] argv, int magic)
        {
            var cache = ScriptEngine.GetObjectCache(ctx);
            var object_id = cache.AddObject(new Foo());
            JSValue obj = JSApi.JSB_NewBridgeClassObject(ctx, new_target, object_id);
            JSApi.JSB_SetBridgeType(ctx, obj, magic);
            return obj;
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        private static JSValue BindTest(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            var rt = ScriptEngine.GetRuntime(ctx);
            var cache = rt.GetObjectCache();
            var payload = JSApi.jsb_get_payload_header(this_obj);
            Foo inst;
            if (payload.type_id == BridgeObjectType.ObjectRef && cache.TryGetTypedObject(payload.value, out inst))
            {
               var rval = inst.Test();
               return JSApi.JS_NewInt32(ctx, rval);
            }

            return JSApi.JS_ThrowInternalError(ctx, "unbounded value");
        }
        
        [MonoPInvokeCallback(typeof(JSCFunction))]
        private static JSValue BindRead_wall(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            return JSApi.JS_ThrowInternalError(ctx, "not implemented");
        }
        
        [MonoPInvokeCallback(typeof(JSCFunction))]
        private static JSValue BindWrite_wall(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            return JSApi.JS_ThrowInternalError(ctx, "not implemented");
        }
        
        public static void Bind(TypeRegister register)
        {
            var ns = register.CreateNamespace("jsb");
            var cls = ns.CreateClass("Foo", typeof(Foo), BindConstructor);
            cls.AddMethod(false, "Test", BindTest, 0);
            cls.AddConstValue("greet", "hello, world");
            cls.AddProperty(true, "wall", BindRead_wall, BindWrite_wall);
            cls.AddProperty(true, "rwall", BindRead_wall, null);
            cls.AddProperty(false, "xwall", BindRead_wall, BindWrite_wall);
            cls.AddProperty(false, "rxwall", BindRead_wall, null);
            cls.Close();
            ns.Close();
        }
    }
}
