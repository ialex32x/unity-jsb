using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using UnityEngine;

namespace QuickJS.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct JSPayload
    {
        private unsafe void* _value;

        public override unsafe int GetHashCode()
        {
            return (int) _value;
        }

        public override unsafe string ToString()
        {
            return ((int) _value).ToString("x");
        }

        public override unsafe bool Equals(object obj)
        {
            if (obj is JSPayload)
            {
                return ((JSPayload) obj)._value == _value;
            }

            return false;
        }
    }
}
