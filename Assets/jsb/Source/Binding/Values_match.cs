using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using Native;
    using UnityEngine;

    // 处理类型匹配
    public partial class Values
    {
        protected static bool js_match_type(JSContext ctx, JSValue jsValue, Type type)
        {
            if (type == null)
            {
                return true;
            }
            if (type == typeof(object))
            {
                return true;
            }
            if (type == typeof(Type))
            {
                Type otype;
                return js_get_type(ctx, jsValue, out otype); // 只要求匹配 Type 本身, 不比较具体 Type
                // return otype == type;
            }

            if (JSApi.JS_IsObject(jsValue))
            {
                if (JSApi.JS_IsArray(ctx, jsValue) == 1)
                {
                    if (!type.IsArray && !_assignableFromArray.Contains(type))
                    {
                        return false;
                    }
                }
                else if (JSApi.JS_IsFunction(ctx, jsValue) == 1)
                {
                    //TODO: 完善处理 delegate 
                    return type == typeof(ScriptFunction) || type.BaseType == typeof(MulticastDelegate);
                }

                var header = JSApi.jsb_get_payload_header(jsValue);
                if (header.type_id == BridgeObjectType.ObjectRef)
                {
                    var cache = ScriptEngine.GetObjectCache(ctx);
                    return cache.MatchObjectType(header.value, type);
                }

                if (header.type_id == BridgeObjectType.TypeRef)
                {
                    var types = ScriptEngine.GetTypeDB(ctx);
                    var eType = types.GetType(header.value);
                    if (eType != null)
                    {
                        // Debug.LogFormat("match type? {0} {1} {2}", eType, type, typeid); 
                        return eType == type;
                    }
                    // Debug.LogFormat("match type {0} with typeid {1}", type, typeid);
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
                return ptr != IntPtr.Zero;
            }
            
            return false;
        }

        // 检查变参参数
        // offset: 从偏移处开始为变参
        protected static bool js_match_param_types(JSContext ctx, int offset, JSValue[] argv, Type type)
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

        protected static bool js_match_types(JSContext ctx, JSValue[] argv, Type t0)
        {
            return js_match_type(ctx, argv[0], t0);
        }

        protected static bool js_match_types(JSContext ctx, JSValue[] argv, Type t0, Type t1)
        {
            return js_match_type(ctx, argv[0], t0) && js_match_type(ctx, argv[1], t1);
        }

        protected static bool js_match_types(JSContext ctx, JSValue[] argv, Type t0, Type t1, Type t2)
        {
            return js_match_type(ctx, argv[0], t0) && js_match_type(ctx, argv[1], t1) && js_match_type(ctx, argv[2], t2);
        }

        protected static bool js_match_types(JSContext ctx, JSValue[] argv, Type t0, Type t1, Type t2, Type t3)
        {
            return js_match_type(ctx, argv[0], t0) && js_match_type(ctx, argv[1], t1) && js_match_type(ctx, argv[2], t2) && js_match_type(ctx, argv[3], t3);
        }

        protected static bool js_match_types(JSContext ctx, JSValue[] argv, Type t0, Type t1, Type t2, Type t3, Type t4)
        {
            return js_match_type(ctx, argv[0], t0) && js_match_type(ctx, argv[1], t1) && js_match_type(ctx, argv[2], t2) && js_match_type(ctx, argv[3], t3) && js_match_type(ctx, argv[4], t4);
        }

        protected static bool js_match_types(JSContext ctx, JSValue[] argv, Type t0, Type t1, Type t2, Type t3, Type t4, Type t5)
        {
            return js_match_type(ctx, argv[0], t0) && js_match_type(ctx, argv[1], t1) && js_match_type(ctx, argv[2], t2) && js_match_type(ctx, argv[3], t3) && js_match_type(ctx, argv[4], t4) && js_match_type(ctx, argv[5], t5);
        }

        protected static bool js_match_types(JSContext ctx, JSValue[] argv, Type t0, Type t1, Type t2, Type t3, Type t4, Type t5, Type t6)
        {
            return js_match_type(ctx, argv[0], t0) && js_match_type(ctx, argv[1], t1) && js_match_type(ctx, argv[2], t2) && js_match_type(ctx, argv[3], t3) && js_match_type(ctx, argv[4], t4) && js_match_type(ctx, argv[5], t5) && js_match_type(ctx, argv[6], t6);
        }

        protected static bool js_match_types(JSContext ctx, JSValue[] argv, params Type[] types)
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
    }
}
