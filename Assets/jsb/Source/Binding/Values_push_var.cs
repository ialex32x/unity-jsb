using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using Native;
    using UnityEngine;
    
    public partial class Values
    {
        // 自动判断类型
        //TODO: use type-indexed handler map
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_var(JSContext ctx, object o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }
            var type = o.GetType();
        
            //NOTE: 1. push as simple types
            if (type.IsValueType)
            {
                if (type.IsPrimitive)
                {
                    if (type == typeof(bool))
                    {
                        return js_push_primitive(ctx, (bool)o);
                    }
                    if (type == typeof(byte))
                    {
                        return js_push_primitive(ctx, (byte)o);
                    }
                    if (type == typeof(char))
                    {
                        return js_push_primitive(ctx, (char)o);
                    }
                    if (type == typeof(sbyte))
                    {
                        return js_push_primitive(ctx, (sbyte)o);
                    }
                    if (type == typeof(short))
                    {
                        return js_push_primitive(ctx, (short)o);
                    }
                    if (type == typeof(ushort))
                    {
                        return js_push_primitive(ctx, (ushort)o);
                    }
                    if (type == typeof(int))
                    {
                        return js_push_primitive(ctx, (int)o);
                    }
                    if (type == typeof(uint))
                    {
                        return js_push_primitive(ctx, (uint)o);
                    }
                    if (type == typeof(long))
                    {
                        return js_push_primitive(ctx, (long)o);
                    }
                    if (type == typeof(ulong))
                    {
                        return js_push_primitive(ctx, (ulong)o);
                    }
                    if (type == typeof(float))
                    {
                        return js_push_primitive(ctx, (float)o);
                    }
                    if (type == typeof(double))
                    {
                        return js_push_primitive(ctx, (double)o);
                    }
                }
                else
                {
                    if (type.IsEnum)
                    {
                        return js_push_primitive(ctx, Convert.ToInt32(o));
                    }
                }
            }
            else
            {
                if (type == typeof(string))
                {
                    return js_push_primitive(ctx, (string)o);
                }
            }
        
            //NOTE: 2. fallthrough, push as object
            return js_push_classvalue(ctx, o);
        }
    }
}
