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
        private FileSystemWatcher _prefsWatcher;

        static EditorRuntime()
        {
            var prefs = UnityHelper.LoadPrefs();
            if (prefs.editorScripting)
            {
                _instance = new EditorRuntime(prefs);
            }
        }

        public static Prefs GetPrefs()
        {
            return _instance?._prefs;
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
            if (File.Exists(_prefs.filePath))
            {
                var path = Path.GetDirectoryName(_prefs.filePath);
                _prefsWatcher = new FileSystemWatcher(string.IsNullOrWhiteSpace(path) ? "." : path, Path.GetFileName(_prefs.filePath));
                _prefsWatcher.Changed += OnFileChanged;
                _prefsWatcher.Created += OnFileChanged;
                _prefsWatcher.Deleted += OnFileChanged;
                _prefsWatcher.EnableRaisingEvents = true;
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            _runtime.EnqueueAction(OnFileChangedSync, e.FullPath);
        }

        private void OnFileChangedSync(ScriptRuntime runtime, JSAction action)
        {
            if (action.args != null && action.args.GetType() == typeof(string))
            {
                var fullPath1 = Path.GetFullPath((string)action.args);
                var fullPath2 = Path.GetFullPath(_prefs.filePath);

                if (string.Compare(fullPath1, fullPath2, true) == 0)
                {
                    _prefs = UnityHelper.LoadPrefs();
                }
            }
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
            if (_prefsWatcher != null)
            {
                _prefsWatcher.Dispose();
                _prefsWatcher = null;
            }
            runtime.Shutdown();
        }

        public bool Reload()
        {
            if (EditorApplication.isCompiling)
            {
                return false;
            }

            OnQuitting();
            OnInit();
            return true;
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
                    binder = DefaultBinder.GetBinder(_prefs.preferredBindingMethod),
                });
                _ready = true;
            }
        }

        private void OnScriptRuntimeCreated(ScriptRuntime runtime)
        {
            runtime.OnInitializing += OnScriptRuntimeInitialized;
            runtime.OnMainModuleLoaded += OnScriptRuntimeMainModuleLoaded;
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

        private void OnScriptRuntimeMainModuleLoaded(ScriptRuntime runtime)
        {
            runtime.ResolveModule(_prefs.editorEntryPoint);

            foreach (var module in _prefs.editorRequires)
            {
                runtime.ResolveModule(module);
            }
        }

        private void OnScriptRuntimeInitialized(ScriptRuntime runtime)
        {
            var tsconfig = GetTSConfig();

            if (tsconfig != null)
            {
                runtime.AddSearchPath(tsconfig.compilerOptions.outDir);
            }
            JSScriptFinder.GetInstance().ModuleSourceChanged += OnModuleSourceChanged;

            if (!string.IsNullOrEmpty(_prefs.editorEntryPoint) && !Application.isPlaying)
            {
                runtime.EvalMain(_prefs.editorEntryPoint);
            }
        }

        private void OnModuleSourceChanged(string modulePath, JSScriptClassType classTypes)
        {
            //TODO CustomEditor should support hotload itself (JSInspectorBase.cs)
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
                case PlayModeStateChange.ExitingEditMode: OnQuitting(); break;
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