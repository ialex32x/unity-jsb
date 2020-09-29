using System;

namespace QuickJS.Unity
{
    using QuickJS.Utils;
    using UnityEditor;
    using QuickJS.IO;
    using UnityEngine;
    using QuickJS;
    using QuickJS.Binding;
    using QuickJS.Native;

    // [InitializeOnLoad]
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
            Debug.LogWarningFormat("init");
            _instance = new EditorRuntime();
        }

        public static EditorRuntime GetInstance()
        {
            return _instance;
        }

        public EditorRuntime()
        {
            _runMode = RunMode.None;
            EditorApplication.delayCall += OnInit;
            EditorApplication.update += OnUpdate;
            EditorApplication.quitting += OnQuitting;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        ~EditorRuntime()
        {
        }

        private void OnQuitting()
        {
            if (_runtime == null)
            {
                return;
            }
            var runtime = _runtime;
            _runtime = null;
            _runMode = RunMode.None;
            EditorApplication.delayCall -= OnInit;
            EditorApplication.update -= OnUpdate;
            EditorApplication.quitting -= OnQuitting;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            runtime.Shutdown();
        }

        private void OnInit()
        {
            if (_runMode == RunMode.Playing)
            {
                return;
            }
            _tick = Environment.TickCount;

            _runtime = ScriptEngine.CreateRuntime(true);
            _runtime.Initialize(this);
        }

        private void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            switch (mode)
            {
                case PlayModeStateChange.EnteredEditMode: _runMode = RunMode.Editor; EditorApplication.delayCall += OnInit; break;
                case PlayModeStateChange.EnteredPlayMode: _runMode = RunMode.Playing; break;
            }
        }

        private void OnUpdate()
        {
            if (_runtime != null)
            {
                if (EditorApplication.isCompiling)
                {
                    OnQuitting();
                    return;
                }
                var tick = Environment.TickCount;
                _runtime.Update((int)(tick - _tick));
                _tick = tick;
            }
        }

        public void OnCreate(ScriptRuntime runtime)
        {
            runtime.AddSearchPath("Scripts/out/editor");
            runtime.AddSearchPath("node_modules");
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
                var ret = _instance._runtime.GetMainContext().EvalSource<string>(code, "eval");
                var logger = _instance._runtime.GetLogger();
                if (logger != null)
                {
                    logger.Write(LogLevel.Info, ret);
                }
            }
        }
    }
}
