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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsException()
        {
            return JSApi.JS_IsException(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNullish()
        {
            return JSApi.JS_IsNull(this) || JSApi.JS_IsUndefined(this);
        }

        public override int GetHashCode()
        {
            return u.int32 << 2 | (int)tag;
        }

        public bool Equals(JSValue other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is JSValue)
            {
                var other = (JSValue)obj;
                return this == other;
            }

            return false;
        }

        public override string ToString()
        {
            if (tag >= 0)
            {
                if (tag == JSApi.JS_TAG_FLOAT64)
                {
                    return u.float64.ToString();
                }

                return u.int32.ToString();
            }

            return string.Format("Ref:{0}", u.ptr);
        }

        public static bool operator ==(JSValue a, JSValue b)
        {
            if (b.tag == a.tag)
            {
                if (a.tag >= 0)
                {
                    return a.tag == JSApi.JS_TAG_FLOAT64 ? a.u.float64 == b.u.float64 : a.u.int32 == b.u.int32;
                }
            }
            return false;
        }

        public static bool operator !=(JSValue a, JSValue b)
        {
            return !(a == b);
        }
    }
}
