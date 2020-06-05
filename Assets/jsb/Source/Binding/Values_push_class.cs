using System;
using System.Collections.Generic;
using AOT;

namespace QuickJS.Binding
{
    using Native;
    using UnityEngine;


    public partial class Values
    {
        // variant push
        public static void js_push_classvalue(JSContext ctx, UnityEngine.Object o)
        {
            if (o == null)
            {
                DuktapeDLL.js_push_null(ctx);
                return;
            }
            js_push_object(ctx, (object)o);
        }

        // public static void js_push_classvalue(JSContext ctx, IO.ByteBuffer o)
        // {
        //     if (o == null)
        //     {
        //         DuktapeDLL.js_push_null(ctx);
        //         return;
        //     }
        //     var mem_ptr = DuktapeDLL.js_push_fixed_buffer(ctx, (uint)o.readableBytes);
        //     if (mem_ptr != IntPtr.Zero)
        //     {
        //         o.ReadAllBytes(mem_ptr);
        //     }
        // }

        public static void js_push_classvalue(JSContext ctx, DuktapeObject o)
        {
            if (o == null)
            {
                DuktapeDLL.js_push_null(ctx);
                return;
            }
            if (!o.Push(ctx))
            {
                DuktapeDLL.js_push_null(ctx);
            }
        }

        public static void js_push_classvalue(JSContext ctx, Array o)
        {
            if (o == null)
            {
                DuktapeDLL.js_push_null(ctx);
                return;
            }
            if (o.GetType() == typeof(byte[]))
            {
                var typed = (byte[])o;
                var mem_ptr = DuktapeDLL.js_push_fixed_buffer(ctx, (uint)typed.Length);
                if (mem_ptr != IntPtr.Zero)
                {
                    System.Runtime.InteropServices.Marshal.Copy(typed, 0, mem_ptr, typed.Length);
                }
                return;
            }
            js_push_object(ctx, (object)o);
        }

        // variant push
        public static void js_push_classvalue(JSContext ctx, object o)
        {
            if (o == null)
            {
                DuktapeDLL.js_push_null(ctx);
                return;
            }
            var type = o.GetType();
            if (type.IsEnum)
            {
                js_push_primitive(ctx, Convert.ToInt32(o));
                return;
            }
            // if (type.IsArray)
            // {
            //     js_push_any(ctx, (Array)o);
            //     return;
            // }
            if (type.BaseType == typeof(MulticastDelegate))
            {
                js_push_delegate(ctx, (Delegate)o);
                return;
            }
            js_push_object(ctx, (object)o);
        }

        // push 一个对象实例 （调用者需要自行负责提前null检查） 
        private static void js_push_object(JSContext ctx, object o)
        {
            var cache = DuktapeVM.GetObjectCache(ctx);
            IntPtr heapptr;
            if (cache.TryGetJSValue(o, out heapptr))
            {
                // Debug.LogWarningFormat("cache hit push {0}", heapptr);
                DuktapeDLL.js_push_heapptr(ctx, heapptr);
                return;
            }
            DuktapeDLL.js_push_object(ctx);
            duk_bind_native(ctx, -1, o);
        }

        // 自动判断类型
        public static void js_push_var(JSContext ctx, object o)
        {
            if (o == null)
            {
                DuktapeDLL.js_push_null(ctx);
                return;
            }
            var type = o.GetType();
            
            //NOTE: 1. push as simple types
            if (type.IsValueType)
            {
                if (type.IsPrimitive)
                {
                    if (type == typeof(bool))
                    {
                        js_push_primitive(ctx, (bool)o);
                        return;
                    }
                    if (type == typeof(byte))
                    {
                        js_push_primitive(ctx, (byte)o);
                        return;
                    }
                    if (type == typeof(char))
                    {
                        js_push_primitive(ctx, (char)o);
                        return;
                    }
                    if (type == typeof(sbyte))
                    {
                        js_push_primitive(ctx, (sbyte)o);
                        return;
                    }
                    if (type == typeof(short))
                    {
                        js_push_primitive(ctx, (short)o);
                        return;
                    }
                    if (type == typeof(ushort))
                    {
                        js_push_primitive(ctx, (ushort)o);
                        return;
                    }
                    if (type == typeof(int))
                    {
                        js_push_primitive(ctx, (int)o);
                        return;
                    }
                    if (type == typeof(uint))
                    {
                        js_push_primitive(ctx, (uint)o);
                        return;
                    }
                    if (type == typeof(long))
                    {
                        js_push_primitive(ctx, (long)o);
                        return;
                    }
                    if (type == typeof(ulong))
                    {
                        js_push_primitive(ctx, (ulong)o);
                        return;
                    }
                    if (type == typeof(float))
                    {
                        js_push_primitive(ctx, (float)o);
                        return;
                    }
                    if (type == typeof(double))
                    {
                        js_push_primitive(ctx, (double)o);
                        return;
                    }
                }
                else
                {
                    if (type.IsEnum)
                    {
                        js_push_primitive(ctx, Convert.ToInt32(o));
                        return;
                    }
                }
            }
            else
            {
                if (type == typeof(string))
                {
                    js_push_primitive(ctx, (string)o);
                    return;
                }
            }

            //NOTE: 2. fallthrough, push as object
            js_push_classvalue(ctx, o);
        }
    }
}
