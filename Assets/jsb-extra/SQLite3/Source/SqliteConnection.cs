using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QuickJS.Extra.Sqlite
{
    using Native;

    public unsafe class SqliteConnection
    {
        private sqlite3 _db;

        public void Open()
        {
            SqliteApi.sqlite3_open("test.sqlite", out _db);
        }

        public void Close()
        {
            SqliteApi.sqlite3_close(_db);
        }
    }
}