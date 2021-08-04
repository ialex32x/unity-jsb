using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace QuickJS.Extra.Sqlite
{
    using Native;

    public unsafe class VFS
    {
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
