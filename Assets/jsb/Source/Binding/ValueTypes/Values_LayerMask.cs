using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using UnityEngine;
    using Native;

    public partial class Values
    {
        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, LayerMask o, int type_id, bool disposable)
        {
            var val = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(int) * 1);
            if (!JSApi.JS_IsException(val))
            {
                JSApi.jsb_set_int_1(val, o.value);
            }
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, ref LayerMask o)
        {
            return JSApi.jsb_set_int_1(this_obj, o.value) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, LayerMask o)
        {
            var proto = FindPrototypeOf<LayerMask>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(int) * 1);
            JSApi.jsb_set_int_1(val, o.value);
            return val;
        }

        public static JSValue js_push_structvalue(JSContext ctx, LayerMask? o)
        {
            if (o == null)
            {
                return JSApi.JS_NULL;
            }
            var proto = FindPrototypeOf<LayerMask>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(int) * 1);
            JSApi.jsb_set_int_1(val, ((LayerMask)o).value);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out LayerMask o)
        {
            int pres;
            JSApi.JS_ToInt32(ctx, out pres, val);
            o = (LayerMask)pres; // no check
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out LayerMask? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            int pres;
            JSApi.JS_ToInt32(ctx, out pres, val);
            o = (LayerMask)pres; // no check
            return true;
        }

    }
}
