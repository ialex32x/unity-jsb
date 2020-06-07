using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using Native;
    using UnityEngine;

    public partial class Values
    {
        public static bool js_get_var(JSContext ctx, JSValue val, out object o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }

            if (val.IsBoolean())
            {
                bool rval;
                var rs = js_get_primitive(ctx, val, out rval);
                o = rval;
                return rs;
            }

            if (val.IsString())
            {
                string rval;
                var rs = js_get_primitive(ctx, val, out rval);
                o = rval;
                return rs;
            }

            if (val.IsNumber())
            {
                double rval;
                var rs = js_get_primitive(ctx, val, out rval);
                o = rval;
                return rs;
            }

            if (val.IsObject())
            {
                return js_get_cached_object(ctx, val, out o);
            }

            o = null;
            return false;
        }
    }
}
