using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using UnityEngine;

namespace QuickJS.Native
{
    [StructLayout(LayoutKind.Explicit)]
    public struct JSClassValueUnion
    {
        [FieldOffset(0)] public int int32;
        [FieldOffset(0)] public IntPtr ptr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JSClassValue
    {
        public int type_id;
        public JSClassValueUnion u;

        public override unsafe int GetHashCode()
        {
            return (int) type_id & ((int) u.ptr << 2);
        }

        public override unsafe bool Equals(object obj)
        {
            if (obj is JSClassValue)
            {
                var t = (JSClassValue) obj;
                return t.type_id == type_id && t.u.ptr == u.ptr;
            }

            return false;
        }
    }
}
