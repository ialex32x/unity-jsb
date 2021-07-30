#if !JSB_UNITYLESS
using System;
using System.IO;
using System.Collections.Generic;

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
    public class EditorRuntime
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
        private Prefs _prefs;

        static EditorRuntime()
        {
            var prefs = UnityHelper.LoadPrefs();
            if (prefs.editorScripting)
            {
                _instance = new EditorRuntime(prefs);
            }
        }

        public static EditorRuntime GetInstance()
        {
            return _instance;
        }

        public static ScriptRuntime GetRuntime()
        {
            return _instance != null && _instance._ready ? _instance._runtime : null;
        }

        public EditorRuntime(Prefs prefs)
        {
            _prefs = prefs;
            _runMode = RunMode.None;
            ScriptEngine.RuntimeCreated += OnScriptRuntimeCreated;
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
            JSScriptFinder.GetInstance().ModuleSourceChanged -= OnModuleSourceChanged;
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
                var logger = new DefaultScriptLogger();
                var pathResolver = new PathResolver();
                var fileSystem = new DefaultFileSystem(logger);
                var asyncManager = new DefaultAsyncManager();

                _tick = Environment.TickCount;
                _runtime = ScriptEngine.CreateRuntime(true);
                _runtime.AddModuleResolvers();
                _runtime.extraBinding = (runtime, register) =>
                {
                    FSWatcher.Bind(register);
                };
                _runtime.Initialize(new ScriptRuntimeArgs
                {
                    fileSystem = fileSystem,
                    pathResolver = pathResolver,
                    asyncManager = asyncManager,
                    logger = logger,
                    byteBufferAllocator = new ByteBufferPooledAllocator(),
                    binder = DefaultBinder.GetBinder(_prefs.reflectBinding),
                });
                _ready = true;
            }
        }

        private void OnScriptRuntimeCreated(ScriptRuntime runtime)
        {
            runtime.OnInitialized += OnScriptRuntimeInitialized;
        }

        public TSConfig GetTSConfig(string workspace = null)
        {
            var tsconfigPath = string.IsNullOrEmpty(workspace) ? "tsconfig.json" : Path.Combine(workspace, "tsconfig.json");
            if (File.Exists(tsconfigPath))
            {
                var text = Utils.TextUtils.NormalizeJson(File.ReadAllText(tsconfigPath));
                var tsconfig = JsonUtility.FromJson<TSConfig>(text);
                return tsconfig;
            }

            Debug.LogWarning("no tsconfig.json found");
            return null;
        }

        private void OnScriptRuntimeInitialized(ScriptRuntime runtime)
        {
            var tsconfig = GetTSConfig();

            if (tsconfig != null)
            {
                runtime.AddSearchPath(tsconfig.compilerOptions.outDir);
            }

            if (!string.IsNullOrEmpty(_prefs.editorEntryPoint))
            {
                runtime.EvalMain(_prefs.editorEntryPoint);
            }
            else 
            {
                runtime.EvalEmptyMain();
            }

            foreach (var module in _prefs.editorRequires)
            {
                runtime.ResolveModule(module);
            }

            var editorScripts = new List<JSScriptClassPathHint>();
            JSScriptFinder.GetInstance().ModuleSourceChanged += OnModuleSourceChanged;
            JSScriptFinder.GetInstance().Search(JSScriptClassType.CustomEditor, editorScripts);
            foreach (var editorScript in editorScripts)
            {
                runtime.ResolveModule(editorScript.modulePath);
            }
        }

        private void OnModuleSourceChanged(string modulePath, JSScriptClassType classTypes)
        {
            if ((classTypes & JSScriptClassType.CustomEditor) != 0)
            {
                if (_runtime != null && _runtime.isValid && !EditorApplication.isCompiling)
                {
                    _runtime.ResolveModule(modulePath);
                }
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

        public static void Eval(string code)
        {
            if (_instance != null)
            {
                if (_instance._runtime != null)
                {
                    _instance._runtime.GetMainContext().EvalSource(code, "eval");
                    return;
                }
                else
                {
                    if (_instance._runMode == RunMode.Playing)
                    {
                        var runtime = ScriptEngine.GetRuntime(false);
                        if (runtime != null)
                        {
                            runtime.GetMainContext().EvalSource(code, "eval");
                            return;
                        }
                    }
                }
            }

            Debug.LogError("no running EditorRuntime");
        }

        public static void ShowWindow(string module, string typename)
        {
            Eval($"require('UnityEditor').EditorWindow.GetWindow(require('{module}').{typename}).Show()");
        }
    }
}
#endif