using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using UnityEngine;
    using Native;

    public partial class Values
    {
        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, Vector3 o, int type_id, bool disposable)
        {
            var val = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(float) * 3);
            if (!JSApi.JS_IsException(val))
            {
                JSApi.jsb_set_float_3(val, o.x, o.y, o.z);
            }
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, ref Vector3 o)
        {
            return JSApi.jsb_set_float_3(this_obj, o.x, o.y, o.z) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Vector3 o)
        {
            var proto = FindPrototypeOf<Vector3>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 3);
            JSApi.jsb_set_float_3(val, o.x, o.y, o.z);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Vector3 o)
        {
            float x, y, z;
            var ret = JSApi.jsb_get_float_3(val, out x, out y, out z);
            o = new Vector3(x, y, z);
            return ret != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Vector3? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            float x, y, z;
            var ret = JSApi.jsb_get_float_3(val, out x, out y, out z);
            o = new Vector3(x, y, z);
            return ret != 0;
        }

    }
}
