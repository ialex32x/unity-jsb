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
        // variant push
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_classvalue(JSContext ctx, UnityEngine.Object o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }
            return js_push_object(ctx, (object)o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_classvalue(JSContext ctx, ScriptValue o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }
            return JSApi.JS_DupValue(ctx, o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe JSValue js_push_classvalue(JSContext ctx, Array o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }
            if (o.GetType() == typeof(byte[]))
            {
                var typed = (byte[])o;
                fixed (byte* mem_ptr = typed)
                {
                    return JSApi.JS_NewArrayBufferCopy(ctx, mem_ptr, typed.Length);
                }
            }
            return js_push_object(ctx, (object)o);
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_classvalue(JSContext ctx, IO.ByteBuffer o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }
            return JSApi.JS_ThrowInternalError(ctx, "not implemented: 在c端实现内存分配返回内存地址以及JSValue");
            // var mem_ptr = DuktapeDLL.js_push_fixed_buffer(ctx, (uint)o.readableBytes);
            // if (mem_ptr != IntPtr.Zero)
            // {
            //     o.ReadAllBytes(mem_ptr);
            // }
        }

        // 尝试还原 js function/dispatcher
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_delegate(JSContext ctx, Delegate o)
        {
            var dDelegate = o.Target as ScriptDelegate;
            if (dDelegate != null)
            {
                return JSApi.JS_DupValue(ctx, dDelegate);
            }
        
            // fallback
            return js_push_object(ctx, (object)o);
        }
        
        // variant push
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_classvalue(JSContext ctx, object o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }

            var type = o.GetType();
            if (type.IsEnum)
            {
                return js_push_primitive(ctx, Convert.ToInt32(o));
            }

            if (type.BaseType == typeof(MulticastDelegate))
            {
                return js_push_delegate(ctx, (Delegate)o);
            }

            return js_push_object(ctx, (object)o);
        }

        // push 一个对象实例 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static JSValue js_push_object(JSContext ctx, object o)
        {
            var cache = ScriptEngine.GetObjectCache(ctx);
            JSValue heapptr;
            if (cache.TryGetJSValue(o, out heapptr))
            {
                // Debug.LogWarningFormat("cache hit push {0}", heapptr);
                return JSApi.JS_DupValue(ctx, heapptr);
            }
            return NewBridgeClassObject(ctx, o);
        }

        // 自动判断类型
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
