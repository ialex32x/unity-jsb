using System;
using System.Runtime.InteropServices;

namespace QuickJS.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct JSRuntime
    {
        private unsafe void* _rt;
        
        public override unsafe int GetHashCode()
        {
            return (int) _rt;
        }
    }
}