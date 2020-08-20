using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using Native;
    using UnityEngine;

    public partial class Values
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, LayerMask o)
        {
            var proto = FindPrototypeOf(ctx, typeof(LayerMask));
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(int) * 1);
            JSApi.jsb_set_int_1(val, o.value);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Color o)
        {
            var proto = FindPrototypeOf(ctx, typeof(Color));
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 4);
            JSApi.jsb_set_float_4(val, o.r, o.g, o.b, o.a);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Color32 o)
        {
            var proto = FindPrototypeOf(ctx, typeof(Color32));
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(byte) * 4);
            JSApi.jsb_set_byte_4(val, o.r, o.g, o.b, o.a);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Vector2 o)
        {
            var proto = FindPrototypeOf(ctx, typeof(Vector2));
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 2);
            JSApi.jsb_set_float_2(val, o.x, o.y);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Vector2Int o)
        {
            var proto = FindPrototypeOf(ctx, typeof(Vector2Int));
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(int) * 2);
            JSApi.jsb_set_int_2(val, o.x, o.y);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Vector3 o)
        {
            var proto = FindPrototypeOf(ctx, typeof(Vector3));
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 3);
            JSApi.jsb_set_float_3(val, o.x, o.y, o.z);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Vector3Int o)
        {
            var proto = FindPrototypeOf(ctx, typeof(Vector2Int));
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(int) * 3);
            JSApi.jsb_set_int_3(val, o.x, o.y, o.z);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Vector4 o)
        {
            var proto = FindPrototypeOf(ctx, typeof(Vector4));
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 4);
            JSApi.jsb_set_float_4(val, o.x, o.y, o.z, o.w);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Quaternion o)
        {
            var proto = FindPrototypeOf(ctx, typeof(Quaternion));
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 4);
            JSApi.jsb_set_float_4(val, o.x, o.y, o.z, o.w);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe JSValue js_push_structvalue(JSContext ctx, Matrix4x4 o)
        {
            var proto = FindPrototypeOf(ctx, typeof(Matrix4x4));
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 4 * 4);
            
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
                JSApi.jsb_set_floats(val, 4 * 4, ptr);
            }

            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue<T>(JSContext ctx, T o)
                where T : struct
        {
            return js_push_classvalue(ctx, o);
        }
    }
}
