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
        private enum RunMode
        {
            Editor,
            Playing,
            None,
        }
#pragma warning disable 0649
        private static EditorRuntime _instance;
#pragma warning restore 0649
        private ScriptRuntime _runtime;
        private RunMode _runMode;
        private long _tick;

        static EditorRuntime()
        {
            //TODO: 暂时屏蔽
            // _instance = new EditorRuntime();
        }

        public EditorRuntime()
        {
            _runMode = RunMode.None;
            EditorApplication.delayCall += OnInit;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        ~EditorRuntime()
        {
        }

        private void OnInit()
        {
            if (_runMode == RunMode.Playing)
            {
                return;
            }
            _tick = Environment.TickCount;

            var logger = new UnityLogger();
            var fileResolver = new FileResolver();
            var fileSystem = new DefaultFileSystem(logger);
            fileResolver.AddSearchPath("Assets/Examples/Scripts/out/editor");
            fileResolver.AddSearchPath("node_modules");

            _runtime = ScriptEngine.CreateRuntime(true);
            _runtime.Initialize(fileSystem, fileResolver, this, logger, new ByteBufferPooledAllocator());
            _runtime.OnDestroy += OnDestroy;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            switch (mode)
            {
                case PlayModeStateChange.EnteredEditMode: _runMode = RunMode.Editor; EditorApplication.delayCall += OnInit; break;
                case PlayModeStateChange.EnteredPlayMode: _runMode = RunMode.Playing; break;
            }
        }

        private void OnDestroy(ScriptRuntime rt)
        {
        }

        private void OnUpdate()
        {
            if (_runtime != null)
            {
                var tick = Environment.TickCount;
                _runtime.Update((int)(tick - _tick));
                _tick = tick;
            }
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
