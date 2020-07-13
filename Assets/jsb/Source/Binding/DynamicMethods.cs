using System;
using System.Reflection;
using System.Collections.Generic;
using QuickJS.Native;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    // 存在重载的函数调用
    public class DynamicMethods : IDynamicMethod
    {
        private List<DynamicMethodBase> _overloads;

        public DynamicMethods(int initCapacity)
        {
            _overloads = new List<DynamicMethodBase>(initCapacity);
        }

        public void Add(DynamicMethodBase method)
        {
            var count = _overloads.Count;
            for (var i = 0; i < count; i++)
            {
                var overload = _overloads[i];
                if (method.GetParameters().Length > overload.GetParameters().Length)
                {
                    _overloads.Insert(i, method);
                    return;
                }
            }
            _overloads.Add(method);
        }

        public JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            var count = _overloads.Count;
            for (var i = 0; i < count; i++)
            {
                var method = _overloads[i];
                if (method.CheckArgs(ctx, argc, argv))
                {
                    return method.Invoke(ctx, this_obj, argc, argv);
                }
            }
            return JSApi.JS_ThrowInternalError(ctx, "no overload method matched");
        }
    }
}
