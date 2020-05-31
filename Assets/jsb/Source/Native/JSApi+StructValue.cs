using System;
using System.Runtime.InteropServices;
using System.Text;

namespace QuickJS.Native
{
    public partial class JSApi
    {
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSStructValue JSB_NewStructValue(JSContext ctx, uint size);

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
