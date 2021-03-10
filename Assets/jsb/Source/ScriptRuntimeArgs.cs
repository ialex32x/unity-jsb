using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Reflection;

namespace QuickJS
{
    using Native;
    using Binding;
    using Utils;
    using Module;

    public struct ScriptRuntimeArgs
    {
        public IFileSystem fileSystem;
        public IPathResolver pathResolver;
        public IScriptRuntimeListener listener;
        public IAsyncManager asyncManager;
        public IScriptLogger logger;
        public IO.IByteBufferAllocator byteBufferAllocator;

        // only available in editor
        public bool useReflectBind;
    }
}
