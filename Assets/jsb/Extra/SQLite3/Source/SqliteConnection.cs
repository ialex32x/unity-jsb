#if !UNITY_WEBGL
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AOT;
using System.Text;

namespace QuickJS.Extra
{
    using QuickJS;
    using QuickJS.IO;
    using QuickJS.Native;
    using QuickJS.Binding;
    using QuickJS.Extra.Sqlite;

    public class SqliteConnection : Values, IScriptFinalize
    {
        public void OnJSFinalize()
        {
        }

        public static void Bind(TypeRegister register)
        {

        }
    }
}
#endif
