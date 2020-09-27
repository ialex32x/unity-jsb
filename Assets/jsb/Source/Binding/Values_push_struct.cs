using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using Native;
    using UnityEngine;

    public partial class Values
    {
        public static JSValue js_push_structvalue(JSContext ctx, ref Rect o)
        {
            var proto = FindPrototypeOf<Rect>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 4);
            JSApi.jsb_set_float_4(val, o.x, o.y, o.width, o.height);
            return val;
        }

        public static JSValue js_push_structvalue(JSContext ctx, ref Rect? o)
        {
            if (o == null)
            {
                return JSApi.JS_NULL;
            }
            var proto = FindPrototypeOf<Rect>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 4);
            var v = (Rect)o;
            JSApi.jsb_set_float_4(val, v.x, v.y, v.width, v.height);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, ref LayerMask o)
        {
            var proto = FindPrototypeOf<LayerMask>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(int) * 1);
            JSApi.jsb_set_int_1(val, o.value);
            return val;
        }

        public static JSValue js_push_structvalue(JSContext ctx, ref LayerMask? o)
        {
            if (o == null)
            {
                return JSApi.JS_NULL;
            }
            var proto = FindPrototypeOf<LayerMask>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(int) * 1);
            JSApi.jsb_set_int_1(val, ((LayerMask)o).value);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, ref Color o)
        {
            var proto = FindPrototypeOf<Color>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 4);
            JSApi.jsb_set_float_4(val, o.r, o.g, o.b, o.a);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, ref Color32 o)
        {
            var proto = FindPrototypeOf<Color32>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(byte) * 4);
            JSApi.jsb_set_byte_4(val, o.r, o.g, o.b, o.a);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, ref Vector2 o)
        {
            var proto = FindPrototypeOf<Vector2>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 2);
            JSApi.jsb_set_float_2(val, o.x, o.y);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, ref Vector2Int o)
        {
            var proto = FindPrototypeOf<Vector2Int>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(int) * 2);
            JSApi.jsb_set_int_2(val, o.x, o.y);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, ref Vector3 o)
        {
            var proto = FindPrototypeOf<Vector3>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 3);
            JSApi.jsb_set_float_3(val, o.x, o.y, o.z);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, ref Vector3Int o)
        {
            var proto = FindPrototypeOf<Vector2Int>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(int) * 3);
            JSApi.jsb_set_int_3(val, o.x, o.y, o.z);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, ref Vector4 o)
        {
            var proto = FindPrototypeOf<Vector4>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 4);
            JSApi.jsb_set_float_4(val, o.x, o.y, o.z, o.w);
            return val;
        }

        public static JSValue js_push_structvalue(JSContext ctx, ref Vector4? o)
        {
            if (o == null)
            {
                return JSApi.JS_NULL;
            }
            var proto = FindPrototypeOf<Vector4>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 4);
            var v = (Vector4)o;
            JSApi.jsb_set_float_4(val, v.x, v.y, v.z, v.w);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, ref Quaternion o)
        {
            var proto = FindPrototypeOf<Quaternion>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 4);
            JSApi.jsb_set_float_4(val, o.x, o.y, o.z, o.w);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe JSValue js_push_structvalue(JSContext ctx, ref Ray o)
        {
            var proto = FindPrototypeOf<Ray>(ctx);
            JSValue val = JSApi.jsb_new_bridge_value(ctx, proto, sizeof(float) * 6);
            var buffer = stackalloc float[6];
            var origin = o.origin;
            var direction = o.direction;
            buffer[0] = origin.x;
            buffer[1] = origin.y;
            buffer[2] = origin.z;
            buffer[3] = direction.x;
            buffer[4] = direction.y;
            buffer[5] = direction.z;
            JSApi.jsb_set_floats(val, 6, buffer);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe JSValue js_push_structvalue(JSContext ctx, ref Matrix4x4 o)
        {
            var proto = FindPrototypeOf<Matrix4x4>(ctx);
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
        public static JSValue js_push_structvalue<T>(JSContext ctx, ref T o)
            where T : struct
        {
            return js_push_classvalue(ctx, o);
        }
    }
}
