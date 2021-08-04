using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using Native;
    using UnityEngine;

    public partial class Values
    {
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out FairyGUI.Margin o)
        {
            int r, g, b, a;
            var ret = JSApi.jsb_get_int_4(val, out r, out g, out b, out a);
            o = new FairyGUI.Margin();
            o.left = r;
            o.right = g;
            o.top = b;
            o.bottom = a;
            return ret != 0;
        }

        public static JSValue js_push_structvalue(JSContext ctx, FairyGUI.Margin o)
        {
            var proto = FindPrototypeOf<Rect>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(int) * 4);
            JSApi.jsb_set_int_4(val, o.left, o.right, o.top, o.bottom);
            return val;
        }
    }
}