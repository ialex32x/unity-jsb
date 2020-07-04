using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using UnityEngine;
    using Native;

    // 处理特殊操作, 关联本地对象等
    public partial class Values
    {
        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, LayerMask o, int type_id)
        {
            var val = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(int) * 1);
            if (!JSApi.JS_IsException(val))
            {
                JSApi.jsb_set_int_1(val, o.value);
            }
            return val;
        }

        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, Color o, int type_id)
        {
            var val = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(float) * 4);
            if (!JSApi.JS_IsException(val))
            {
                JSApi.jsb_set_float_4(val, o.r, o.g, o.b, o.a);
            }
            return val;
        }

        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, Color32 o, int type_id)
        {
            var val = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(int) * 4);
            if (!JSApi.JS_IsException(val))
            {
                JSApi.jsb_set_int_4(val, o.r, o.g, o.b, o.a);
            }
            return val;
        }

        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, Vector2 o, int type_id)
        {
            var val = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(float) * 2);
            if (!JSApi.JS_IsException(val))
            {
                JSApi.jsb_set_float_2(val, o.x, o.y);
            }
            return val;
        }

        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, Vector2Int o, int type_id)
        {
            var val = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(int) * 2);
            if (!JSApi.JS_IsException(val))
            {
                JSApi.jsb_set_int_2(val, o.x, o.y);
            }
            return val;
        }

        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, Vector3 o, int type_id)
        {
            var val = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(float) * 3);
            if (!JSApi.JS_IsException(val))
            {
                JSApi.jsb_set_float_3(val, o.x, o.y, o.z);
            }
            return val;
        }

        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, Vector3Int o, int type_id)
        {
            var val = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(int) * 3);
            if (!JSApi.JS_IsException(val))
            {
                JSApi.jsb_set_int_3(val, o.x, o.y, o.z);
            }
            return val;
        }

        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, Vector4 o, int type_id)
        {
            var val = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(float) * 4);
            if (!JSApi.JS_IsException(val))
            {
                JSApi.jsb_set_float_4(val, o.x, o.y, o.z, o.w);
            }
            return val;
        }

        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, Quaternion o, int type_id)
        {
            var val = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(float) * 4);
            if (!JSApi.JS_IsException(val))
            {
                JSApi.jsb_set_float_4(val, o.x, o.y, o.z, o.w);
            }
            return val;
        }

        public static unsafe JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, Matrix4x4 o, int type_id)
        {
            var val = JSApi.JSB_NewBridgeClassValue(ctx, new_target, sizeof(float) * 4 * 4);
            if (!JSApi.JS_IsException(val))
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
                    JSApi.jsb_set_floats(val, 4 * 4, ptr);
                }

                // JSApi.JSB_SetBridgeType(ctx, val, type_id);
            }
            return val;
        }

        //NOTE: 用于对 c# 对象产生 js 包装对象
        // 分两种情况, 这里是第1种, 在构造中使用
        public static JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, object o, int type_id)
        {
            var cache = ScriptEngine.GetObjectCache(ctx);
            var object_id = cache.AddObject(o);
            var val = JSApi.JSB_NewBridgeClassObject(ctx, new_target, object_id);
            if (JSApi.JS_IsException(val))
            {
                cache.RemoveObject(object_id);
            }
            
            return val;
        }

        //NOTE: 用于对 c# 对象产生 js 包装对象
        // 分两种情况, 这里是第2种, 用于一般情况
        public static JSValue NewBridgeClassObject(JSContext ctx, object o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }
            
            var type = o.GetType();
            var runtime = ScriptEngine.GetRuntime(ctx);
            var db = runtime.GetTypeDB();
            var proto = db.GetPrototypeOf(type);

            if (proto.IsNullish())
            {
                db.GetDynamicType(type);
                proto = db.GetPrototypeOf(type);
                if (proto.IsNullish())
                {
                    return JSApi.JS_ThrowInternalError(ctx, string.Format("no prototype found for {0}", type));
                }
            }

            var cache = runtime.GetObjectCache();
            var object_id = cache.AddObject(o);
            var val = JSApi.jsb_new_bridge_object(ctx, proto, object_id);
            if (val.IsException())
            {
                cache.RemoveObject(object_id);
            }

            return val;
        }
    }
}
