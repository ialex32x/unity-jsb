using System;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using Utils;

    public static class ScriptRuntimeExtensions
    {
        public static void Initialize(this ScriptRuntime rt, IFileSystem fileSystem, IScriptRuntimeListener listener)
        {
            rt.Initialize(fileSystem, new Unity.DefaultPathResolver(), listener, new Unity.DefaultLogger(), new IO.ByteBufferPooledAllocator());
        }
    }
}
