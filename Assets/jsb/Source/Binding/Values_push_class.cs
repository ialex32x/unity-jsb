using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using Native;

    public partial class Values
    {
#if !JSB_UNITYLESS
        // variant push
        public static JSValue js_push_classvalue(JSContext ctx, UnityEngine.Object o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }

            return js_push_object(ctx, (object)o);
        }
#endif

        public static JSValue js_push_classvalue(JSContext ctx, ScriptValue o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }
            return JSApi.JS_DupValue(ctx, o);
        }

        public static unsafe JSValue js_push_classvalue(JSContext ctx, Array o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }

            return js_push_object(ctx, (object)o);
        }

        /// <summary>
        /// 用于显式要求转为 JS Array (返回值与 CS Array 实例没有生命周期关联)
        /// </summary>
        public static unsafe JSValue js_push_classvalue_array(JSContext ctx, object o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }
            if (!(o is Array))
            {
                return JSApi.ThrowException(ctx, new InvalidCastException($"fail to cast type to Array"));
            }
            var arr = (Array)o;
            var length = arr.Length;
            var rval = JSApi.JS_NewArray(ctx);
            try
            {
                for (var i = 0; i < length; i++)
                {
                    var obj = arr.GetValue(i);
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

        public static JSValue js_push_classvalue(JSContext ctx, ScriptPromise promise)
        {
            if (promise == null)
            {
                return JSApi.JS_NULL;
            }
            return JSApi.JS_DupValue(ctx, promise);
        }

        // variant push
        // public static JSValue js_push_classvalue(JSContext ctx, Type type)
        // {
        //     if (type == null)
        //     {
        //         return JSApi.JS_NULL;
        //     }

        //     var runtime = ScriptEngine.GetRuntime(ctx);
        //     var db = runtime.GetTypeDB();

        //     return db.GetConstructorOf(type);
        // }

        public static JSValue js_push_classvalue(JSContext ctx, Type o)
        {
            var context = ScriptEngine.GetContext(ctx);
            var types = context.GetTypeDB();
            var jsVal = types.GetPrototypeOf(o);
            return JSApi.JS_DupValue(ctx, jsVal);
        }

        // variant push
        public static JSValue js_push_classvalue(JSContext ctx, object o)
        {
            if (o == null)
            {
                return JSApi.JS_NULL;
            }

            return js_push_object(ctx, o);
        }

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

        // 用于热更 C# 代码中传入的 this
        public static JSValue js_push_classvalue_hotfix(JSContext ctx, object this_obj)
        {
            if (this_obj == null)
            {
                return JSApi.JS_NULL;
            }

            return js_push_object(ctx, this_obj);
        }

        // push 一个对象实例 
        public static JSValue js_push_object(JSContext ctx, object o)
        {
            var cache = ScriptEngine.GetObjectCache(ctx);
            JSValue heapptr;
            if (cache.TryGetJSValue(o, out heapptr))
            {
                return JSApi.JS_DupValue(ctx, heapptr);
            }
            return NewBridgeObjectBind(ctx, o, true);
        }
    }
}
