#if !UNITY_WEBGL || UNITY_WEBGL
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WebSockets
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct lws
    {
        public static readonly lws Null = new lws();
        
        public void* _value;

        public bool IsValid()
        {
            return _value != (void*)0;
        }

        public override int GetHashCode()
        {
            return (int) _value;
        }

        public bool Equals(lws other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is lws)
            {
                var other = (lws)obj;
                return this == other;
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("lws:{0:X}", (ulong) _value);
        }

        public static bool operator ==(lws a, lws b)
        {
            return a._value == b._value;
        }

        public static bool operator !=(lws a, lws b)
        {
            return !(a == b);
        }
    }
}
#endif
