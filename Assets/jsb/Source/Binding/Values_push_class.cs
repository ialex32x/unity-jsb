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
        public static unsafe JSValue js_push_classvalue(JSContext ctx, byte[] o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }
            var typed = (byte[])o;
            fixed (byte* mem_ptr = typed)
            {
                return JSApi.JS_NewArrayBufferCopy(ctx, mem_ptr, typed.Length);
            }
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

        // 用于显示要求转为 js array (将与 cs array 实例无关联)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe JSValue js_push_classvalue_array<T>(JSContext ctx, T[] o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }
            var length = o.Length;
            var rval = JSApi.JS_NewArray(ctx);
            try
            {
                for (var i = 0; i < length; i++)
                {
                    var obj = o[i];
                    var elem = Values.js_push_object(ctx, obj);
                    JSApi.JS_SetPropertyUint32(ctx, rval, (uint)i, elem);
                }
            }
            catch (Exception exception)
            {
                JSApi.JS_FreeValue(ctx, rval);
                return JSApi.ThrowException(ctx, exception);
            }
            return rval;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe JSValue js_push_classvalue(JSContext ctx, IO.ByteBuffer o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }

            fixed (byte* buf = o.data)
            {
                return JSApi.JS_NewArrayBufferCopy(ctx, buf, o.readableBytes);
            }
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
    }
}
