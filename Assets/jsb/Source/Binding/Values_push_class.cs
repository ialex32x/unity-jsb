using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using Native;

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
        public static JSValue js_push_classvalue(JSContext ctx, ScriptPromise promise)
        {
            if (promise == null)
            {
                return JSApi.JS_NULL;
            }
            return JSApi.JS_DupValue(ctx, promise);
        }

        // variant push
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_classvalue(JSContext ctx, Type type)
        {
            if (type == null)
            {
                return JSApi.JS_NULL;
            }

            var runtime = ScriptEngine.GetRuntime(ctx);
            var db = runtime.GetTypeDB();

            return db.GetConstructorOf(type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_classvalue_hotfix(JSContext ctx, Type type)
        {
            if (type == null)
            {
                return JSApi.JS_NULL;
            }

            var runtime = ScriptEngine.GetRuntime(ctx);
            var db = runtime.GetTypeDB();

            return db.GetConstructorOf(type);
        }

        public static JSValue js_push_classvalue(JSContext ctx, Delegate o)
        {
            return js_push_delegate(ctx, o);
        }

        // variant push
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_classvalue(JSContext ctx, object o)
        {
            if (o == null)
            {
                return JSApi.JS_NULL;
            }

            return js_push_object(ctx, o);
        }

        // 用于热更 C# 代码中传入的 this
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_classvalue_hotfix(JSContext ctx, object this_obj)
        {
            if (this_obj == null)
            {
                return JSApi.JS_NULL;
            }

            return js_push_object(ctx, this_obj);
        }

        // push 一个对象实例 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_object(JSContext ctx, object o)
        {
            var cache = ScriptEngine.GetObjectCache(ctx);
            JSValue heapptr;
            if (cache.TryGetJSValue(o, out heapptr))
            {
                // Debug.LogWarningFormat("cache hit push {0}", heapptr);
                return JSApi.JS_DupValue(ctx, heapptr);
            }
            return NewBridgeClassObject(ctx, o, true);
        }
    }
}
