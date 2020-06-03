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
            return JS_UNDEFINED;
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
            cls.AddConstValue("name", "hello, world");
            cls.Close();
            ns.Close();
        }
    }
}
