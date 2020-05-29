using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

namespace QuickJS.Native
{
    using JSValueConst = JSValue;
    using JSRuntime = IntPtr;
    using JSContext = IntPtr;
    using JS_BOOL = Int32;
    using JSClassID = UInt32;
    using JSAtom = UInt32;
    using size_t = UIntPtr;
    using uint32_t = UInt32;
    using int64_t = Int64;
    
    [StructLayout(LayoutKind.Sequential)]
    public struct JSClassDef
    {
        public IntPtr class_name; // ok?

        [MarshalAs(UnmanagedType.FunctionPtr)] public JSClassFinalizer finalizer;

        public IntPtr gc_mark;
        public IntPtr call;
        public IntPtr exotic;
    }
}
