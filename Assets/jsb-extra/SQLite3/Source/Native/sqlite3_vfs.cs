using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace QuickJS.Extra.Sqlite.Native
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || JSB_UNITYLESS || (UNITY_WSA && !UNITY_EDITOR)
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public unsafe delegate int xOpenDelegate(sqlite3_vfs* vfs, IntPtr zName, sqlite3_file* file, int flags, ref int pOutFlags);
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || JSB_UNITYLESS || (UNITY_WSA && !UNITY_EDITOR)
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public unsafe delegate int xDeleteDelegate(sqlite3_vfs* vfs, IntPtr zName, int syncDir);
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || JSB_UNITYLESS || (UNITY_WSA && !UNITY_EDITOR)
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public unsafe delegate int xAccessDelegate(sqlite3_vfs* vfs, IntPtr zName, int flags, ref int pResOut);
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || JSB_UNITYLESS || (UNITY_WSA && !UNITY_EDITOR)
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public unsafe delegate int xFullPathnameDelegate(sqlite3_vfs* vfs, IntPtr zName, int nOut, IntPtr zOut);

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct sqlite3_vfs
    {
        public int iVersion; /* Structure version number (currently 3) */
        public int szOsFile; /* Size of subclassed sqlite3_file */
        public int mxPathname; /* Maximum file pathname length */
        public IntPtr pNext; /* Next registered VFS */
        public IntPtr zName; /* Name of this virtual file system */
        public IntPtr pAppData; /* Pointer to application-specific data */
        public IntPtr xOpen; // int (*xOpen)(sqlite3_vfs*, const char *zName, sqlite3_file*, int flags, int *pOutFlags);
        public IntPtr xDelete; // int (*xDelete)(sqlite3_vfs*, const char *zName, int syncDir);
        public IntPtr xAccess; // int (*xAccess)(sqlite3_vfs*, const char *zName, int flags, int *pResOut);
        public IntPtr xFullPathname; // int (*xFullPathname)(sqlite3_vfs*, const char *zName, int nOut, char *zOut);

        public IntPtr xDlOpen; // void *(*xDlOpen)(sqlite3_vfs*, const char *zFilename);
        public IntPtr xDlError; // void (*xDlError)(sqlite3_vfs*, int nByte, char *zErrMsg);
        public IntPtr xDlSym; // void (*(*xDlSym)(sqlite3_vfs*,void*, const char *zSymbol))(void);
        public IntPtr xDlClose; // void (*xDlClose)(sqlite3_vfs*, void*);

        public IntPtr xRandomness; // int (*xRandomness)(sqlite3_vfs*, int nByte, char *zOut);
        public IntPtr xSleep; // int (*xSleep)(sqlite3_vfs*, int microseconds);
        public IntPtr xCurrentTime; // int (*xCurrentTime)(sqlite3_vfs*, double*);
        public IntPtr xGetLastError; // int (*xGetLastError)(sqlite3_vfs*, int, char *);
        public IntPtr xCurrentTimeInt64; // int (*xCurrentTimeInt64)(sqlite3_vfs*, sqlite3_int64*);
        public IntPtr xSetSystemCall; // int (*xSetSystemCall)(sqlite3_vfs*, const char *zName, sqlite3_syscall_ptr);
        public IntPtr xGetSystemCall; // sqlite3_syscall_ptr (*xGetSystemCall)(sqlite3_vfs*, const char *zName);
        public IntPtr xNextSystemCall; // const char *(*xNextSystemCall)(sqlite3_vfs*, const char *zName);
    }
}
