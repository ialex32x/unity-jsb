using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace QuickJS.Extra.Sqlite
{
    using Native;

    public class SqliteConnection
    {
        private sqlite3 _db;
        // private sqlite3_vfs _vfs;

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