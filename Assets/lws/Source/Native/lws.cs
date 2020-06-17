using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WebSockets
{
    [StructLayout(LayoutKind.Sequential)]
    public struct lws
    {
        public unsafe void* _value;
    }
}
