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
        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, Color o, int type_id, bool disposable)
        {
            var val = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(float) * 4);
            if (!JSApi.JS_IsException(val))
            {
                JSApi.jsb_set_float_4(val, o.r, o.g, o.b, o.a);
            }
            return val;
        }

        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, ref Color o)
        {
            return JSApi.jsb_set_float_4(this_obj, o.r, o.g, o.b, o.a) == 1;
        }

        public static JSValue js_push_structvalue(JSContext ctx, Color o)
        {
            var proto = FindPrototypeOf<Color>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 4);
            JSApi.jsb_set_float_4(val, o.r, o.g, o.b, o.a);
            return val;
        }

        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Color o)
        {
            float r, g, b, a;
            var ret = JSApi.jsb_get_float_4(val, out r, out g, out b, out a);
            o = new Color(r, g, b, a);
            return ret != 0;
        }

        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Color? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            float r, g, b, a;
            var ret = JSApi.jsb_get_float_4(val, out r, out g, out b, out a);
            o = new Color(r, g, b, a);
            return ret != 0;
        }

    }
}
#endif