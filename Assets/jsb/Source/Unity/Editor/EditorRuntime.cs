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
        private int _tick;
        private bool _ready;

        static EditorRuntime()
        {
            var prefs = Prefs.Load();
            if (prefs.editorScripting)
            {
                _instance = new EditorRuntime();
            }
        }

        public static EditorRuntime GetInstance()
        {
            return _instance;
        }

        public static ScriptRuntime GetRuntime()
        {
            return _instance != null && _instance._ready ?_instance._runtime : null;
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

            if (_runtime == null)
            {
                _tick = Environment.TickCount;
                _runtime = ScriptEngine.CreateRuntime(true);
                _runtime.Initialize(this);
            }
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
                if (tick < _tick)
                {
                    _runtime.Update((tick - int.MinValue) + (int.MaxValue - _tick));
                }
                else
                {
                    _runtime.Update(tick - _tick);
                }
                _tick = tick;
            }
        }

        public void OnCreate(ScriptRuntime runtime)
        {
            runtime.AddSearchPath("Scripts/out");
            runtime.AddSearchPath("node_modules");
        }

        public void OnBind(ScriptRuntime runtime, TypeRegister register)
        {
            FSWatcher.Bind(register);
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
                _ready = true;
                var prefs = Prefs.Load();
                if (prefs != null && !string.IsNullOrEmpty(prefs.editorEntryPoint))
                {
                    _runtime.EvalMain(prefs.editorEntryPoint);
                }
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
            if (_instance != null && _instance._runtime != null)
            {
                _instance._runtime.GetMainContext().EvalSource(code, "eval");
            }
            else
            {
                Debug.LogError("no running EditorRuntime");
            }
        }

        public static void ShowWindow(string module, string typename)
        {
            Eval($"require('UnityEditor').EditorWindow.GetWindow(require('{module}').{typename}).Show()");
        }
    }
}
