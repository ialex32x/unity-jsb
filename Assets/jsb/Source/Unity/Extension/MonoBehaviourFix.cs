using System;
using System.Reflection;

namespace QuickJS.Unity
{
    using Native;
    using Binding;
    using UnityEngine;

    public class MonoBehaviourFix
    {
        [JSCFunction(true,
            "AddComponent<T extends Component>(type: {{ new(): T }}): T")]
        public static JSValue Bind_AddComponent(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            try
            {
                if (argc == 1)
                {
                    UnityEngine.GameObject self;
                    if (!Values.js_get_classvalue(ctx, this_obj, out self))
                    {
                        throw new ThisBoundException();
                    }
                    System.Type arg0;
                    if (!Values.js_get_type(ctx, argv[0], out arg0))
                    {
                        throw new ParameterException(typeof(UnityEngine.GameObject), "AddComponent", typeof(System.Type), 0);
                    }
                    var inject = Values._js_game_object_add_component(ctx, argv[0], self, arg0);
                    if (!inject.IsUndefined())
                    {
                        return inject;
                    }
                    var ret = self.AddComponent(arg0);
                    return Values.js_push_classvalue(ctx, ret);
                }
                throw new NoSuitableMethodException("AddComponent", argc);
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }
    }
}
