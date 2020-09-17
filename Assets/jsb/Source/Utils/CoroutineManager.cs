using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using QuickJS.Native;
using UnityEngine;

namespace QuickJS.Utils
{
    public interface ICoroutineManager
    {
        // [experimental] 最终可能从 CoroutineManager 中剥离
        void EvalSourceAsync(ScriptContext context, string src);

        JSValue Yield(ScriptContext context, object awaitObject);

        void Destroy();
    }
}
