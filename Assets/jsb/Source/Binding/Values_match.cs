using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using Native;

    // 处理类型匹配
    public partial class Values
    {
        // guess real type in Ref<T> / Out<T>
        public static bool js_match_type_hint(JSContext ctx, JSValue jsValue, Type type)
        {
            if (jsValue.IsNullish())
            {
                return false;
            }
            
            var context = ScriptEngine.GetContext(ctx);
            var jsHintType = JSApi.JS_GetProperty(ctx, jsValue, context.GetAtom("type"));
            if (jsHintType.IsNullish())
            {
                return true;
            }

            var rs = js_match_type(ctx, jsHintType, type);
            JSApi.JS_FreeValue(ctx, jsHintType);
            return rs;
        }

        public static bool js_match_type(JSContext ctx, JSValue jsValue, Type type)
        {
            if (type == null)
            {
                return true;
            }
            if (type == typeof(object))
            {
                return true;
            }
            if (type == typeof(Type)) //TODO: remove
            {
                Type otype;
                return js_get_type(ctx, jsValue, out otype); // 只要求匹配 Type 本身, 不比较具体 Type
                // return otype == type;
            }

            if (JSApi.JS_IsObject(jsValue))
            {
                if (type == typeof(ScriptFunction) || type.BaseType == typeof(MulticastDelegate))
                {
                    return JSApi.JS_IsFunction(ctx, jsValue) == 1;
                }

                var header = JSApi.jsb_get_payload_header(jsValue);
                switch (header.type_id)
                {
                    case BridgeObjectType.ObjectRef:
                        {
                            var cache = ScriptEngine.GetObjectCache(ctx);
                            return cache.MatchObjectType(header.value, type);
                        }
                    case BridgeObjectType.TypeRef:
                        {
                            return type == typeof(Type);
                        }
                    case BridgeObjectType.ValueType:
                        {
                            var context = ScriptEngine.GetContext(ctx);
                            var type_id = JSApi.JSB_GetBridgeType(ctx, jsValue, context.GetAtom(Values.KeyForCSharpTypeID));
                            if (type_id >= 0)
                            {
                                var types = context.GetTypeDB();
                                var o = types.GetType(type_id);
                                // Debug.Log($"get type from exported registry {o}:{type_id} expected:{type}");
                                return o == type;
                            }
                            break;
                        }
                    default: // plain js object?
                        {
                            var context = ScriptEngine.GetContext(ctx);
                            if (type.IsValueType)
                            {
                                if (type.IsPrimitive || type.IsEnum)
                                {
                                    return context.CheckNumberType(jsValue);
                                }
                            }
                            else if (type == typeof(string))
                            {
                                return context.CheckStringType(jsValue);
                            }

                            break;
                        }
                }

                return type.IsSubclassOf(typeof(ScriptValue));
            }

            if (jsValue.IsNullish())
            {
                return !type.IsValueType && !type.IsPrimitive;
            }

            if (jsValue.IsBoolean())
            {
                return type == typeof(bool);
            }

            if (jsValue.IsString())
            {
                return type == typeof(string);
            }

            if (jsValue.IsNumber())
            {
                if (type.IsEnum || type.IsPrimitive)
                {
                    return true;
                }

                return false;
            }

            if (type == typeof(byte[]))
            {
                size_t psize;
                var ptr = JSApi.JS_GetArrayBuffer(ctx, out psize, jsValue);
                if (ptr != IntPtr.Zero)
                {
                    return true;
                }

                var asBuffer = JSApi.JS_GetProperty(ctx, jsValue, ScriptEngine.GetContext(ctx).GetAtom("buffer"));
                if (asBuffer.IsObject())
                {
                    ptr = JSApi.JS_GetArrayBuffer(ctx, out psize, asBuffer);
                    JSApi.JS_FreeValue(ctx, asBuffer);
                    return ptr != IntPtr.Zero;
                }
                else
                {
                    JSApi.JS_FreeValue(ctx, asBuffer);
                }
            }

            return false;
        }

        // 检查变参参数
        // offset: 从偏移处开始为变参
        public static bool js_match_param_types(JSContext ctx, int offset, JSValue[] argv, Type type)
        {
            var nargs = argv.Length;
            for (var i = offset; i < nargs; i++)
            {
                if (!js_match_type(ctx, argv[i], type))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool js_match_types(JSContext ctx, JSValue[] argv, Type t0)
        {
            return js_match_type(ctx, argv[0], t0);
        }

        public static bool js_match_types(JSContext ctx, JSValue[] argv, Type t0, Type t1)
        {
            return js_match_type(ctx, argv[0], t0) && js_match_type(ctx, argv[1], t1);
        }

        public static bool js_match_types(JSContext ctx, JSValue[] argv, Type t0, Type t1, Type t2)
        {
            return js_match_type(ctx, argv[0], t0) && js_match_type(ctx, argv[1], t1) && js_match_type(ctx, argv[2], t2);
        }

        public static bool js_match_types(JSContext ctx, JSValue[] argv, Type t0, Type t1, Type t2, Type t3)
        {
            return js_match_type(ctx, argv[0], t0) && js_match_type(ctx, argv[1], t1) && js_match_type(ctx, argv[2], t2) && js_match_type(ctx, argv[3], t3);
        }

        public static bool js_match_types(JSContext ctx, JSValue[] argv, Type t0, Type t1, Type t2, Type t3, Type t4)
        {
            return js_match_type(ctx, argv[0], t0) && js_match_type(ctx, argv[1], t1) && js_match_type(ctx, argv[2], t2) && js_match_type(ctx, argv[3], t3) && js_match_type(ctx, argv[4], t4);
        }

        public static bool js_match_types(JSContext ctx, JSValue[] argv, Type t0, Type t1, Type t2, Type t3, Type t4, Type t5)
        {
            return js_match_type(ctx, argv[0], t0) && js_match_type(ctx, argv[1], t1) && js_match_type(ctx, argv[2], t2) && js_match_type(ctx, argv[3], t3) && js_match_type(ctx, argv[4], t4) && js_match_type(ctx, argv[5], t5);
        }

        public static bool js_match_types(JSContext ctx, JSValue[] argv, Type t0, Type t1, Type t2, Type t3, Type t4, Type t5, Type t6)
        {
            return js_match_type(ctx, argv[0], t0) && js_match_type(ctx, argv[1], t1) && js_match_type(ctx, argv[2], t2) && js_match_type(ctx, argv[3], t3) && js_match_type(ctx, argv[4], t4) && js_match_type(ctx, argv[5], t5) && js_match_type(ctx, argv[6], t6);
        }

        public static bool js_match_types(JSContext ctx, JSValue[] argv, params Type[] types)
        {
            for (int i = 0, size = types.Length; i < size; i++)
            {
                if (!js_match_type(ctx, argv[i], types[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool js_match_parameters(JSContext ctx, JSValue[] argv, ParameterInfo[] parameterInfos)
        {
            for (int i = 0, size = parameterInfos.Length; i < size; i++)
            {
                var parameterInfo = parameterInfos[i];
                if (!js_match_type(ctx, argv[i], parameterInfo.ParameterType))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
