using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using Native;
    using UnityEngine;

    // 处理 unity 相关常用类型
    public partial class Values
    {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Color o)
        {
            float r, g, b, a;
            var ret = JSApi.jsb_get_float_4(val, out r, out g, out b, out a);
            o = new Color(r, g, b, a);
            return ret != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Vector2 o)
        {
            float x, y;
            var ret = JSApi.jsb_get_float_2(val, out x, out y);
            o = new Vector2(x, y);
            return ret != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Vector2? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
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
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Vector2Int? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Vector3Int o)
        {
            int x, y, z;
            var ret = JSApi.jsb_get_int_3(val, out x, out y, out z);
            o = new Vector3Int(x, y, z);
            return ret != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Vector3Int? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
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
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Vector4? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_get_structvalue(JSContext ctx, JSValue val, out Quaternion? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
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

        public static unsafe bool js_get_structvalue(JSContext ctx, JSValue val, out Matrix4x4? o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
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
    }
}
