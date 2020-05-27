using System;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

namespace jsb
{
    public struct JSValue
    {
        double u; // IntPtr
        long tag;
    }

    public class JSBridgeDLL
    {
#if UNITY_IPHONE && !UNITY_EDITOR
	    const string JSBDLL = "__Internal";
#else
        const string JSBDLL = "libquickjs";
#endif
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr JS_NewRuntime();

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JS_FreeRuntime(IntPtr rt);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr JS_NewContext(IntPtr rt);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JS_FreeContext(IntPtr rt);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_ToInt32(IntPtr ctx, out int pres, JSValue val);
        
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_Eval(IntPtr ctx, byte[] input, ulong input_len, byte[] filename, int eval_flags);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JSB_Test(IntPtr ctx);

        public static JSValue JS_Eval(IntPtr ctx, string input)
        {
            var bytes  = Encoding.UTF8.GetBytes(input);
            var buf = new byte[bytes.Length + 1];
            bytes.CopyTo(buf, 0);

            var xx = Encoding.UTF8.GetBytes("main");
            var nn = new byte[xx.Length + 1];
            xx.CopyTo(nn, 0);
            return JS_Eval(ctx, buf, (ulong) bytes.Length, nn, 0);
        }
    }
}
