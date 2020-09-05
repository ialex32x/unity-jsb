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
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, ref LayerMask o)
        {
            return JSApi.jsb_set_int_1(this_obj, o.value) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, ref Vector2 o)
        {
            return JSApi.jsb_set_float_2(this_obj, o.x, o.y) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, ref Vector2Int o)
        {
            return JSApi.jsb_set_int_2(this_obj, o.x, o.y) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, ref Color o)
        {
            return JSApi.jsb_set_float_4(this_obj, o.r, o.g, o.b, o.a) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, ref Color32 o)
        {
            return JSApi.jsb_set_int_4(this_obj, o.r, o.g, o.b, o.a) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, ref Vector3 o)
        {
            return JSApi.jsb_set_float_3(this_obj, o.x, o.y, o.z) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, ref Vector3Int o)
        {
            return JSApi.jsb_set_int_3(this_obj, o.x, o.y, o.z) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, ref Vector4 o)
        {
            return JSApi.jsb_set_float_4(this_obj, o.x, o.y, o.z, o.w) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, ref Quaternion o)
        {
            return JSApi.jsb_set_float_4(this_obj, o.x, o.y, o.z, o.w) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool js_rebind_this(JSContext ctx, JSValue this_obj, ref Ray o)
        {
            var buffer = stackalloc float[6];
            var origin = o.origin;
            var direction = o.direction;
            buffer[0] = origin.x;
            buffer[1] = origin.y;
            buffer[2] = origin.z;
            buffer[3] = direction.x;
            buffer[4] = direction.y;
            buffer[5] = direction.z;
            return JSApi.jsb_set_floats(this_obj, 6, buffer) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool js_rebind_this(JSContext ctx, JSValue this_obj, ref Matrix4x4 o)
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

        public static bool js_rebind_this<T>(JSContext ctx, JSValue this_obj, ref T o)
        where T : struct
        {
            //TODO: lookup type rebind-op map at first, fallback to object if fail
            var header = JSApi.jsb_get_payload_header(this_obj);
            switch (header.type_id)
            {
                case BridgeObjectType.ObjectRef:
                    return ScriptEngine.GetObjectCache(ctx).ReplaceObject(header.value, o);
            }
            return false;
        }

        // fallback
        public static bool js_rebind_this(JSContext ctx, JSValue this_obj, ref object o)
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