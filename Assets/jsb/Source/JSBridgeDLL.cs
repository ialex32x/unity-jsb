using System;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

// Default Marshaling for Strings
// https://docs.microsoft.com/en-us/dotnet/framework/interop/default-marshaling-for-strings

namespace jsb
{
    using JSValueConst = JSValue;
    using JSRuntime = IntPtr;
    using JSContext = IntPtr;
    using JS_BOOL = Int32;
    using size_t = UInt64; // 临时
    using JSClassID = UInt32;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
    public delegate void JSClassFinalizer(JSRuntime rt, JSValue val);
#else
    public delegate void JSClassFinalizer(JSRuntime rt, JSValue val);
#endif

    [StructLayout(LayoutKind.Explicit)]
    public struct JSValueUnion
    {
        [FieldOffset(0)]
        int int32;

        [FieldOffset(0)]
        double float64;

        [FieldOffset(0)]
        IntPtr ptr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JSClassDef
    {
        [MarshalAs(UnmanagedType.LPStr)]
        string class_name; // ok?

        JSClassFinalizer finalizer;

        IntPtr gc_mark;
        IntPtr call;
        IntPtr exotic;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JSValue
    {
        JSValueUnion u; // IntPtr
        long tag;
    }

    public partial class JSBridgeDLL
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
        public static extern IntPtr JS_GetContextOpaque(JSContext ctx);
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JS_SetContextOpaque(JSContext ctx, IntPtr opaque);


        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSClassID JS_NewClassID(ref JSClassID pclass_id);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_NewClass(JSRuntime rt, JSClassID class_id, ref JSClassDef class_def);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JS_RunGC(JSRuntime rt);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL JS_IsLiveObject(JSRuntime rt, JSValueConst obj);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_ToInt32(IntPtr ctx, out int pres, JSValue val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_Eval(IntPtr ctx, byte[] input, size_t input_len, byte[] filename, int eval_flags);

        public static JSValue JS_Eval(IntPtr ctx, string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var buf = new byte[bytes.Length + 1];
            bytes.CopyTo(buf, 0);

            var xx = Encoding.UTF8.GetBytes("main");
            var nn = new byte[xx.Length + 1];
            xx.CopyTo(nn, 0);
            return JS_Eval(ctx, buf, (size_t)bytes.Length, nn, 0);
        }

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JSB_Test(IntPtr ctx);

    }
}
