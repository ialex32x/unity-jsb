using System;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

namespace jsb
{
    public class JSBridgeDLL
    {
#if UNITY_IPHONE && !UNITY_EDITOR
	    const string JSBDLL = "__Internal";
#else
        const string JSBDLL = "jsb";
#endif

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr XJS_NewRuntime();

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr XJS_NewContext(IntPtr rt);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void XJS_FreeContext(IntPtr rt);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void XJS_FreeRuntime(IntPtr rt);

    }
}
