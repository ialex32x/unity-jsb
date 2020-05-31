using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using UnityEngine;

namespace QuickJS.Native
{
    public enum JSClassValueTag : int
    {
        ReferenceID, 
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct JSClassValueUnion
    {
        [FieldOffset(0)] public int int32;
        [FieldOffset(0)] public IntPtr ptr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JSClassValue
    {
        public JSClassValueUnion u;
        public JSClassValueTag tag;

        public override unsafe int GetHashCode()
        {
            return (int) tag & ((int) u.ptr << 2);
        }

        public override unsafe bool Equals(object obj)
        {
            if (obj is JSClassValue)
            {
                var t = (JSClassValue) obj;
                return t.tag == tag && t.u.ptr == u.ptr;
            }

            return false;
        }
    }
}
