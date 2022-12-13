using System;
using QuickJS;
using QuickJS.Binding;
using QuickJS.Utils;
using QuickJS.IO;
using QuickJS.Extra;
using QuickJS.Unity;

namespace Example
{
    using UnityEngine;

    // test
    public class Sample : MonoBehaviour
    {
        public enum FileLoader
        {
            Default,
            Resources,
            Http,
        }
        public UnityEngine.UI.ScrollRect scrollRect;
        public UnityEngine.UI.Text text;
        public FileLoader fileLoader;
        public string baseUrl = "http://127.0.0.1:8183";
        [ExampleScriptsHint("Scripts/out")]
        public string entryFileName = "example_main.js";
        [ExampleToggleHint("ReflectBind Mode")]
        public bool useReflectBind;
        // public bool sourceMap; // temporarily removed
        public bool stacktrace;

        /// <summary>
        /// only supported with v8-bridge
        /// </summary>
        public bool withDebugServer = true;
        public int debugServerPort = 9229;
        
        /// <summary>
        /// script runtime will complete the initialization process until the debugger is actually connected
        /// @seealso ScriptRuntimeArgs.waitingForDebugger
        /// </summary>
        public bool waitingForDebugger = false;

        private ScriptRuntime _rt;
        private MiniConsole _mConsole;

        void Awake()
        {
            IFileSystem fileSystem;

            _mConsole = new MiniConsole(scrollRect, text, 100);
            _rt = ScriptEngine.CreateRuntime();
            var asyncManager = new DefaultAsyncManager();
            var pathResolver = new PathResolver();
            pathResolver.AddSearchPath("node_modules");

            if (fileLoader == FileLoader.Resources)
            {
                fileSystem = new ResourcesFileSystem();

                // it's the relative path under Unity Resources directory space
                pathResolver.AddSearchPath("dist");
            }
            else if (fileLoader == FileLoader.Http)
            {
                fileSystem = new HttpFileSystem(baseUrl);
            }
            else
            {
                // the DefaultFileSystem only demonstrates the minimalistic implementation of file access, it's usually enough for development in editor.
                // you should implement your own filesystem layer for the device-end runtime (based on AssetBundle or zip)
                fileSystem = new DefaultFileSystem();
                pathResolver.AddSearchPath("Scripts/out");
                // pathResolver.AddSearchPath("../Scripts/out");
                // _rt.AddSearchPath("Assets/Examples/Scripts/dist");
            }

            _rt.withStacktrace = stacktrace;
            // if (sourceMap)
            // {
            // _rt.EnableSourceMap();
            // }
            _rt.AddModuleResolvers();
            _rt.OnInitialized += OnInitialized;
            _rt.Initialize(new ScriptRuntimeArgs
            {
                withDebugServer = withDebugServer,
                waitingForDebugger = waitingForDebugger, 
                debugServerPort = debugServerPort,
                fileSystem = fileSystem,
                pathResolver = pathResolver,
                asyncManager = asyncManager,
                byteBufferAllocator = new ByteBufferPooledAllocator(),
                binder = DefaultBinder.GetBinder(useReflectBind),
                // apiBridge = new Experimental.CustomApiBridgeImpl(),
            });
        }

        private void OnInitialized(ScriptRuntime obj)
        {
            Debug.LogFormat("run main script: {0}", entryFileName);
            _rt.EvalMain(entryFileName);
        }

        void Update()
        {
            if (_rt != null)
            {
                _rt.Update((int)(Time.deltaTime * 1000f));
            }
        }

        void OnDestroy()
        {
            ScriptEngine.Shutdown();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}