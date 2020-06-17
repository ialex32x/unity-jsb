using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WebSockets
{
    [StructLayout(LayoutKind.Sequential)]
    public struct lws_context
    {
        public unsafe void* _value;

        public unsafe bool IsValid()
        {
            return _value != (void*)0;
        }
    }
}
