using System;
using System.Runtime.InteropServices;
using System.Text;

namespace QuickJS.Native
{
    using int32_t = Int32;
    using uint32_t = UInt32;
    
    public partial class JSApi
    {
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSClassID JSB_GetBridgeClassID();

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JSB_NewBridgeObject(JSContext ctx, JSValue proto, int object_id);
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JSB_SetBridgeType(JSContext ctx, JSValue obj, int32_t type);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JSB_NewBridgeValue(JSContext ctx, JSValue proto, uint32_t size);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSPayloadHeader JSB_FreePayload(JSContext ctx, JSValue val);
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSPayloadHeader JSB_FreePayloadRT(JSRuntime rt, JSValue val);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSPayloadHeader jsb_get_payload_header(JSValue val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSPayload jsb_get_payload(JSValue val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void jsb_get_floats(JSPayload sv, int n, float* v0);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void jsb_set_floats(JSPayload sv, int n, float* v0);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_get_float_2(JSPayload sv, out float v0, out float v1);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_set_float_2(JSPayload sv, float v0, float v1);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_get_float_3(JSPayload sv, out float v0, out float v1, out float v2);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_set_float_3(JSPayload sv, float v0, float v1, float v2);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_get_float_4(JSPayload sv, out float v0, out float v1, out float v2, out float v3);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_set_float_4(JSPayload sv, float v0, float v1, float v2, float v3);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_get_int_2(JSPayload sv, out int v0, out int v1);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_set_int_2(JSPayload sv, int v0, int v1);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_get_int_3(JSPayload sv, out int v0, out int v1, out int v2);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_set_int_3(JSPayload sv, int v0, int v1, int v2);
        
    }
}
