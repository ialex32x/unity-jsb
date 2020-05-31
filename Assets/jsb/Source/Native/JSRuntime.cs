using System;
using System.Runtime.InteropServices;

namespace QuickJS.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct JSRuntime
    {
        private unsafe void* _ptr;

        public static readonly JSRuntime Null;

        public override unsafe int GetHashCode()
        {
            return (int)_ptr;
        }

        public override unsafe bool Equals(object obj)
        {
            if (obj is JSRuntime)
            {
                var t = (JSRuntime)obj;
                return t._ptr == _ptr;
            }

            return false;
        }
    }
}
