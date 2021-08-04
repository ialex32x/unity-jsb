using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace QuickJS.Extra.Sqlite.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct sqlite3_file
    {
        private sqlite3_io_methods *pMethods;
    }
}
