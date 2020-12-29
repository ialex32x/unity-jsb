using System;
using System.Collections.Generic;

namespace QuickJS.Binding
{
    using Native;

    public partial class Values
    {
        public delegate bool JSValueCast(JSContext ctx, JSValue val, out object o);

        // 用于根据 Type 信息将 JSValue 专为对应的 CS Object
        private static Dictionary<Type, JSValueCast> _JSCastMap = new Dictionary<Type, JSValueCast>();

        // type: expected type of object o
        public static bool js_get_var(JSContext ctx, JSValue val, Type type, out object o)
        {
            //TODO: 处理数组
            // if (type.IsArray)
            // {
            // }

            if (type.BaseType == typeof(MulticastDelegate))
            {
                Delegate d;
                var rs = js_get_delegate(ctx, val, type, out d);
                o = d;
                return rs;
            }

            if (type.IsEnum)
            {
                return js_get_enumvalue(ctx, val, type, out o);
            }

            JSValueCast cast;
            if (_JSCastMap.TryGetValue(type, out cast))
            {
                return cast(ctx, val, out o);
            }

            if (val.IsObject())
            {
                return js_get_cached_object(ctx, val, out o);
            }

            //TODO: 在期望类型非常宽泛的情况下(比如 object), 怎么合理自适应处理
            if (val.IsString() && type.IsAssignableFrom(typeof(string)))
            {
                return js_value_cast_string(ctx, val, out o);
            }

            o = null;
            return false;
        }

        // 初始化, 在 Values 静态构造时调用
        private static void init_js_cast_map()
        {
            _JSCastMap[typeof(void)] = js_value_cast_void;
            _JSCastMap[typeof(bool)] = js_value_cast_bool;
            _JSCastMap[typeof(byte)] = js_value_cast_byte;
            _JSCastMap[typeof(char)] = js_value_cast_char;
            _JSCastMap[typeof(sbyte)] = js_value_cast_sbyte;
            _JSCastMap[typeof(short)] = js_value_cast_short;
            _JSCastMap[typeof(ushort)] = js_value_cast_ushort;
            _JSCastMap[typeof(int)] = js_value_cast_int;
            _JSCastMap[typeof(uint)] = js_value_cast_uint;
            _JSCastMap[typeof(long)] = js_value_cast_long;
            _JSCastMap[typeof(ulong)] = js_value_cast_ulong;
            _JSCastMap[typeof(float)] = js_value_cast_float;
            _JSCastMap[typeof(double)] = js_value_cast_double;
            _JSCastMap[typeof(string)] = js_value_cast_string;
            _JSCastMap[typeof(Type)] = js_value_cast_type;
            _JSCastMap[typeof(ScriptValue)] = js_value_cast_script_value;
        }

        private static bool js_value_cast_void(JSContext ctx, JSValue val, out object o)
        {
            o = null;
            return true;
        }

        private static bool js_value_cast_bool(JSContext ctx, JSValue val, out object o)
        {
            bool rval;
            var rs = js_get_primitive(ctx, val, out rval);
            o = rval;
            return rs;
        }

        private static bool js_value_cast_byte(JSContext ctx, JSValue val, out object o)
        {
            byte rval;
            var rs = js_get_primitive(ctx, val, out rval);
            o = rval;
            return rs;
        }

        private static bool js_value_cast_char(JSContext ctx, JSValue val, out object o)
        {
            char rval;
            var rs = js_get_primitive(ctx, val, out rval);
            o = rval;
            return rs;
        }

        private static bool js_value_cast_sbyte(JSContext ctx, JSValue val, out object o)
        {
            sbyte rval;
            var rs = js_get_primitive(ctx, val, out rval);
            o = rval;
            return rs;
        }

        private static bool js_value_cast_short(JSContext ctx, JSValue val, out object o)
        {
            short rval;
            var rs = js_get_primitive(ctx, val, out rval);
            o = rval;
            return rs;
        }

        private static bool js_value_cast_ushort(JSContext ctx, JSValue val, out object o)
        {
            ushort rval;
            var rs = js_get_primitive(ctx, val, out rval);
            o = rval;
            return rs;
        }

        private static bool js_value_cast_int(JSContext ctx, JSValue val, out object o)
        {
            int rval;
            var rs = js_get_primitive(ctx, val, out rval);
            o = rval;
            return rs;
        }

        private static bool js_value_cast_uint(JSContext ctx, JSValue val, out object o)
        {
            uint rval;
            var rs = js_get_primitive(ctx, val, out rval);
            o = rval;
            return rs;
        }

        private static bool js_value_cast_long(JSContext ctx, JSValue val, out object o)
        {
            long rval;
            var rs = js_get_primitive(ctx, val, out rval);
            o = rval;
            return rs;
        }

        private static bool js_value_cast_ulong(JSContext ctx, JSValue val, out object o)
        {
            ulong rval;
            var rs = js_get_primitive(ctx, val, out rval);
            o = rval;
            return rs;
        }

        private static bool js_value_cast_float(JSContext ctx, JSValue val, out object o)
        {
            float rval;
            var rs = js_get_primitive(ctx, val, out rval);
            o = rval;
            return rs;
        }

        private static bool js_value_cast_double(JSContext ctx, JSValue val, out object o)
        {
            double rval;
            var rs = js_get_primitive(ctx, val, out rval);
            o = rval;
            return rs;
        }

        private static bool js_value_cast_string(JSContext ctx, JSValue val, out object o)
        {
            string rval;
            var rs = js_get_primitive(ctx, val, out rval);
            o = rval;
            return rs;
        }

        private static bool js_value_cast_type(JSContext ctx, JSValue val, out object o)
        {
            Type rval;
            var rs = js_get_type(ctx, val, out rval);
            o = rval;
            return rs;
        }

        private static bool js_value_cast_script_value(JSContext ctx, JSValue val, out object o)
        {
            ScriptValue rval;
            var rs = js_get_classvalue(ctx, val, out rval);
            o = rval;
            return rs;
        }
    }
}
