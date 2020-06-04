using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using UnityEngine;

namespace QuickJS.Native
{
    using int32_t = Int32;
    using uint32_t = UInt32;

    [StructLayout(LayoutKind.Sequential)]
    public struct JSPayloadHeader
    {
        public int32_t type_id;
        public int32_t object_id;
        
        public override int GetHashCode()
        {
            return type_id & (object_id << 2);
        }

        public override bool Equals(object obj)
        {
            if (obj is JSPayloadHeader)
            {
                var t = (JSPayloadHeader) obj;
                return t.type_id == type_id && t.object_id == object_id;
            }

            return false;
        }
    }
}
