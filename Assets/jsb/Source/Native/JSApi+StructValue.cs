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
        public static extern JSValue JSB_NewBridgeObject(JSContext ctx, JSValue proto, int object_id);
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JSB_NewBridgeClassObject(JSContext ctx, JSValue new_target, int object_id);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JSB_NewBridgeValue(JSContext ctx, JSValue proto, uint32_t size);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JSB_NewBridgeClassValue(JSContext ctx, JSValue new_target, int32_t size);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL JSB_SetBridgeType(JSContext ctx, JSValue obj, int32_t type);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int32_t JSB_GetBridgeType(JSContext ctx, JSValue obj);

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
        public static extern JS_BOOL jsb_get_int_2(JSValue val, out int v0, out int v1);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_set_int_2(JSValue val, int v0, int v1);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_get_int_3(JSValue val, out int v0, out int v1, out int v2);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL jsb_set_int_3(JSValue val, int v0, int v1, int v2);
        
    }
}
