using System;

namespace QuickJS.Binding
{
    using Native;

    public partial class Values
    {
        //TODO: use type-indexed handler map
        // type: expected type of object o
        public static bool js_get_var(JSContext ctx, JSValue val, Type type, out object o)
        {
            if (type.IsValueType)
            {
                if (type.IsPrimitive)
                {
                    if (type == typeof(bool))
                    {
                        bool rval;
                        var rs = js_get_primitive(ctx, val, out rval);
                        o = rval;
                        return rs;
                    }
                    if (type == typeof(byte))
                    {
                        byte rval;
                        var rs = js_get_primitive(ctx, val, out rval);
                        o = rval;
                        return rs;
                    }
                    if (type == typeof(char))
                    {
                        char rval;
                        var rs = js_get_primitive(ctx, val, out rval);
                        o = rval;
                        return rs;
                    }
                    if (type == typeof(sbyte))
                    {
                        sbyte rval;
                        var rs = js_get_primitive(ctx, val, out rval);
                        o = rval;
                        return rs;
                    }
                    if (type == typeof(short))
                    {
                        short rval;
                        var rs = js_get_primitive(ctx, val, out rval);
                        o = rval;
                        return rs;
                    }
                    if (type == typeof(ushort))
                    {
                        ushort rval;
                        var rs = js_get_primitive(ctx, val, out rval);
                        o = rval;
                        return rs;
                    }
                    if (type == typeof(int))
                    {
                        int rval;
                        var rs = js_get_primitive(ctx, val, out rval);
                        o = rval;
                        return rs;
                    }
                    if (type == typeof(uint))
                    {
                        uint rval;
                        var rs = js_get_primitive(ctx, val, out rval);
                        o = rval;
                        return rs;
                    }
                    if (type == typeof(long))
                    {
                        long rval;
                        var rs = js_get_primitive(ctx, val, out rval);
                        o = rval;
                        return rs;
                    }
                    if (type == typeof(ulong))
                    {
                        ulong rval;
                        var rs = js_get_primitive(ctx, val, out rval);
                        o = rval;
                        return rs;
                    }
                    if (type == typeof(float))
                    {
                        float rval;
                        var rs = js_get_primitive(ctx, val, out rval);
                        o = rval;
                        return rs;
                    }
                    if (type == typeof(double))
                    {
                        double rval;
                        var rs = js_get_primitive(ctx, val, out rval);
                        o = rval;
                        return rs;
                    }
                }
                else
                {
                    if (type.IsEnum)
                    {
                        return js_get_enumvalue(ctx, val, type, out o);
                    }
                }
            }
            else
            {
                if (type == typeof(string))
                {
                    string rval;
                    var rs = js_get_primitive(ctx, val, out rval);
                    o = rval;
                    return rs;
                }
            }

            if (val.IsNullish())
            {
                o = null;
                return true;
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
