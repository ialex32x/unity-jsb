using System;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace QuickJS.Extra.Sqlite.Native
{
    using sqlite3_int64 = System.Int64;
    using sqlite3_uint64 = System.UInt64;

    public class SqliteApi
    {
#if UNITY_IPHONE && !UNITY_EDITOR
	    const string SQLITE3DLL = "__Internal";
#else
        const string SQLITE3DLL = "sqlite3";
#endif
        /*
        ** CAPI3REF: Flags For File Open Operations
        **
        ** These bit values are intended for use in the
        ** 3rd parameter to the [sqlite3_open_v2()] interface and
        ** in the 4th parameter to the [sqlite3_vfs.xOpen] method.
        */
        [Flags]
        public enum OpenFlags : int
        {
            READONLY = 0x00000001,  /* Ok for sqlite3_open_v2() */
            READWRITE = 0x00000002,  /* Ok for sqlite3_open_v2() */
            CREATE = 0x00000004,  /* Ok for sqlite3_open_v2() */
            DELETEONCLOSE = 0x00000008,  /* VFS only */
            EXCLUSIVE = 0x00000010,  /* VFS only */
            AUTOPROXY = 0x00000020,  /* VFS only */
            URI = 0x00000040,  /* Ok for sqlite3_open_v2() */
            MEMORY = 0x00000080,  /* Ok for sqlite3_open_v2() */
            MAIN_DB = 0x00000100,  /* VFS only */
            TEMP_DB = 0x00000200,  /* VFS only */
            TRANSIENT_DB = 0x00000400,  /* VFS only */
            MAIN_JOURNAL = 0x00000800,  /* VFS only */
            TEMP_JOURNAL = 0x00001000,  /* VFS only */
            SUBJOURNAL = 0x00002000,  /* VFS only */
            MASTER_JOURNAL = 0x00004000,  /* VFS only */
            NOMUTEX = 0x00008000,  /* Ok for sqlite3_open_v2() */
            FULLMUTEX = 0x00010000,  /* Ok for sqlite3_open_v2() */
            SHAREDCACHE = 0x00020000,  /* Ok for sqlite3_open_v2() */
            PRIVATECACHE = 0x00040000,  /* Ok for sqlite3_open_v2() */
            WAL = 0x00080000,  /* VFS only */
            NOFOLLOW = 0x01000000,  /* Ok for sqlite3_open_v2() */
        }

        public enum PrepFlags : uint
        {
            ZERO = 0,

            PERSISTENT = 0x01,
            NORMALIZE = 0x02,
            NO_VTAB = 0x04,
        }

        public enum DataTypes : int
        {
            INTEGER = 1,
            FLOAT = 2,
            BLOB = 4,
            NULL = 5,
            TEXT = 3,
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
        public unsafe delegate void SqliteActionCallback(IntPtr ptr);

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
        public unsafe delegate int SqliteExecCallback(IntPtr ptr, int col_count, IntPtr pp1, IntPtr pp2);

        public static ResultCode sqlite3_open(
            string filename,   /* Database filename (UTF-8) */
            out sqlite3 ppDb      /* OUT: SQLite db handle */
        )
        {
            return sqlite3_open_v2(filename, out ppDb, OpenFlags.READWRITE | OpenFlags.CREATE, null);
        }

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe ResultCode sqlite3_open_v2(
            string filename,   /* Database filename (UTF-8) */
            out sqlite3 ppDb,     /* OUT: SQLite db handle */
            OpenFlags flags,   /* Flags */
            string zVfs        /* Name of VFS module to use */
        );

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ResultCode sqlite3_close(sqlite3 db);

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ResultCode sqlite3_close_v2(sqlite3 db);

        #region error
        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_errcode(sqlite3 db);
        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_extended_errcode(sqlite3 db);
        #endregion 

        #region vfs
        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe sqlite3_vfs* sqlite3_vfs_find(IntPtr zVfsName);
        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int sqlite3_vfs_register(sqlite3_vfs* vfs, int makeDflt);
        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int sqlite3_vfs_unregister(sqlite3_vfs* vfs);
        #endregion 

        #region memory management
        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr sqlite3_malloc(int size);
        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr sqlite3_realloc(IntPtr ptr, int size);
        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern sqlite3_uint64 sqlite3_msize(IntPtr ptr);
        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void sqlite3_free(IntPtr ptr);
        #endregion

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_libversion_number();

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe ResultCode sqlite3_exec(
            sqlite3 db,                                  /* An open database */
            byte* sql,                           /* SQL to be evaluated */
            IntPtr callback,  /* Callback function */
            IntPtr p1,                                    /* 1st argument to callback */
            byte** errmsg                              /* Error msg written here */
        );

        public static unsafe ResultCode sqlite3_exec(sqlite3 db, string zSql)
        {
            var bytes = GetNullTerminatedBytes(zSql);
            fixed (byte* ptr = bytes)
            {
                return sqlite3_exec(db, ptr, IntPtr.Zero, IntPtr.Zero, (byte**)0);
            }
        }

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe ResultCode sqlite3_prepare_v3(
            sqlite3 db,            /* Database handle */
            byte* zSql,       /* SQL statement, UTF-8 encoded */
            int nByte,              /* Maximum length of zSql in bytes. */
            PrepFlags prepFlags, /* Zero or more SQLITE_PREPARE_ flags */
            out sqlite3_stmt ppStmt,  /* OUT: Statement handle */
            out IntPtr pzTail     /* OUT: Pointer to unused portion of zSql */
        );

        public static unsafe ResultCode sqlite3_prepare_v3(sqlite3 db, string zSql, PrepFlags prepFlags, out sqlite3_stmt ppStmt)
        {
            IntPtr pzTail;
            var bytes = Encoding.UTF8.GetBytes(zSql);
            fixed (byte* ptr = bytes)
            {
                return sqlite3_prepare_v3(db, ptr, bytes.Length, prepFlags, out ppStmt, out pzTail);
            }
        }

        public static unsafe ResultCode sqlite3_prepare_v3(sqlite3 db, string zSql, out sqlite3_stmt ppStmt)
        {
            IntPtr pzTail;
            var bytes = Encoding.UTF8.GetBytes(zSql);
            fixed (byte* ptr = bytes)
            {
                return sqlite3_prepare_v3(db, ptr, bytes.Length, PrepFlags.ZERO, out ppStmt, out pzTail);
            }
        }

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ResultCode sqlite3_step(sqlite3_stmt pStmt);

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ResultCode sqlite3_finalize(sqlite3_stmt pStmt);

        #region sqlite3 bind

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ResultCode sqlite3_clear_bindings(sqlite3_stmt pStmt);
        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ResultCode sqlite3_reset(sqlite3_stmt pStmt);

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe ResultCode sqlite3_bind_blob(sqlite3_stmt pStmt, int index, byte* ptr, int n, /*void(*)(void*)*/IntPtr xDel);
        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe ResultCode sqlite3_bind_blob64(sqlite3_stmt pStmt, int index, byte* ptr, sqlite3_uint64 n, /*void(*)(void*)*/IntPtr xDel);

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ResultCode sqlite3_bind_double(sqlite3_stmt pStmt, int index, double value);

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ResultCode sqlite3_bind_int(sqlite3_stmt pStmt, int index, int value);

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ResultCode sqlite3_bind_int64(sqlite3_stmt pStmt, int index, sqlite3_int64 value);

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ResultCode sqlite3_bind_null(sqlite3_stmt pStmt, int index);

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe ResultCode sqlite3_bind_text(sqlite3_stmt pStmt, int index, byte* pText, int nByte, IntPtr xDel);

        public static unsafe ResultCode sqlite3_bind_text(sqlite3_stmt pStmt, int index, string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            fixed (byte* ptr = bytes)
            {
                return sqlite3_bind_text(pStmt, index, ptr, bytes.Length, IntPtr.Zero);
            }
        }

        // ResultCode sqlite3_bind_text16(sqlite3_stmt pStmt, int index, const void*, int, void(*)(void*));
        // ResultCode sqlite3_bind_text64(sqlite3_stmt pStmt, int index, const char*, sqlite3_uint64, void(*)(void*), unsigned char encoding);
        // ResultCode sqlite3_bind_value(sqlite3_stmt pStmt, int index, const sqlite3_value*);
        // ResultCode sqlite3_bind_pointer(sqlite3_stmt pStmt, int index, void*, const char*,void(*)(void*));

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ResultCode sqlite3_bind_zeroblob(sqlite3_stmt pStmt, int index, int n);
        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern ResultCode sqlite3_bind_zeroblob64(sqlite3_stmt pStmt, int index, sqlite3_uint64 n);

        #endregion

        #region sqlite3 column

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr sqlite3_column_blob(sqlite3_stmt pStmt, int iCol);

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern double sqlite3_column_double(sqlite3_stmt pStmt, int iCol);

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_column_int(sqlite3_stmt pStmt, int iCol);

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern sqlite3_int64 sqlite3_column_int64(sqlite3_stmt pStmt, int iCol);

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_text")]
        private static extern unsafe IntPtr _sqlite3_column_text(sqlite3_stmt pStmt, int iCol);

        public static string sqlite3_column_text(sqlite3_stmt pStmt, int iCol)
        {
            var nByte = sqlite3_column_bytes(pStmt, iCol);
            if (nByte < 0)
            {
                return null;
            }
            if (nByte == 0)
            {
                return string.Empty;
            }
            var ptr = _sqlite3_column_text(pStmt, iCol);
            return GetString(ptr, nByte);
        }

        // const void* sqlite3_column_text16(sqlite3_stmt *, int iCol);
        // sqlite3_value* sqlite3_column_value(sqlite3_stmt*, int iCol);
        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_column_bytes(sqlite3_stmt pStmt, int iCol);
        // int sqlite3_column_bytes16(sqlite3_stmt*, int iCol);
        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern DataTypes sqlite3_column_type(sqlite3_stmt pStmt, int iCol);

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_column_count(sqlite3_stmt pStmt);

        #endregion

        #region helpers

        public static byte[] GetNullTerminatedBytes(string str)
        {
            if (str == null)
            {
                return null;
            }

            var len = str.Length;
            if (len > 0 && str[len - 1] == 0)
            {
                return Encoding.UTF8.GetBytes(str);
            }

            var count = Encoding.UTF8.GetByteCount(str);
            var bytes = new byte[count + 1];
            Encoding.UTF8.GetBytes(str, 0, len, bytes, 0);

            return bytes;
        }

        public static unsafe string GetString(IntPtr ptr, int len)
        {
            var str = Marshal.PtrToStringAnsi(ptr, len);
            if (str == null)
            {
                // var pointer = (byte*)(void*)ptr;
                // return Encoding.UTF8.GetString(pointer, len);

                var buffer = new byte[len];
                Marshal.Copy(ptr, buffer, 0, len);
                return Encoding.UTF8.GetString(buffer);
            }

            return str;
        }
        #endregion 
    }
}
