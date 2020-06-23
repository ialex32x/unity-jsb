using System;
using System.Collections.Generic;
using QuickJS.Native;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    public interface IDynamicMethod
    {
        JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv);
    }

    public class DynamicMethod : IDynamicMethod
    {
        public bool CheckArgs(int argc, JSValue[] argv)
        {
            //TODO: check args if any overload func exists
            return true;
        }

        public JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            //TODO: invoke csharp method
            return JSApi.JS_UNDEFINED;
        }
    }
}
