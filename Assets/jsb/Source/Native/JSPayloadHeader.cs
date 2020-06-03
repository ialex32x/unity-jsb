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

    [StructLayout(LayoutKind.Explicit)]
    public struct JSPayloadHeaderUnion
    {
        [FieldOffset(0)]
        public int32_t object_id;
        
        [FieldOffset(0)]
        public uint32_t size;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct JSPayloadHeader
    {
        public int32_t type_id;
        public JSPayloadHeaderUnion header;
    }
}