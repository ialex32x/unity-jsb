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
        /// <summary>
        /// [v8-bridge] open debug server automatically after JSContext created
        /// </summary>
        public bool withDebugServer;

        /// <summary>
        /// [v8-bridge] wait until the debugger connected and be ready to use
        /// NOTE: this feature is unfinished, the initial breakpoints are still unable to hit
        /// </summary>
        public bool waitingForDebugger;

        /// <summary>
        /// [v8-bridge] the port to listen of the debug server
        /// </summary>
        public int debugServerPort;

        public IFileSystem fileSystem;
        
        public IPathResolver pathResolver;

        public IAsyncManager asyncManager;

        /// <summary>
        /// Customize how the log messages printing. (e.g DefaultScriptLogger directly print logs to the console)
        /// </summary>
        public IScriptLogger logger;

        /// <summary>
        /// The allocator of ByteBuffer. ByteBuffer is only used in unity-jsb C# code, not for quickjs itself.
        /// It's usually used for exchanging data between JS and C#.
        /// </summary>
        public IO.IByteBufferAllocator byteBufferAllocator;

        /// <summary>
        /// specify the binding method (usually via DefaultBinder.GetBinder(...)).
        /// NOTE: reflectbind is only supported in editor.
        /// </summary>
        public BindAction binder;
    }
}
