using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace QuickJS.Extra.Sqlite
{
    using QuickJS;
    using QuickJS.IO;
    using QuickJS.Native;
    using QuickJS.Binding;
    using Native;

    using UnityEngine;

    public class SqliteStarter : MonoBehaviour
    {
        void Awake()
        {
            Debug.LogFormat("sqlite3_libversion_number: {0}", SqliteApi.sqlite3_libversion_number());
            sqlite3 db;
            Debug.LogFormat("open: {0}", SqliteApi.sqlite3_open("test.sqlite", out db));

            sqlite3_stmt stmt;
            SqliteApi.sqlite3_prepare_v3(db, "select * from test;", out stmt);
            while (SqliteApi.sqlite3_step(stmt) == SqliteApi.ResultCode.ROW)
            {
                var id = SqliteApi.sqlite3_column_int(stmt, 0);
                var text = SqliteApi.sqlite3_column_text(stmt, 1);
                Debug.LogFormat("select: {0} = {1}", id, text);
            }
            SqliteApi.sqlite3_finalize(stmt);

            Debug.LogFormat("insert: {0}", SqliteApi.sqlite3_exec(db, "INSERT INTO \"main\".\"test\" (\"name\") VALUES (\"gogogogogo\")"));
            Debug.LogFormat("close: {0}", SqliteApi.sqlite3_close(db));
        }
    }
}
