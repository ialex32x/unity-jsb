using System;

namespace QuickJS.Editor
{
    using QuickJS.Utils;
    using UnityEditor;
    using QuickJS.IO;
    using UnityEngine;
    using QuickJS;
    using QuickJS.Binding;
    using QuickJS.Native;

    [InitializeOnLoad]
    public class EditorRuntime : IScriptRuntimeListener
    {
        private static EditorRuntime _instance;
        private ScriptRuntime _runtime;
        private long _tick;

        static EditorRuntime()
        {
            _instance = new EditorRuntime();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        public static void OnDidReloadScripts()
        {
        }

        public EditorRuntime()
        {
            _tick = Environment.TickCount;

            if (EditorApplication.isPlaying)
            {
                Debug.LogFormat(".isPlaying");
                return;
            }
            var logger = new UnityLogger();
            var fileResolver = new FileResolver();
            var fileSystem = new DefaultFileSystem(logger);
            fileResolver.AddSearchPath("Assets/Examples/Scripts/out/editor");
            fileResolver.AddSearchPath("node_modules");

            _runtime = ScriptEngine.CreateRuntime();
            _runtime.Initialize(fileSystem, fileResolver, this, logger, new ByteBufferPooledAllocator());
            EditorApplication.update += OnUpdate;
        }

        private void OnUpdate()
        {
            var tick = Environment.TickCount;
            _runtime.Update((int)(tick - _tick));
            _tick = tick;
        }

        public void OnBind(ScriptRuntime runtime, TypeRegister register)
        {
            // QuickJS.Extra.WebSocket.Bind(register);
            // QuickJS.Extra.XMLHttpRequest.Bind(register);
            // if (!runtime.isWorker)
            // {
            //     QuickJS.Extra.DOMCompatibleLayer.Bind(register);
            //     QuickJS.Extra.NodeCompatibleLayer.Bind(register);
            // }
        }

        public void OnComplete(ScriptRuntime runtime)
        {
            if (!runtime.isWorker)
            {
                _runtime.EvalMain("main.js");
            }
        }

        private static void onEvalReturn(JSContext ctx, JSValue jsValue)
        {
            var logger = _instance._runtime.GetLogger();
            if (logger != null)
            {
                var ret = JSApi.GetString(ctx, jsValue);
                logger.Write(LogLevel.Info, ret);
            }
        }

        public static void Eval(string code)
        {
            if (_instance._runtime != null)
            {
                _instance._runtime.GetMainContext().EvalSourceFree(code, "eval", onEvalReturn);
            }
        }
    }
}
