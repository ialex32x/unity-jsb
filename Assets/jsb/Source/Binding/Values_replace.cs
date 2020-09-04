using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using UnityEngine;
    using Native;

    public partial class Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, LayerMask o)
        {
            return JSApi.jsb_set_int_1(this_obj, o.value) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, Vector2 o)
        {
            return JSApi.jsb_set_float_2(this_obj, o.x, o.y) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, Vector2Int o)
        {
            return JSApi.jsb_set_int_2(this_obj, o.x, o.y) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, Color o)
        {
            return JSApi.jsb_set_float_4(this_obj, o.r, o.g, o.b, o.a) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, Color32 o)
        {
            return JSApi.jsb_set_int_4(this_obj, o.r, o.g, o.b, o.a) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, Vector3 o)
        {
            return JSApi.jsb_set_float_3(this_obj, o.x, o.y, o.z) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, Vector3Int o)
        {
            return JSApi.jsb_set_int_3(this_obj, o.x, o.y, o.z) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, Vector4 o)
        {
            return JSApi.jsb_set_float_4(this_obj, o.x, o.y, o.z, o.w) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, Quaternion o)
        {
            return JSApi.jsb_set_float_4(this_obj, o.x, o.y, o.z, o.w) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool js_rebind_this(JSContext ctx, JSValue this_obj, Matrix4x4 o)
        {
            _matrix_floats_buffer[0] = o.m00;
            _matrix_floats_buffer[1] = o.m10;
            _matrix_floats_buffer[2] = o.m20;
            _matrix_floats_buffer[3] = o.m30;

            _matrix_floats_buffer[4] = o.m01;
            _matrix_floats_buffer[5] = o.m11;
            _matrix_floats_buffer[6] = o.m21;
            _matrix_floats_buffer[7] = o.m31;

            _matrix_floats_buffer[8] = o.m02;
            _matrix_floats_buffer[9] = o.m12;
            _matrix_floats_buffer[10] = o.m22;
            _matrix_floats_buffer[11] = o.m32;

            _matrix_floats_buffer[12] = o.m03;
            _matrix_floats_buffer[13] = o.m13;
            _matrix_floats_buffer[14] = o.m23;
            _matrix_floats_buffer[15] = o.m33;

            fixed (float* ptr = _matrix_floats_buffer)
            {
                return JSApi.jsb_set_floats(this_obj, 4 * 4, ptr) == 1;
            }
        }

        // public static bool js_rebind_this<T>(JSContext ctx, JSValue this_obj, ref T o)
        // where T : struct
        // {
        //     //X-TODO: type rebind-op map
        // }

        // fallback
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, object o)
        {
            var header = JSApi.jsb_get_payload_header(this_obj);
            switch (header.type_id)
            {
                case BridgeObjectType.ObjectRef:
                    return ScriptEngine.GetObjectCache(ctx).ReplaceObject(header.value, o);
            }
            return false;
        }
    }
}