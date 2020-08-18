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

        /*
        ** CAPI3REF: Result Codes
        ** KEYWORDS: {result code definitions}
        **
        ** Many SQLite functions return an integer result code from the set shown
        ** here in order to indicate success or failure.
        **
        ** New error codes may be added in future versions of SQLite.
        **
        ** See also: [extended result code definitions]
        */
        public enum ResultCode : int
        {
            OK = 0,   /* Successful result */
            /* beginning-of-error-codes */
            ERROR = 1,   /* Generic error */
            INTERNAL = 2,   /* Internal logic error in SQLite */
            PERM = 3,   /* Access permission denied */
            ABORT = 4,   /* Callback routine requested an abort */
            BUSY = 5,   /* The database file is locked */
            LOCKED = 6,   /* A table in the database is locked */
            NOMEM = 7,   /* A malloc() failed */
            READONLY = 8,   /* Attempt to write a readonly database */
            INTERRUPT = 9,   /* Operation terminated by sqlite3_interrupt()*/
            IOERR = 10,   /* Some kind of disk I/O error occurred */
            CORRUPT = 11,   /* The database disk image is malformed */
            NOTFOUND = 12,   /* Unknown opcode in sqlite3_file_control() */
            FULL = 13,   /* Insertion failed because database is full */
            CANTOPEN = 14,   /* Unable to open the database file */
            PROTOCOL = 15,   /* Database lock protocol error */
            EMPTY = 16,   /* Internal use only */
            SCHEMA = 17,   /* The database schema changed */
            TOOBIG = 18,   /* String or BLOB exceeds size limit */
            CONSTRAINT = 19,   /* Abort due to constraint violation */
            MISMATCH = 20,   /* Data type mismatch */
            MISUSE = 21,   /* Library used incorrectly */
            NOLFS = 22,   /* Uses OS features not supported on host */
            AUTH = 23,   /* Authorization denied */
            FORMAT = 24,   /* Not used */
            RANGE = 25,   /* 2nd parameter to sqlite3_bind out of range */
            NOTADB = 26,   /* File opened that is not a database file */
            NOTICE = 27,   /* Notifications from sqlite3_log() */
            WARNING = 28,   /* Warnings from sqlite3_log() */
            ROW = 100,  /* sqlite3_step() has another row ready */
            DONE = 101,  /* sqlite3_step() has finished executing */
            /* end-of-error-codes */

            /*
            ** CAPI3REF: Extended Result Codes
            ** KEYWORDS: {extended result code definitions}
            **
            ** In its default configuration, SQLite API routines return one of 30 integer
            ** [result codes].  However, experience has shown that many of
            ** these result codes are too coarse-grained.  They do not provide as
            ** much information about problems as programmers might like.  In an effort to
            ** address this, newer versions of SQLite (version 3.3.8 [dateof:3.3.8]
            ** and later) include
            ** support for additional result codes that provide more detailed information
            ** about errors. These [extended result codes] are enabled or disabled
            ** on a per database connection basis using the
            ** [sqlite3_extended_result_codes()] API.  Or, the extended code for
            ** the most recent error can be obtained using
            ** [sqlite3_extended_errcode()].
            */
            ERROR_MISSING_COLLSEQ = (ERROR | (1 << 8)),
            ERROR_RETRY = (ERROR | (2 << 8)),
            ERROR_SNAPSHOT = (ERROR | (3 << 8)),
            IOERR_READ = (IOERR | (1 << 8)),
            IOERR_SHORT_READ = (IOERR | (2 << 8)),
            IOERR_WRITE = (IOERR | (3 << 8)),
            IOERR_FSYNC = (IOERR | (4 << 8)),
            IOERR_DIR_FSYNC = (IOERR | (5 << 8)),
            IOERR_TRUNCATE = (IOERR | (6 << 8)),
            IOERR_FSTAT = (IOERR | (7 << 8)),
            IOERR_UNLOCK = (IOERR | (8 << 8)),
            IOERR_RDLOCK = (IOERR | (9 << 8)),
            IOERR_DELETE = (IOERR | (10 << 8)),
            IOERR_BLOCKED = (IOERR | (11 << 8)),
            IOERR_NOMEM = (IOERR | (12 << 8)),
            IOERR_ACCESS = (IOERR | (13 << 8)),
            IOERR_CHECKRESERVEDLOCK = (IOERR | (14 << 8)),
            IOERR_LOCK = (IOERR | (15 << 8)),
            IOERR_CLOSE = (IOERR | (16 << 8)),
            IOERR_DIR_CLOSE = (IOERR | (17 << 8)),
            IOERR_SHMOPEN = (IOERR | (18 << 8)),
            IOERR_SHMSIZE = (IOERR | (19 << 8)),
            IOERR_SHMLOCK = (IOERR | (20 << 8)),
            IOERR_SHMMAP = (IOERR | (21 << 8)),
            IOERR_SEEK = (IOERR | (22 << 8)),
            IOERR_DELETE_NOENT = (IOERR | (23 << 8)),
            IOERR_MMAP = (IOERR | (24 << 8)),
            IOERR_GETTEMPPATH = (IOERR | (25 << 8)),
            IOERR_CONVPATH = (IOERR | (26 << 8)),
            IOERR_VNODE = (IOERR | (27 << 8)),
            IOERR_AUTH = (IOERR | (28 << 8)),
            IOERR_BEGIN_ATOMIC = (IOERR | (29 << 8)),
            IOERR_COMMIT_ATOMIC = (IOERR | (30 << 8)),
            IOERR_ROLLBACK_ATOMIC = (IOERR | (31 << 8)),
            IOERR_DATA = (IOERR | (32 << 8)),
            LOCKED_SHAREDCACHE = (LOCKED | (1 << 8)),
            LOCKED_VTAB = (LOCKED | (2 << 8)),
            BUSY_RECOVERY = (BUSY | (1 << 8)),
            BUSY_SNAPSHOT = (BUSY | (2 << 8)),
            BUSY_TIMEOUT = (BUSY | (3 << 8)),
            CANTOPEN_NOTEMPDIR = (CANTOPEN | (1 << 8)),
            CANTOPEN_ISDIR = (CANTOPEN | (2 << 8)),
            CANTOPEN_FULLPATH = (CANTOPEN | (3 << 8)),
            CANTOPEN_CONVPATH = (CANTOPEN | (4 << 8)),
            CANTOPEN_DIRTYWAL = (CANTOPEN | (5 << 8)) /* Not Used */,
            CANTOPEN_SYMLINK = (CANTOPEN | (6 << 8)),
            CORRUPT_VTAB = (CORRUPT | (1 << 8)),
            CORRUPT_SEQUENCE = (CORRUPT | (2 << 8)),
            CORRUPT_INDEX = (CORRUPT | (3 << 8)),
            READONLY_RECOVERY = (READONLY | (1 << 8)),
            READONLY_CANTLOCK = (READONLY | (2 << 8)),
            READONLY_ROLLBACK = (READONLY | (3 << 8)),
            READONLY_DBMOVED = (READONLY | (4 << 8)),
            READONLY_CANTINIT = (READONLY | (5 << 8)),
            READONLY_DIRECTORY = (READONLY | (6 << 8)),
            ABORT_ROLLBACK = (ABORT | (2 << 8)),
            CONSTRAINT_CHECK = (CONSTRAINT | (1 << 8)),
            CONSTRAINT_COMMITHOOK = (CONSTRAINT | (2 << 8)),
            CONSTRAINT_FOREIGNKEY = (CONSTRAINT | (3 << 8)),
            CONSTRAINT_FUNCTION = (CONSTRAINT | (4 << 8)),
            CONSTRAINT_NOTNULL = (CONSTRAINT | (5 << 8)),
            CONSTRAINT_PRIMARYKEY = (CONSTRAINT | (6 << 8)),
            CONSTRAINT_TRIGGER = (CONSTRAINT | (7 << 8)),
            CONSTRAINT_UNIQUE = (CONSTRAINT | (8 << 8)),
            CONSTRAINT_VTAB = (CONSTRAINT | (9 << 8)),
            CONSTRAINT_ROWID = (CONSTRAINT | (10 << 8)),
            CONSTRAINT_PINNED = (CONSTRAINT | (11 << 8)),
            NOTICE_RECOVER_WAL = (NOTICE | (1 << 8)),
            NOTICE_RECOVER_ROLLBACK = (NOTICE | (2 << 8)),
            WARNING_AUTOINDEX = (WARNING | (1 << 8)),
            AUTH_USER = (AUTH | (1 << 8)),
            OK_LOAD_PERMANENTLY = (OK | (1 << 8)),
            OK_SYMLINK = (OK | (2 << 8)),
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
