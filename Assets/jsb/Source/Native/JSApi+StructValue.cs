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
        public static extern void JSB_NewTypePayload(JSContext ctx, JSValue val, JSClassID class_id, int32_t type_id);
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JSB_NewClassPayload(JSContext ctx, JSValue val, JSClassID class_id, int32_t type_id, int32_t object_id);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JSB_NewStructPayload(JSContext ctx, JSValue val, JSClassID class_id, int32_t type_id, int32_t object_id,
            uint32_t size);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSPayloadHeader JSB_FreePayload(JSContext ctx, JSValue val, JSClassID class_id);
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSPayloadHeader JSB_FreePayloadRT(JSRuntime rt, JSValue val, JSClassID class_id);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSPayloadHeader jsb_get_payload(JSValue val, JSClassID class_id);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void jsb_get_floats(JSStructValue sv, int n, float* v0);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void jsb_set_floats(JSStructValue sv, int n, float* v0);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_get_float_2(JSStructValue sv, out float v0, out float v1);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_set_float_2(JSStructValue sv, float v0, float v1);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_get_float_3(JSStructValue sv, out float v0, out float v1, out float v2);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_set_float_3(JSStructValue sv, float v0, float v1, float v2);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_get_float_4(JSStructValue sv, out float v0, out float v1, out float v2, out float v3);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_set_float_4(JSStructValue sv, float v0, float v1, float v2, float v3);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_get_int_2(JSStructValue sv, out int v0, out int v1);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_set_int_2(JSStructValue sv, int v0, int v1);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_get_int_3(JSStructValue sv, out int v0, out int v1, out int v2);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_set_int_3(JSStructValue sv, int v0, int v1, int v2);
        
    }
}
