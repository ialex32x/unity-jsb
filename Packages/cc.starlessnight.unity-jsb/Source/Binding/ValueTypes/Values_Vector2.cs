#if !JSB_UNITYLESS
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using UnityEngine;
    using Native;

    public partial class Values
    {
        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, Vector2 o, int type_id, bool disposable)
        {
            var val = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(float) * 2);
            if (!JSApi.JS_IsException(val))
            {
                JSApi.jsb_set_float_2(val, o.x, o.y);
            }
            return val;
        }

        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, ref Vector2 o)
        {
            return JSApi.jsb_set_float_2(this_obj, o.x, o.y) == 1;
        }

        public static JSValue js_push_structvalue(JSContext ctx, Vector2 o)
        {
            var proto = FindPrototypeOf<Vector2>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 2);
            JSApi.jsb_set_float_2(val, o.x, o.y);
            return val;
        }

        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Vector2 o)
        {
            float x, y;
            var ret = JSApi.jsb_get_float_2(val, out x, out y);
            o = new Vector2(x, y);
            return ret != 0;
        }

        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Vector2? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            float x, y;
            var ret = JSApi.jsb_get_float_2(val, out x, out y);
            o = new Vector2(x, y);
            return ret != 0;
        }

    }
}
#endif