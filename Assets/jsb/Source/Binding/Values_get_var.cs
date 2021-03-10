using System;
using System.Reflection;
using System.Collections.Generic;

namespace QuickJS.Binding
{
    using Native;

    public partial class Values
    {
        // 用于根据 Type 信息将 JSValue 专为对应的 CS Object
        private static Dictionary<Type, MethodInfo> _JSCastMap = new Dictionary<Type, MethodInfo>();

        private static Dictionary<Type, MethodInfo> _CSCastMap = new Dictionary<Type, MethodInfo>();

        private static Dictionary<Type, MethodInfo> _JSRebindMap = new Dictionary<Type, MethodInfo>();

        private static Dictionary<Type, MethodInfo> _JSNewMap = new Dictionary<Type, MethodInfo>();

        // 初始化, 在 Values 静态构造时调用
        private static void init_cast_map()
        {
            var methods = typeof(Values).GetMethods();
            foreach (var method in methods)
            {
                if (!method.IsGenericMethodDefinition)
                {
                    var parameters = method.GetParameters();

                    if (method.Name == "NewBridgeClassObject")
                    {
                        if (parameters.Length == 5)
                        {
                            var type = parameters[2].ParameterType;
                            _JSNewMap[type] = method;
                        }
                    }
                    else if (method.Name == "js_rebind_this")
                    {
                        if (parameters.Length == 3 && parameters[2].ParameterType.IsByRef)
                        {
                            var type = parameters[2].ParameterType.GetElementType();
                            _JSRebindMap[type] = method;
                        }
                    }
                    else if (method.Name.StartsWith("js_get_"))
                    {
                        if (parameters.Length == 3 && parameters[2].ParameterType.IsByRef)
                        {
                            var type = parameters[2].ParameterType.GetElementType();

                            switch (method.Name)
                            {
                                case "js_get_primitive":
                                case "js_get_structvalue":
                                case "js_get_classvalue":
                                    _JSCastMap[type] = method;
                                    break;
                            }
                        }
                    }
                    else if (method.Name.StartsWith("js_push_"))
                    {
                        if (parameters.Length == 2)
                        {
                            var type = parameters[1].ParameterType;

                            _CSCastMap[type] = method;
                        }
                    }
                }
            }
        }

        public static bool js_rebind_var(JSContext ctx, JSValue this_obj, Type type, object o)
        {
            MethodInfo method;
            if (_JSRebindMap.TryGetValue(type, out method))
            {
                var parameters = new object[3] { ctx, this_obj, o };
                return (bool)method.Invoke(o, parameters);
            }
            return false;
        }

        // 自动判断类型
        public static JSValue js_push_var(JSContext ctx, object o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }

            var type = o.GetType();

            if (type.BaseType == typeof(MulticastDelegate))
            {
                return js_push_delegate(ctx, o as Delegate);
            }

            if (type.IsEnum)
            {
                return js_push_primitive(ctx, Convert.ToInt32(o));
            }

            MethodInfo cast;
            do
            {
                if (_CSCastMap.TryGetValue(type, out cast))
                {
                    var parameters = new object[2] { ctx, o };
                    var rval = (JSValue)cast.Invoke(null, parameters);
                    return rval;
                }
                type = type.BaseType;
            } while (type != null);

            //NOTE: 2. fallthrough, push as object
            return js_push_classvalue(ctx, o);
        }

        public static JSValue js_new_var(JSContext ctx, JSValue new_target, Type type, object o, int type_id, bool disposable)
        {
            MethodInfo cast;
            if (_JSNewMap.TryGetValue(type, out cast))
            {
                var parameters = new object[5] { ctx, new_target, o, type_id, disposable };
                var rval = (JSValue)cast.Invoke(null, parameters);
                return rval;
            }

            return NewBridgeClassObject(ctx, new_target, o, type_id, disposable);
        }

        public static bool js_get_var(JSContext ctx, JSValue val, out object o)
        {
            return js_get_fallthrough(ctx, val, out o);
        }

        // type: expected type of object o
        public static bool js_get_var(JSContext ctx, JSValue val, Type type, out object o)
        {
            //TODO: 处理数组
            // if (type.IsArray)
            // {
            // }

            if (type.BaseType == typeof(MulticastDelegate))
            {
                Delegate d;
                var rs = js_get_delegate(ctx, val, type, out d);
                o = d;
                return rs;
            }

            if (type.IsEnum)
            {
                return js_get_enumvalue(ctx, val, type, out o);
            }

            if (type == typeof(void))
            {
                o = null;
                return true;
            }

            MethodInfo cast;
            do
            {
                if (_JSCastMap.TryGetValue(type, out cast))
                {
                    var parameters = new object[3] { ctx, val, null };
                    var rval = (bool)cast.Invoke(null, parameters);
                    o = parameters[2];
                    return rval;
                }
                type = type.BaseType;
            } while (type != null);

            return js_get_fallthrough(ctx, val, out o);
        }
    }
}
