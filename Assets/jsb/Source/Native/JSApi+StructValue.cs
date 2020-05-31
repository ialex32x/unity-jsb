using System;
using System.Runtime.InteropServices;
using System.Text;

namespace QuickJS.Native
{
    public partial class JSApi
    {
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSB_StructValue JSB_NewStructValue(JSContext ctx, uint size);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void jsb_get_floats(JSB_StructValue sv, int n, float* v0);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void jsb_set_floats(JSB_StructValue sv, int n, float* v0);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_get_float_2(JSB_StructValue sv, out float v0, out float v1);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_set_float_2(JSB_StructValue sv, float v0, float v1);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_get_float_3(JSB_StructValue sv, out float v0, out float v1, out float v2);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_set_float_3(JSB_StructValue sv, float v0, float v1, float v2);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_get_float_4(JSB_StructValue sv, out float v0, out float v1, out float v2, out float v3);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_set_float_4(JSB_StructValue sv, float v0, float v1, float v2, float v3);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_get_int_2(JSB_StructValue sv, out int v0, out int v1);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_set_int_2(JSB_StructValue sv, int v0, int v1);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_get_int_3(JSB_StructValue sv, out int v0, out int v1, out int v2);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jsb_set_int_3(JSB_StructValue sv, int v0, int v1, int v2);
        
    }
}
