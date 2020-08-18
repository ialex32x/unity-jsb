using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace QuickJS.Extra.Sqlite.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct sqlite3
    {
        private void* _value;

        public override string ToString()
        {
            return ((ulong)_value).ToString();
        }
    }
}
