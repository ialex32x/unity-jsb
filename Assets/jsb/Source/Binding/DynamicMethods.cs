using System;
using System.Reflection;
using System.Collections.Generic;
using QuickJS.Native;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    public class DynamicMethods : IDynamicMethod
    {
        private DynamicMethod[] _overloads;

        public static DynamicMethods Create(Type type, MethodBase[] methodBase)
        {
            throw new NotImplementedException();
        }

        public JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            var len = _overloads.Length;
            for (var i = 0; i < len; i++)
            {
                var method = _overloads[i];
                if (method.CheckArgs(argc, argv))
                {
                    return method.Invoke(ctx, this_obj, argc, argv);
                }
            }
            return JSApi.JS_ThrowInternalError(ctx, "no overload method matched");
        }
    }
}
