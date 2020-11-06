using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using UnityEngine;
    using Native;

    // 处理特殊操作, 关联本地对象等
    public partial class Values
    {
        //NOTE: 用于对 c# 对象产生 js 包装对象
        // 分两种情况, 这里是第1种, 在构造中使用
        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, object o, int type_id)
        {
            var cache = ScriptEngine.GetObjectCache(ctx);
            var object_id = cache.AddObject(o, false);
            var val = JSApi.JSB_NewBridgeClassObject(ctx, new_target, object_id);
            if (JSApi.JS_IsException(val))
            {
                cache.RemoveObject(object_id);
            }
            else
            {
                cache.AddJSValue(o, val);
            }

            return val;
        }

        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, IScriptFinalize o, int type_id)
        {
            var cache = ScriptEngine.GetObjectCache(ctx);
            var object_id = cache.AddObject(o, true);
            var val = JSApi.JSB_NewBridgeClassObject(ctx, new_target, object_id);
            if (JSApi.JS_IsException(val))
            {
                cache.RemoveObject(object_id);
            }
            else
            {
                cache.AddJSValue(o, val);
            }

            return val;
        }

        //NOTE: 用于对 c# 对象产生 js 包装对象
        // 分两种情况, 这里是第2种, 用于一般情况
        public static JSValue NewBridgeClassObject(JSContext ctx, object o, bool makeRef)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }

            var type = o.GetType();
            var runtime = ScriptEngine.GetRuntime(ctx);
            var db = runtime.GetTypeDB();
            var proto = db.GetPrototypeOf(type.BaseType == typeof(MulticastDelegate) ? typeof(Delegate) : type);

            if (proto.IsNullish())
            {
                db.GetDynamicType(type, false);
                proto = db.GetPrototypeOf(type);
                if (proto.IsNullish())
                {
                    return JSApi.JS_ThrowInternalError(ctx, string.Format("no prototype found for {0}", type));
                }
            }

            var cache = runtime.GetObjectCache();
            var object_id = cache.AddObject(o, false);
            var val = JSApi.jsb_new_bridge_object(ctx, proto, object_id);
            if (val.IsException())
            {
                cache.RemoveObject(object_id);
            }
            else
            {
                if (makeRef)
                {
                    cache.AddJSValue(o, val);
                }
            }

            return val;
        }
    }
}
