using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using Native;

    // 处理常规值, class, struct
    public partial class Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out IntPtr o)
        {
            object o_t;
            var ret = js_get_object(ctx, val, out o_t);
            o = (IntPtr)o_t;
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out IntPtr? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            object o_t;
            var ret = js_get_object(ctx, val, out o_t);
            o = (IntPtr)o_t;
            return ret;
        }

        public static bool js_get_primitive_array(JSContext ctx, JSValue val, out IntPtr[] o)
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new IntPtr[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    IntPtr e;
                    if (js_get_primitive(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<IntPtr[]>(ctx, val, out o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out bool o)
        {
#if JSB_STRICT
            if (!val.IsBoolean())
            {
                o = false;
                return false;
            }
#endif
            var r = JSApi.JS_ToBool(ctx, val);
            o = r != 0;
            return r >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out bool? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
#if JSB_STRICT
            if (!val.IsBoolean())
            {
                o = null;
                return false;
            }
#endif
            var r = JSApi.JS_ToBool(ctx, val);
            o = r != 0;
            return r >= 0;
        }

        public static bool js_get_primitive_array(JSContext ctx, JSValue val, out bool[] o)
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new bool[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    bool e;
                    if (js_get_primitive(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<bool[]>(ctx, val, out o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out sbyte o)
        {
            int pres;
            JSApi.JS_ToInt32(ctx, out pres, val);
            o = (sbyte)pres; // no check
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out sbyte? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            int pres;
            JSApi.JS_ToInt32(ctx, out pres, val);
            o = (sbyte)pres; // no check
            return true;
        }

        public static bool js_get_primitive_array(JSContext ctx, JSValue val, out sbyte[] o)
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new sbyte[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    sbyte e;
                    if (js_get_primitive(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<sbyte[]>(ctx, val, out o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out byte o)
        {
            int pres;
            JSApi.JS_ToInt32(ctx, out pres, val);
            o = (byte)pres; // no check
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out byte? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            int pres;
            JSApi.JS_ToInt32(ctx, out pres, val);
            o = (byte)pres; // no check
            return true;
        }

        public static bool js_get_primitive_array(JSContext ctx, JSValue val, out byte[] o)
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new byte[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    byte e;
                    if (js_get_primitive(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }

            // check ArrayBuffer 
            size_t psize;
            var pbuf = JSApi.JS_GetArrayBuffer(ctx, out psize, val);

            if (pbuf != IntPtr.Zero)
            {
                o = new byte[psize];
                Marshal.Copy(pbuf, o, 0, psize);
                return true;
            }

            // check TypedArray
            var asBuffer = JSApi.JS_GetProperty(ctx, val, ScriptEngine.GetContext(ctx).GetAtom("buffer"));
            if (asBuffer.IsObject())
            {
                pbuf = JSApi.JS_GetArrayBuffer(ctx, out psize, asBuffer);
                JSApi.JS_FreeValue(ctx, asBuffer);

                if (pbuf != IntPtr.Zero)
                {
                    o = new byte[psize];
                    Marshal.Copy(pbuf, o, 0, psize);
                    return true;
                }
            }
            else
            {
                JSApi.JS_FreeValue(ctx, asBuffer);
            }

            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<byte[]>(ctx, val, out o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out char o)
        {
            int pres;
            JSApi.JS_ToInt32(ctx, out pres, val);
            o = (char)pres; // no check
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out char? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            int pres;
            JSApi.JS_ToInt32(ctx, out pres, val);
            o = (char)pres; // no check
            return true;
        }

        public static bool js_get_primitive_array(JSContext ctx, JSValue val, out char[] o)
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new char[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    char e;
                    if (js_get_primitive(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<char[]>(ctx, val, out o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out string o)
        {
            //TODO: make this behaviour configurable?
            var context = ScriptEngine.GetContext(ctx);
            var cache = context.GetStringCache();
            if (cache.TryGetValue(val, out o))
            {
                return true;
            }

            o = JSApi.GetString(ctx, val); // no check
            return true;
        }

        public static bool js_get_primitive_array(JSContext ctx, JSValue val, out string[] o)
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new string[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    string e;
                    if (js_get_primitive(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<string[]>(ctx, val, out o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out short o)
        {
            int pres;
            JSApi.JS_ToInt32(ctx, out pres, val);
            o = (short)pres; // no check
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out short? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            int pres;
            JSApi.JS_ToInt32(ctx, out pres, val);
            o = (short)pres; // no check
            return true;
        }

        public static bool js_get_primitive_array(JSContext ctx, JSValue val, out short[] o)
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new short[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    short e;
                    if (js_get_primitive(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<short[]>(ctx, val, out o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out ushort o)
        {
            int pres;
            JSApi.JS_ToInt32(ctx, out pres, val);
            o = (ushort)pres; // no check
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out ushort? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            int pres;
            JSApi.JS_ToInt32(ctx, out pres, val);
            o = (ushort)pres; // no check
            return true;
        }

        public static bool js_get_primitive_array(JSContext ctx, JSValue val, out ushort[] o)
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new ushort[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    ushort e;
                    if (js_get_primitive(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<ushort[]>(ctx, val, out o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out int o)
        {
            int pres;
            JSApi.JS_ToInt32(ctx, out pres, val);
            o = pres; // no check
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out int? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            int pres;
            JSApi.JS_ToInt32(ctx, out pres, val);
            o = pres; // no check
            return true;
        }

        public static bool js_get_primitive_array(JSContext ctx, JSValue val, out int[] o)
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new int[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    int e;
                    if (js_get_primitive(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<int[]>(ctx, val, out o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out uint o)
        {
            uint pres;
            JSApi.JSB_ToUint32(ctx, out pres, val);
            o = pres; // no check
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out uint? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            uint pres;
            JSApi.JSB_ToUint32(ctx, out pres, val);
            o = pres; // no check
            return true;
        }

        public static bool js_get_primitive_array(JSContext ctx, JSValue val, out uint[] o)
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new uint[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    uint e;
                    if (js_get_primitive(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<uint[]>(ctx, val, out o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out long o)
        {
            long pres;
            JSApi.JS_ToInt64(ctx, out pres, val);
            o = pres; // no check
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out long? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            long pres;
            JSApi.JS_ToInt64(ctx, out pres, val);
            o = pres; // no check
            return true;
        }

        public static bool js_get_primitive_array(JSContext ctx, JSValue val, out long[] o)
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new long[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    long e;
                    if (js_get_primitive(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<long[]>(ctx, val, out o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out ulong o)
        {
            ulong pres;
            JSApi.JS_ToIndex(ctx, out pres, val);
            o = pres; // no check
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out ulong? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            ulong pres;
            JSApi.JS_ToIndex(ctx, out pres, val);
            o = pres; // no check
            return true;
        }

        public static bool js_get_primitive_array(JSContext ctx, JSValue val, out ulong[] o)
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new ulong[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    ulong e;
                    if (js_get_primitive(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<ulong[]>(ctx, val, out o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out float o)
        {
#if JSB_STRICT
            if (!val.IsNumber())
            {
                o = 0f;
                return false;
            }
#endif
            double pres;
            var res = JSApi.JS_ToFloat64(ctx, out pres, val);
            o = (float)pres; // no check
            return res == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out float? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
#if JSB_STRICT
            if (!val.IsNumber())
            {
                o = null;
                return false;
            }
#endif
            double pres;
            var res = JSApi.JS_ToFloat64(ctx, out pres, val);
            o = (float)pres; // no check
            return res == 0;
        }

        public static bool js_get_primitive_array(JSContext ctx, JSValue val, out float[] o)
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new float[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    float e;
                    if (js_get_primitive(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<float[]>(ctx, val, out o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out double o)
        {
            double pres;
            JSApi.JS_ToFloat64(ctx, out pres, val);
            o = pres; // no check
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_primitive(JSContext ctx, JSValue val, out double? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            double pres;
            JSApi.JS_ToFloat64(ctx, out pres, val);
            o = pres; // no check
            return true;
        }

        public static bool js_get_primitive_array(JSContext ctx, JSValue val, out double[] o)
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new double[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    double e;
                    if (js_get_primitive(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<double[]>(ctx, val, out o);
        }

        // fallthrough
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue<T>(JSContext ctx, JSValue val, out T o)
        where T : struct
        {
            object o_t;
            var ret = js_get_object(ctx, val, out o_t);
            o = (T)o_t;
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue<T>(JSContext ctx, JSValue val, out T? o)
        where T : struct
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            object o_t;
            var ret = js_get_object(ctx, val, out o_t);
            o = (T)o_t;
            return ret;
        }

        public static bool js_get_structvalue_array<T>(JSContext ctx, JSValue val, out T[] o)
        where T : struct
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new T[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    T e;
                    if (js_get_structvalue(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<T[]>(ctx, val, out o);
        }

        public static bool js_get_structvalue_array<T>(JSContext ctx, JSValue val, out T?[] o)
        where T : struct
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new T?[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    T? e;
                    if (js_get_structvalue(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<T?[]>(ctx, val, out o);
        }

        public static bool js_get_classvalue(JSContext ctx, JSValue val, out Delegate o)
        {
            return js_get_delegate_unsafe(ctx, val, out o);
        }

        // not value type (except string/array)
        public static bool js_get_classvalue<T>(JSContext ctx, JSValue val, out T o)
        where T : class
        {
            object o_t;
            if (js_get_cached_object(ctx, val, out o_t))
            {
                o = o_t as T;
                if (o_t != null && o == null)
                {
                    // throw new InvalidCastException(string.Format("{0} type mismatch {1}", o_t.GetType(), typeof(T)));
                    return false;
                }
                return true;
            }
            // var type_id = JSApi.JSB_GetBridgeType(ctx, val);
            // if (type_id >= 0)
            // {
            //     var valType = ScriptEngine.GetTypeDB(ctx).GetType(type_id);
            //     throw new InvalidCastException(string.Format("{0} type mismatch {1}", valType, typeof(T)));
            // }
            // var jsType = val.tag;
            // throw new InvalidCastException(string.Format("{0} type mismatch {1}", jsType, typeof(T)));
            o = default(T);
            return false;
        }

        public static bool js_get_classvalue<T>(JSContext ctx, JSValue val, out TypedScriptPromise<T> o)
        {
            ScriptPromise value;
            if (js_get_classvalue(ctx, val, out value))
            {
                o = value as TypedScriptPromise<T>;
                return true;
            }
            o = null;
            return false;
        }

        public static bool js_get_classvalue(JSContext ctx, JSValue val, out AnyScriptPromise o)
        {
            ScriptPromise value;
            if (js_get_classvalue(ctx, val, out value))
            {
                o = value as AnyScriptPromise;
                return true;
            }
            o = null;
            return false;
        }

        public static bool js_get_classvalue(JSContext ctx, JSValue val, out ScriptPromise o)
        {
            var context = ScriptEngine.GetContext(ctx);
            var cache = context.GetObjectCache();

            if (cache.TryGetScriptPromise(val, out o))
            {
                return true;
            }

            // 不能构造一个 JS Promise
            o = null;
            return false;
        }

        public static bool js_get_classvalue(JSContext ctx, JSValue val, out ScriptValue o)
        {
            var context = ScriptEngine.GetContext(ctx);
            var cache = context.GetObjectCache();

            if (cache.TryGetScriptValue(val, out o))
            {
                return true;
            }

            o = new ScriptValue(context, val);
            return true;
        }

        public static bool js_get_classvalue(JSContext ctx, JSValue val, out ScriptFunction o)
        {
            if (JSApi.JS_IsFunction(ctx, val) != 0)
            {
                var context = ScriptEngine.GetContext(ctx);
                var cache = context.GetObjectCache();
                if (cache.TryGetScriptValue(val, out o))
                {
                    return true;
                }

                o = new ScriptFunction(ScriptEngine.GetContext(ctx), val);
                return true;
            }

            o = null;
            return false;
        }

        public static bool js_get_classvalue(JSContext ctx, JSValue val, out QuickJS.IO.ByteBuffer o)
        {
            object obj;
            if (js_get_cached_object(ctx, val, out obj))
            {
                if (obj is QuickJS.IO.ByteBuffer)
                {
                    o = (QuickJS.IO.ByteBuffer)obj;
                    return true;
                }
            }
            size_t psize;
            var pointer = JSApi.JS_GetArrayBuffer(ctx, out psize, val);
            if (pointer != IntPtr.Zero)
            {
                var runtime = ScriptEngine.GetRuntime(ctx);
                var allocator = runtime.GetByteBufferAllocator();
                if (allocator != null)
                {
                    var length = (int)psize;

                    o = allocator.Alloc(length);
                    runtime.AutoRelease(o);
                    o.WriteBytes(pointer, length);
                    return true;
                }
            }

            var asBuffer = JSApi.JS_GetProperty(ctx, val, ScriptEngine.GetContext(ctx).GetAtom("buffer"));
            if (asBuffer.IsObject())
            {
                pointer = JSApi.JS_GetArrayBuffer(ctx, out psize, asBuffer);
                JSApi.JS_FreeValue(ctx, asBuffer);

                if (pointer != IntPtr.Zero)
                {
                    var runtime = ScriptEngine.GetRuntime(ctx);
                    var allocator = runtime.GetByteBufferAllocator();
                    if (allocator != null)
                    {
                        var length = (int)psize;

                        o = allocator.Alloc(length);
                        runtime.AutoRelease(o);
                        o.WriteBytes(pointer, length);
                        return true;
                    }
                }
            }
            else
            {
                JSApi.JS_FreeValue(ctx, asBuffer);
            }

            o = null;
            return false;
        }

        public static bool js_get_cached_object(JSContext ctx, JSValue val, out object o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            var header = JSApi.jsb_get_payload_header(val);
            switch (header.type_id)
            {
                case BridgeObjectType.ObjectRef:
                    return ScriptEngine.GetObjectCache(ctx).TryGetObject(header.value, out o);
                case BridgeObjectType.TypeRef:
                    o = ScriptEngine.GetTypeDB(ctx).GetType(header.value);
                    return o != null;
                case BridgeObjectType.ValueType:
                    o = null;
                    return false;
            }

            o = null;
            return false;
        }

        // 只处理 JS_OBJECT
        public static bool js_get_object(JSContext ctx, JSValue val, out object o)
        {
            if (JSApi.JS_IsObject(val))
            {
                return js_get_cached_object(ctx, val, out o);
            }
            // Debug.LogFormat("js_get_object({0})", jstype);
            //     case duk_type_t.DUK_TYPE_STRING:
            //         o = JSApi.js_get_string(ctx, idx);
            //         return true;
            //     default: break;
            // }
            // 其他类型不存在对象映射
            o = null;
            return false;
        }

        public static bool js_get_classvalue_array<T>(JSContext ctx, JSValue val, out T[] o)
        where T : class
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new T[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    T e;
                    if (js_get_classvalue(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<T[]>(ctx, val, out o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_enumvalue<T>(JSContext ctx, JSValue val, out T o)
        where T : Enum
        {
            int v;
            var ret = js_get_primitive(ctx, val, out v);
            o = (T)Enum.ToObject(typeof(T), v);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_enumvalue(JSContext ctx, JSValue val, Type type, out object o)
        {
            int v;
            var ret = js_get_primitive(ctx, val, out v);
            o = Enum.ToObject(type, v);
            return ret;
        }

        public static bool js_get_enumvalue_array<T>(JSContext ctx, JSValue val, out T[] o)
        where T : Enum
        {
            var isArray = JSApi.JS_IsArray(ctx, val);
            if (isArray == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    o = null;
                    return js_script_error(ctx);
                }
                int length;
                JSApi.JS_ToInt32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new T[length];
                for (var i = 0U; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    T e;
                    if (js_get_enumvalue(ctx, eVal, out e))
                    {
                        o[i] = e;
                        JSApi.JS_FreeValue(ctx, eVal);
                    }
                    else
                    {
                        o = null;
                        JSApi.JS_FreeValue(ctx, eVal);
                        return false;
                    }
                }
                return true;
            }
            if (isArray == -1)
            {
                o = null;
                return false;
            }
            return js_get_classvalue<T[]>(ctx, val, out o);
        }
    }
}
