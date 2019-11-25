using System;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

namespace jsb
{
    public class JSBridgeDLL
    {
#if UNITY_IPHONE && !UNITY_EDITOR
	    const string DUKTAPEDLL = "__Internal";
#else
        const string DUKTAPEDLL = "jsb";
#endif
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int foo(int a, int b);
    }
}