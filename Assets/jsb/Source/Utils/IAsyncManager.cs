using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using QuickJS.Native;

namespace QuickJS.Utils
{
    public interface IAsyncManager
    {
        void Initialize(int mainThreadId);

        // [experimental] 最终可能从 CoroutineManager 中剥离
        void EvalSourceAsync(ScriptContext context, string src);

        JSValue Yield(ScriptContext context, object awaitObject);

        void Destroy();
    }
}
