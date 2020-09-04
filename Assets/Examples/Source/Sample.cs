using System;
using QuickJS;
using QuickJS.Binding;
using QuickJS.Utils;
using QuickJS.IO;
using QuickJS.Extra;

namespace jsb
{
    using UnityEngine;

    // test
    public class Sample : MonoBehaviour, IScriptRuntimeListener
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
        [ExampleScriptsHint("Assets/Examples/Scripts/out")]
        public string entryFileName = "example_main.js";
        public bool sourceMap;
        public bool stacktrace;
        private ScriptRuntime _rt;
        private MiniConsole _mConsole;

        void Awake()
        {
            IFileSystem fileSystem;

            _mConsole = new MiniConsole(scrollRect, text, 100);
            _rt = ScriptEngine.CreateRuntime();
            var fileResolver = new FileResolver();
            fileResolver.AddSearchPath("node_modules");

            if (fileLoader == FileLoader.Resources)
            {
                fileSystem = new ResourcesFileSystem(_mConsole);
                fileResolver.AddSearchPath("dist");
            }
            else if (fileLoader == FileLoader.HMR)
            {
                Debug.LogWarningFormat("功能未完成");
                fileSystem = new HttpFileSystem(_mConsole, baseUrl);
            }
            else
            {
                fileSystem = new DefaultFileSystem(_mConsole);
                fileResolver.AddSearchPath("Assets/Examples/Scripts/out");
                // _rt.AddSearchPath("Assets/Examples/Scripts/dist");
            }

            _rt.withStacktrace = stacktrace;
            if (sourceMap)
            {
                _rt.EnableSourceMap();
            }
            _mConsole.Write(LogLevel.Info, "Init");
            _rt.Initialize(fileSystem, fileResolver, this, _mConsole, new ByteBufferPooledAllocator());
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

        public void OnBind(ScriptRuntime runtime, TypeRegister register)
        {
            _mConsole.Write(LogLevel.Info, "Bind");
            QuickJS.Extra.WebSocket.Bind(register);
            QuickJS.Extra.XMLHttpRequest.Bind(register);
            if (!runtime.isWorker)
            {
                var uri = new Uri(baseUrl);
                QuickJS.Extra.DOMCompatibleLayer.Bind(register, uri);
                QuickJS.Extra.NodeCompatibleLayer.Bind(register);
            }
            _mConsole.Write(LogLevel.Info, "Bind Finish");
        }

        public void OnComplete(ScriptRuntime runtime)
        {
            if (!runtime.isWorker)
            {
                _mConsole.Write(LogLevel.Info, "run");
                _rt.EvalMain(entryFileName);

                // // 测试, 获取脚本本身返回值 
                // var act = _rt.EvalFile<Action>("do_from_cs");
                // if (act != null)
                // {
                //     act();
                // }
                // else
                // {
                //     Debug.LogWarning("act null");
                // }
                // var v = _rt.EvalFile<ScriptValue>("do_from_cs_v");
                // if (v != null)
                // {
                //     Debug.LogFormat("v.test = {0}", v.GetProperty<string>("test"));
                // }
                // else
                // {
                //     Debug.LogWarning("v null");
                // }
            }
        }
    }
}