using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QuickJS.Extra.Sqlite
{
    using Native;

    public unsafe struct VFSFile
    {

    }

    public unsafe class VFS : IDisposable
    {
        private static VFS _instance;

        private IntPtr _name;
        private sqlite3_vfs* _vfs;

        public static VFS GetInstance()
        {
            if (_instance == null)
            {
                _instance = new VFS();
            }
            return _instance;
        }

        private VFS()
        {
            var name = QuickJS.Utils.TextUtils.GetNullTerminatedBytes("unity");
            _name = Marshal.AllocHGlobal(name.Length);
            Marshal.Copy(name, 0, _name, name.Length);

            _vfs = (sqlite3_vfs*)Marshal.AllocHGlobal(Marshal.SizeOf<sqlite3_vfs>());
            _vfs->iVersion = 3;
            _vfs->szOsFile = Marshal.SizeOf<VFSFile>();
            _vfs->mxPathname = 260 * 4;
            _vfs->pNext = IntPtr.Zero;
            _vfs->zName = _name;
            _vfs->pAppData = IntPtr.Zero;
            // _vfs->xOpen = ;
            // _vfs->xDelete = ;
            // _vfs->xAccess = ;
            // _vfs->xFullPathname = ;
            // _vfs->xDlOpen = ;
            // _vfs->xDlError = ;
            // _vfs->xDlSym = ;
            // _vfs->xDlClose = ;
            // _vfs->xRandomness = ;
            // _vfs->xSleep = ;
            // _vfs->xCurrentTime = ;
            SqliteApi.sqlite3_vfs_register(_vfs, 1);
        }

        ~VFS()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (_vfs != (sqlite3_vfs*)0)
            {
                SqliteApi.sqlite3_vfs_unregister(_vfs);
                Marshal.FreeHGlobal((IntPtr)_vfs);
                Marshal.FreeHGlobal(_name);
                _name = IntPtr.Zero;
                _vfs = (sqlite3_vfs*)0;
            }
        }
    }

    public unsafe class VFSRegister
    {
        // private Dictionary<string, VFS> _all = new Dictionary<string, VFS>();

        [MonoPInvokeCallbackAttribute(typeof(xOpenDelegate))]
        // int xOpenDelegate(sqlite3_vfs* vfs, IntPtr zName, sqlite3_file* file, int flags, ref int pOutFlags);
        public static ResultCode Open(sqlite3_vfs* vfs, IntPtr zName, sqlite3_file* file, int flags, ref int pOutFlags)
        {
            return ResultCode.OK;
        }

        [MonoPInvokeCallbackAttribute(typeof(xDeleteDelegate))]
        // int xDeleteDelegate(sqlite3_vfs* vfs, IntPtr zName, int syncDir);
        public static ResultCode xDelete(sqlite3_vfs* vfs, IntPtr zName, int syncDir)
        {
            return ResultCode.OK;
        }

        [MonoPInvokeCallbackAttribute(typeof(xAccessDelegate))]
        // int xAccessDelegate(sqlite3_vfs* vfs, IntPtr zName, int flags, ref int pResOut);
        public static ResultCode xAccess(sqlite3_vfs* vfs, IntPtr zName, int flags, ref int pResOut)
        {
            return ResultCode.OK;
        }

        [MonoPInvokeCallbackAttribute(typeof(xFullPathnameDelegate))]
        // int xFullPathnameDelegate(sqlite3_vfs* vfs, IntPtr zName, int nOut, IntPtr zOut);
        public static ResultCode xFullPathname(sqlite3_vfs* vfs, IntPtr zName, int nOut, IntPtr zOut)
        {
            return ResultCode.OK;
        }
    }
}
