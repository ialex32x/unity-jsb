#if !UNITY_WEBGL
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace QuickJS.Extra.SQLite3
{
    using size_t = QuickJS.Native.size_t;
    using sqlite3 = IntPtr;

    public class SQLite3Api
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
        public enum OpenFlags
        {
            READONLY =         0x00000001,  /* Ok for sqlite3_open_v2() */
            READWRITE =        0x00000002,  /* Ok for sqlite3_open_v2() */
            CREATE =           0x00000004,  /* Ok for sqlite3_open_v2() */
            DELETEONCLOSE =    0x00000008,  /* VFS only */
            EXCLUSIVE =        0x00000010,  /* VFS only */
            AUTOPROXY =        0x00000020,  /* VFS only */
            URI =              0x00000040,  /* Ok for sqlite3_open_v2() */
            MEMORY =           0x00000080,  /* Ok for sqlite3_open_v2() */
            MAIN_DB =          0x00000100,  /* VFS only */
            TEMP_DB =          0x00000200,  /* VFS only */
            TRANSIENT_DB =     0x00000400,  /* VFS only */
            MAIN_JOURNAL =     0x00000800,  /* VFS only */
            TEMP_JOURNAL =     0x00001000,  /* VFS only */
            SUBJOURNAL =       0x00002000,  /* VFS only */
            MASTER_JOURNAL =   0x00004000,  /* VFS only */
            NOMUTEX =          0x00008000,  /* Ok for sqlite3_open_v2() */
            FULLMUTEX =        0x00010000,  /* Ok for sqlite3_open_v2() */
            SHAREDCACHE =      0x00020000,  /* Ok for sqlite3_open_v2() */
            PRIVATECACHE =     0x00040000,  /* Ok for sqlite3_open_v2() */
            WAL =              0x00080000,  /* VFS only */
            NOFOLLOW =         0x01000000,  /* Ok for sqlite3_open_v2() */
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
        public enum ResultCode
        {
            OK =           0,   /* Successful result */
            /* beginning-of-error-codes */
            ERROR =        1,   /* Generic error */
            INTERNAL =     2,   /* Internal logic error in SQLite */
            PERM =         3,   /* Access permission denied */
            ABORT =        4,   /* Callback routine requested an abort */
            BUSY =         5,   /* The database file is locked */
            LOCKED =       6,   /* A table in the database is locked */
            NOMEM =        7,   /* A malloc() failed */
            READONLY =     8,   /* Attempt to write a readonly database */
            INTERRUPT =    9,   /* Operation terminated by sqlite3_interrupt()*/
            IOERR =       10,   /* Some kind of disk I/O error occurred */
            CORRUPT =     11,   /* The database disk image is malformed */
            NOTFOUND =    12,   /* Unknown opcode in sqlite3_file_control() */
            FULL =        13,   /* Insertion failed because database is full */
            CANTOPEN =    14,   /* Unable to open the database file */
            PROTOCOL =    15,   /* Database lock protocol error */
            EMPTY =       16,   /* Internal use only */
            SCHEMA =      17,   /* The database schema changed */
            TOOBIG =      18,   /* String or BLOB exceeds size limit */
            CONSTRAINT =  19,   /* Abort due to constraint violation */
            MISMATCH =    20,   /* Data type mismatch */
            MISUSE =      21,   /* Library used incorrectly */
            NOLFS =       22,   /* Uses OS features not supported on host */
            AUTH =        23,   /* Authorization denied */
            FORMAT =      24,   /* Not used */
            RANGE =       25,   /* 2nd parameter to sqlite3_bind out of range */
            NOTADB =      26,   /* File opened that is not a database file */
            NOTICE =      27,   /* Notifications from sqlite3_log() */
            WARNING =     28,   /* Warnings from sqlite3_log() */
            ROW =         100,  /* sqlite3_step() has another row ready */
            DONE =        101,  /* sqlite3_step() has finished executing */
        }
        /* end-of-error-codes */

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int sqlite3_open(
            string filename,   /* Database filename (UTF-8) */
            sqlite3 *ppDb      /* OUT: SQLite db handle */
        );

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int sqlite3_open_v2(
            string filename,   /* Database filename (UTF-8) */
            sqlite3 *ppDb,     /* OUT: SQLite db handle */
            OpenFlags flags,   /* Flags */
            string zVfs        /* Name of VFS module to use */
        );
        
        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_close(sqlite3 db);

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_close_v2(sqlite3 db);

        [DllImport(SQLITE3DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int sqlite3_libversion_number();
    }
}
#endif
