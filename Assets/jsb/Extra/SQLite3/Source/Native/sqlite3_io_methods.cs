using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace QuickJS.Extra.Sqlite.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct sqlite3_io_methods
    {
        int iVersion;
        IntPtr xClose; // int (*xClose)(sqlite3_file*);
        IntPtr xRead; // int (*xRead)(sqlite3_file*, void*, int iAmt, sqlite3_int64 iOfst);
        IntPtr xWrite; // int (*xWrite)(sqlite3_file*, const void*, int iAmt, sqlite3_int64 iOfst);
        IntPtr xTruncate; // int (*xTruncate)(sqlite3_file*, sqlite3_int64 size);
        IntPtr xSync; // int (*xSync)(sqlite3_file*, int flags);
        IntPtr xFileSize; // int (*xFileSize)(sqlite3_file*, sqlite3_int64 *pSize);
        IntPtr xLock; // int (*xLock)(sqlite3_file*, int);
        IntPtr xUnlock; // int (*xUnlock)(sqlite3_file*, int);
        IntPtr xCheckReservedLock; // int (*xCheckReservedLock)(sqlite3_file*, int *pResOut);
        IntPtr xFileControl; // int (*xFileControl)(sqlite3_file*, int op, void *pArg);
        IntPtr xSectorSize; // int (*xSectorSize)(sqlite3_file*);
        IntPtr xDeviceCharacteristics; // int (*xDeviceCharacteristics)(sqlite3_file*);
        IntPtr xShmMap; // int (*xShmMap)(sqlite3_file*, int iPg, int pgsz, int, void volatile**);
        IntPtr xShmLock; // int (*xShmLock)(sqlite3_file*, int offset, int n, int flags);
        IntPtr xShmBarrier; // void (*xShmBarrier)(sqlite3_file*);
        IntPtr xShmUnmap; // int (*xShmUnmap)(sqlite3_file*, int deleteFlag);
        IntPtr xFetch; // int (*xFetch)(sqlite3_file*, sqlite3_int64 iOfst, int iAmt, void **pp);
        IntPtr xUnfetch; // int (*xUnfetch)(sqlite3_file*, sqlite3_int64 iOfst, void *p);
    }
}
