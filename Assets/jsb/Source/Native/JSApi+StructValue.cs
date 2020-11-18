using System;
using System.Runtime.InteropServices;
using System.Text;

namespace QuickJS.Native
{
    using int32_t = Int32;
    using uint32_t = UInt32;
    using JS_BOOL = Int32;

    public partial class JSApi
    {
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSClassID JSB_GetBridgeClassID();

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue jsb_construct_bridge_object(JSContext ctx, JSValue proto, int32_t object_id);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue jsb_new_bridge_object(JSContext ctx, JSValue proto, int object_id);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JSB_NewBridgeClassObject(JSContext ctx, JSValue new_target, int object_id);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue jsb_new_bridge_value(JSContext ctx, JSValue proto, uint32_t size);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JSB_NewBridgeClassValue(JSContext ctx, JSValue new_target, int32_t size);

        public static bool JSB_SetBridgeType(JSContext ctx, JSValue obj, JSAtom key, int32_t type)
        {
            if (obj.tag == JS_TAG_OBJECT)
            {
                JS_DefinePropertyValue(ctx, obj, key, JS_NewInt32(ctx, type), JSPropFlags.JS_PROP_HAS_VALUE);
                return true;
            }

            return false;
        }

        public static int32_t JSB_GetBridgeType(JSContext ctx, JSValue obj, JSAtom key)
        {
            if (obj.tag == JS_TAG_OBJECT)
            {
                var val = JS_GetProperty(ctx, obj, key);
                int32_t pres;
                if (JS_ToInt32(ctx, out pres, val) == 0)
                {
                    JS_FreeValue(ctx, val);
                    return pres;
                }
                JS_FreeValue(ctx, val);
            }

            return -1;
        }

        // !!!

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSPayloadHeader JSB_FreePayload(JSContext ctx, JSValue val);
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSPayloadHeader JSB_FreePayloadRT(JSRuntime rt, JSValue val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSPayloadHeader jsb_get_payload_header(JSValue val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSPayload jsb_get_payload(JSValue val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe JS_BOOL jsb_get_floats(JSValue val, int n, float* v0);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe JS_BOOL jsb_set_floats(JSValue val, int n, float* v0);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_get_float_2(JSValue val, out float v0, out float v1);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_set_float_2(JSValue val, float v0, float v1);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_get_float_3(JSValue val, out float v0, out float v1, out float v2);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_set_float_3(JSValue val, float v0, float v1, float v2);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_get_float_4(JSValue val, out float v0, out float v1, out float v2, out float v3);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_set_float_4(JSValue val, float v0, float v1, float v2, float v3);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_get_int_1(JSValue val, out int v0);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_set_int_1(JSValue val, int v0);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_get_int_2(JSValue val, out int v0, out int v1);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_set_int_2(JSValue val, int v0, int v1);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_get_int_3(JSValue val, out int v0, out int v1, out int v2);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_set_int_3(JSValue val, int v0, int v1, int v2);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_get_int_4(JSValue val, out int v0, out int v1, out int v2, out int v3);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_set_int_4(JSValue val, int v0, int v1, int v2, int v3);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_get_byte_4(JSValue val, out byte v0, out byte v1, out byte v2, out byte v3);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_set_byte_4(JSValue val, byte v0, byte v1, byte v2, byte v3);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe JS_BOOL jsb_get_bytes(JSValue val, int n, byte* v0);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe JS_BOOL jsb_set_bytes(JSValue val, int n, byte* v0);
    }
}
