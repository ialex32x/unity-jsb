using AOT;
using QuickJS;
using QuickJS.Binding;
using QuickJS.Native;
using QuickJS.Utils;

namespace jsb
{
    public class Foo : Values
    {
        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        private static JSValue BindConstructor(JSContext ctx, JSValue new_target, int argc, JSValue[] argv, int magic)
        {
            var rt = ScriptEngine.GetRuntime(ctx);
            var cache = rt.GetObjectCache();
            var object_id = cache.AddObject(new object());
            JSValue proto = JSApi.JS_GetProperty(ctx, new_target, JSApi.JS_ATOM_prototype);
            JSValue obj = JSApi.JS_NewObjectProtoClass(ctx, proto, rt._def_class_id);
            JSApi.JSB_NewClassPayload(ctx, obj, rt._def_class_id, 0, object_id);
            JSApi.JS_FreeValue(ctx, proto);
            return obj;
        }

        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        private static JSValue BindTest(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv, int magic)
        {
            return JS_UNDEFINED;
        }
        
        public static void Bind(TypeRegister register)
        {
            var ns = register.CreateNamespace("jsb");
            var cls = ns.CreateClass("Foo", typeof(Foo), BindConstructor);
            cls.AddConstValue("greet", "hello, world");
            cls.Close();
            ns.Close();
        }
    }
}
