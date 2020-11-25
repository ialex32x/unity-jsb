using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using UnityEngine;
    using Native;

    public partial class Values
    {
        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, Color32 o, int type_id, bool disposable)
        {
            var val = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(int) * 4);
            if (!JSApi.JS_IsException(val))
            {
                JSApi.jsb_set_int_4(val, o.r, o.g, o.b, o.a);
            }
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, ref Color32 o)
        {
            return JSApi.jsb_set_int_4(this_obj, o.r, o.g, o.b, o.a) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, ref Color32 o)
        {
            var proto = FindPrototypeOf<Color32>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(byte) * 4);
            JSApi.jsb_set_byte_4(val, o.r, o.g, o.b, o.a);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Color32 o)
        {
            int r, g, b, a;
            var ret = JSApi.jsb_get_int_4(val, out r, out g, out b, out a);
            o = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
            return ret != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Color32? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            int r, g, b, a;
            var ret = JSApi.jsb_get_int_4(val, out r, out g, out b, out a);
            o = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
            return ret != 0;
        }

    }
}
