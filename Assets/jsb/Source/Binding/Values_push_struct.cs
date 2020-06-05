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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, LayerMask o)
        {
            DuktapeDLL.js_push_int(ctx, (int)o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Color o)
        {
            DuktapeDLL.duk_unity_push_color(ctx, o.r, o.g, o.b, o.a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Color32 o)
        {
            DuktapeDLL.duk_unity_push_color32(ctx, o.r, o.g, o.b, o.a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Vector2 o)
        {
            DuktapeDLL.duk_unity_push_vector2(ctx, o.x, o.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Vector2Int o)
        {
            DuktapeDLL.duk_unity_push_vector2i(ctx, o.x, o.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Vector3 o)
        {
            DuktapeDLL.duk_unity_push_vector3(ctx, o.x, o.y, o.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Vector3Int o)
        {
            DuktapeDLL.duk_unity_push_vector3i(ctx, o.x, o.y, o.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Vector4 o)
        {
            DuktapeDLL.duk_unity_push_vector4(ctx, o.x, o.y, o.z, o.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue(JSContext ctx, Quaternion o)
        {
            DuktapeDLL.duk_unity_push_quaternion(ctx, o.x, o.y, o.z, o.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static JSValue js_push_structvalue(JSContext ctx, Matrix4x4 o)
        // {
        //     DuktapeDLL.js_push_array(ctx);
        //     DuktapeDLL.duk_unity_put16f(ctx, -1, ...);
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_structvalue<T>(JSContext ctx, T o)
                where T : struct
        {
            js_push_classvalue(ctx, o);
        }
    }
}
