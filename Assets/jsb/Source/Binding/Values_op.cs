using AOT;
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
        //NOTE: 代替 bind_native, 用于对 c# 对象产生 js 包装对象
        // 分两种情况, 这里是第1种, 在构造中使用
        private static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, object o, int type_id)
        {
            var cache = ScriptEngine.GetObjectCache(ctx);
            var object_id = cache.AddObject(o);
            var val = JSApi.JSB_NewBridgeClassObject(ctx, new_target, object_id);
            if (JSApi.JS_IsException(val))
            {
                cache.RemoveObject(object_id);
            }
            else
            {
                JSApi.JSB_SetBridgeType(ctx, val, type_id);
            }
            return val;
        }

        //NOTE: 代替 bind_native, 用于对 c# 对象产生 js 包装对象
        // 分两种情况, 这里是第2种, 用于一般情况
        public static JSValue NewBridgeClassObject(JSContext ctx, object o)
        {
            if (o == null)
            {
                return JS_UNDEFINED;
            }
            int type_id;
            var type = o.GetType();
            var proto = FindPrototypeOf(ctx, type, out type_id);

            if (proto.IsNullish())
            {
                return JSApi.JS_ThrowInternalError(ctx, string.Format("no prototype found for {0}", type));
            }

            var cache = ScriptEngine.GetObjectCache(ctx);
            var object_id = cache.AddObject(o);
            var val = JSApi.jsb_new_bridge_object(ctx, proto, object_id);
            if (val.IsException())
            {
                cache.RemoveObject(object_id);
            }
            else
            {
                JSApi.JSB_SetBridgeType(ctx, val, type_id);
            }
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue FindPrototypeOf(JSContext ctx, Type type, out int type_id)
        {
            var types = ScriptEngine.GetTypeDB(ctx);
            return types.FindPrototypeOf(type, out type_id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue FindPrototypeOf(JSContext ctx, Type type)
        {
            int type_id;
            var types = ScriptEngine.GetTypeDB(ctx);
            return types.FindPrototypeOf(type, out type_id);
        }

        //
        //         public static bool duk_rebind_this(IntPtr ctx, object o)
        //         {
        //             DuktapeDLL.duk_push_this(ctx);
        //             var ret = duk_rebind_native(ctx, -1, o);
        //             DuktapeDLL.duk_pop(ctx);
        //             return ret;
        //         }
        //
        //         public static bool duk_rebind_this(IntPtr ctx, LayerMask o)
        //         {
        //             DuktapeDLL.duk_push_this(ctx);
        //             DuktapeDLL.duk_push_int(ctx, o);
        //             DuktapeDLL.duk_put_prop_index(ctx, -2, 0);
        //             DuktapeDLL.duk_pop(ctx);
        //             return true;
        //         }
        //
        //         public static bool duk_rebind_this(IntPtr ctx, Vector2 o)
        //         {
        //             DuktapeDLL.duk_push_this(ctx);
        //             DuktapeDLL.duk_unity_put2f(ctx, -1, o.x, o.y);
        //             DuktapeDLL.duk_pop(ctx);
        //             return true;
        //         }
        //
        //         public static bool duk_rebind_this(IntPtr ctx, Vector2Int o)
        //         {
        //             DuktapeDLL.duk_push_this(ctx);
        //             DuktapeDLL.duk_unity_put2i(ctx, -1, o.x, o.y);
        //             DuktapeDLL.duk_pop(ctx);
        //             return true;
        //         }
        //
        //         public static bool duk_rebind_this(IntPtr ctx, Color o)
        //         {
        //             DuktapeDLL.duk_push_this(ctx);
        //             DuktapeDLL.duk_unity_put4f(ctx, -1, o.r, o.g, o.b, o.a);
        //             DuktapeDLL.duk_pop(ctx);
        //             return true;
        //         }
        //
        //         public static bool duk_rebind_this(IntPtr ctx, Color32 o)
        //         {
        //             DuktapeDLL.duk_push_this(ctx);
        //             DuktapeDLL.duk_unity_put4i(ctx, -1, o.r, o.g, o.b, o.a);
        //             DuktapeDLL.duk_pop(ctx);
        //             return true;
        //         }
        //
        //         public static bool duk_rebind_this(IntPtr ctx, Vector3 o)
        //         {
        //             DuktapeDLL.duk_push_this(ctx);
        //             DuktapeDLL.duk_unity_put3f(ctx, -1, o.x, o.y, o.z);
        //             DuktapeDLL.duk_pop(ctx);
        //             return true;
        //         }
        //
        //         public static bool duk_rebind_this(IntPtr ctx, Vector3Int o)
        //         {
        //             DuktapeDLL.duk_push_this(ctx);
        //             DuktapeDLL.duk_unity_put3i(ctx, -1, o.x, o.y, o.z);
        //             DuktapeDLL.duk_pop(ctx);
        //             return true;
        //         }
        //
        //         public static bool duk_rebind_this(IntPtr ctx, Vector4 o)
        //         {
        //             DuktapeDLL.duk_push_this(ctx);
        //             DuktapeDLL.duk_unity_put4f(ctx, -1, o.x, o.y, o.z, o.w);
        //             DuktapeDLL.duk_pop(ctx);
        //             return true;
        //         }
        //
        //         public static bool duk_rebind_this(IntPtr ctx, Quaternion o)
        //         {
        //             DuktapeDLL.duk_push_this(ctx);
        //             DuktapeDLL.duk_unity_put4f(ctx, -1, o.x, o.y, o.z, o.w);
        //             DuktapeDLL.duk_pop(ctx);
        //             return true;
        //         }
        //
        //         // public static bool duk_rebind_this(IntPtr ctx, Matrix4x4 o)
        //         // {
        //         //     DuktapeDLL.duk_push_this(ctx);
        //         //     DuktapeDLL.duk_unity_put16f(ctx, -1, ...);
        //         //     DuktapeDLL.duk_pop(ctx);
        //         //     return true;
        //         // }
        //
        //         public static bool duk_get_native_refid(IntPtr ctx, int idx, out int id)
        //         {
        //             if (DuktapeDLL.duk_unity_get_refid(ctx, idx, out id))
        //             {
        //                 return true;
        //             }
        //             return false;
        //         }
        //
        //         public static bool duk_rebind_native(IntPtr ctx, int idx, object o)
        //         {
        //             int id;
        //             if (DuktapeDLL.duk_unity_get_refid(ctx, idx, out id))
        //             {
        //                 return DuktapeVM.GetObjectCache(ctx).ReplaceObject(id, o);
        //             }
        //             return false;
        //         }
    }
}
