using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WebSockets
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct lws_context
    {
        public static readonly lws_context Null = new lws_context();

        public void* _value;

        public bool IsValid()
        {
            return _value != (void*)0;
        }
        
        public override int GetHashCode()
        {
            return (int) _value;
        }

        public bool Equals(lws_context other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is lws_context)
            {
                var other = (lws_context)obj;
                return this == other;
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("lws_context:{0:X}", (ulong) _value);
        }

        public static bool operator ==(lws_context a, lws_context b)
        {
            return a._value == b._value;
        }

        public static bool operator !=(lws_context a, lws_context b)
        {
            return !(a == b);
        }
    }
}
