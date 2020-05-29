using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

namespace QuickJS.Native
{
    [StructLayout(LayoutKind.Explicit)]
    public struct JSValueUnion
    {
        [FieldOffset(0)] public int int32;

        [FieldOffset(0)] public double float64;

        [FieldOffset(0)] public IntPtr ptr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JSValue
    {
        public JSValueUnion u; // IntPtr
        public long tag;
    }
}
