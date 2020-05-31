using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using UnityEngine;

namespace QuickJS.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct JSStructValue
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
            if (obj is JSStructValue)
            {
                return ((JSStructValue) obj)._value == _value;
            }

            return false;
        }
    }
}
