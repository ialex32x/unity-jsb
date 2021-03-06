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
            HMR,
        }
        public UnityEngine.UI.ScrollRect scrollRect;
        public UnityEngine.UI.Text text;
        public FileLoader fileLoader;
        public string baseUrl = "http://127.0.0.1:8183";
        [ExampleScriptsHint("Scripts/out")]
        public string entryFileName = "example_main.js";
        [ExampleToggleHint("启用 ReflectBind")]
        public bool useReflectBind;
        public bool sourceMap;
        public bool stacktrace;
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
                fileSystem = new ResourcesFileSystem(_mConsole);
                pathResolver.AddSearchPath("dist"); // 这里的路径相对于 Unity Resources 空间
            }
            else if (fileLoader == FileLoader.HMR)
            {
                Debug.LogWarningFormat("功能未完成");
                fileSystem = new HttpFileSystem(_mConsole, baseUrl);
            }
            else
            {
                // 演示了一般文件系统的访问, 实际项目中典型的情况需要自行实现基于 AssetBundle(或 7z/zip) 的文件访问层
                fileSystem = new DefaultFileSystem(_mConsole);
                pathResolver.AddSearchPath("Scripts/out");
                // pathResolver.AddSearchPath("../Scripts/out");
                // _rt.AddSearchPath("Assets/Examples/Scripts/dist");
            }

            _rt.withStacktrace = stacktrace;
            if (sourceMap)
            {
                _rt.EnableSourceMap();
            }
            _rt.AddModuleResolvers();
            _rt.extraBinding = (runtime, register) =>
            {
#if !UNITY_WEBGL
                FSWatcher.Bind(register);
                QuickJS.Extra.WebSocket.Bind(register);
                QuickJS.Extra.XMLHttpRequest.Bind(register);
                if (!runtime.isWorker)
                {
                    var uri = new Uri(baseUrl);
                    QuickJS.Extra.DOMCompatibleLayer.Bind(register, uri);
                    QuickJS.Extra.NodeCompatibleLayer.Bind(register);
                }
#endif
            };
            _rt.Initialize(new ScriptRuntimeArgs
            {
                fileSystem = fileSystem,
                pathResolver = pathResolver,
                asyncManager = asyncManager,
                logger = _mConsole,
                byteBufferAllocator = new ByteBufferPooledAllocator(),
                binder = DefaultBinder.GetBinder(useReflectBind),
            });
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