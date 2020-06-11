using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using Native;
    using UnityEngine;

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
            double pres;
            JSApi.JS_ToFloat64(ctx, out pres, val);
            o = (float)pres; // no check
            return true;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out LayerMask o)
        {
            int pres;
            JSApi.JS_ToInt32(ctx, out pres, val);
            o = (LayerMask)pres; // no check
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Color o)
        {
            float r, g, b, a;
            var ret = JSApi.jsb_get_float_4(val, out r, out g, out b, out a);
            o = new Color(r, g, b, a);
            return ret != 0;
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
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Vector2 o)
        {
            float x, y;
            var ret = JSApi.jsb_get_float_2(val, out x, out y);
            o = new Vector2(x, y);
            return ret != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Vector2Int o)
        {
            int x, y;
            var ret = JSApi.jsb_get_int_2(val, out x, out y);
            o = new Vector2Int(x, y);
            return ret != 0;
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
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Vector3Int o)
        {
            int x, y, z;
            var ret = JSApi.jsb_get_int_3(val, out x, out y, out z);
            o = new Vector3Int(x, y, z);
            return ret != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Vector4 o)
        {
            float x, y, z, w;
            var ret = JSApi.jsb_get_float_4(val, out x, out y, out z, out w);
            o = new Vector4(x, y, z, w);
            return ret != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Quaternion o)
        {
            float x, y, z, w;
            var ret = JSApi.jsb_get_float_4(val, out x, out y, out z, out w);
            o = new Quaternion(x, y, z, w);
            return ret != 0;
        }

        public static unsafe bool js_get_structvalue(JSContext ctx, JSValue val, out Matrix4x4 o)
        {
            int ret;
            fixed (float* ptr = _matrix_floats_buffer)
            {
                ret = JSApi.jsb_get_floats(val, 16, ptr);
            }
            var c0 = new Vector4(_matrix_floats_buffer[0], _matrix_floats_buffer[1], _matrix_floats_buffer[2], _matrix_floats_buffer[3]);
            var c1 = new Vector4(_matrix_floats_buffer[4], _matrix_floats_buffer[5], _matrix_floats_buffer[6], _matrix_floats_buffer[7]);
            var c2 = new Vector4(_matrix_floats_buffer[8], _matrix_floats_buffer[8], _matrix_floats_buffer[10], _matrix_floats_buffer[11]);
            var c3 = new Vector4(_matrix_floats_buffer[12], _matrix_floats_buffer[13], _matrix_floats_buffer[14], _matrix_floats_buffer[15]);
            o = new Matrix4x4(c0, c1, c2, c3);
            return ret != 0;
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

        public static bool js_get_classvalue(JSContext ctx, JSValue val, out ScriptValue o)
        {
            object obj;
            if (js_get_cached_object(ctx, val, out obj))
            {
                if (obj is ScriptValue)
                {
                    o = (ScriptValue)obj;
                    return true;
                }
            }
            if (JSApi.JS_IsObject(val))
            {
                o = new ScriptValue(ScriptEngine.GetContext(ctx), val);
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
                var allocator = ScriptEngine.GetRuntime(ctx).GetByteBufferAllocator();
                if (allocator != null)
                {
                    var length = (int)psize;
                    if (length > 0)
                    {
                        o = allocator.Alloc(length);
                        allocator.AutoRelease(o);
                        o.WriteBytes(pointer, length);
                        return true;
                    }
                }
            }
            o = null;
            return false;
        }

        public static bool js_get_classvalue(JSContext ctx, JSValue val, out ScriptValueArray o)
        {
            object obj;
            if (js_get_cached_object(ctx, val, out obj))
            {
                if (obj is ScriptValueArray)
                {
                    o = (ScriptValueArray)obj;
                    return true;
                }
            }
            if (JSApi.JS_IsArray(ctx, val) != 0)
            {
                o = new ScriptValueArray(ScriptEngine.GetContext(ctx), val);
                return true;
            }
            o = null;
            return false;
        }

        public static bool js_get_classvalue(JSContext ctx, JSValue val, out ScriptFunction o)
        {
            if (JSApi.JS_IsFunction(ctx, val) != 0)
            {
                object obj;
                if (js_get_cached_object(ctx, val, out obj))
                {
                    if (obj is ScriptFunction)
                    {
                        o = (ScriptFunction)obj;
                        return true;
                    }
                }

                o = new ScriptFunction(ScriptEngine.GetContext(ctx), val);
                return true;
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
            }

            //TODO: if o is Delegate, try get from delegate cache list
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
