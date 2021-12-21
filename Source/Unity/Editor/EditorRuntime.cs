#if !JSB_UNITYLESS
using System;
using System.IO;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;
    using QuickJS.Utils;
    using QuickJS.IO;
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
        private Prefs _prefs;
        private FileSystemWatcher _prefsWatcher;
        private float _changedFileInterval;
        private HashSet<string> _changedFileQueue;

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

        public EditorRuntime(Prefs prefs)
        {
            _prefs = prefs;
            _runMode = RunMode.None;
            _changedFileQueue = new HashSet<string>();
            ScriptEngine.RuntimeCreated += OnScriptRuntimeCreated;
            EditorApplication.delayCall += OnEditorInit;
            EditorApplication.update += OnPrefsSync;
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

        private void OnPrefsSync()
        {
            _changedFileInterval += Time.realtimeSinceStartup;
            if (_changedFileInterval < 3f)
            {
                return;
            }

            _changedFileInterval = 0f;
            lock (_changedFileQueue)
            {
                var len = _changedFileQueue.Count;
                if (len > 0)
                {
                    var changedFiles = new string[len];
                    _changedFileQueue.CopyTo(changedFiles);
                    _changedFileQueue.Clear();
                    for (var i = 0; i < len; ++i)
                    {
                        try
                        {
                            var changedFile = changedFiles[i];
                            var fullPath1 = Path.GetFullPath(changedFile);
                            var fullPath2 = Path.GetFullPath(_prefs.filePath);

                            if (string.Compare(fullPath1, fullPath2, true) == 0)
                            {
                                _prefs = UnityHelper.LoadPrefs();
                            }
                        }
                        catch (Exception exception)
                        {
                            Debug.LogErrorFormat("{0}\n{1}\n", exception.ToString(), exception.StackTrace);
                        }
                    }
                }
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            lock (_changedFileQueue)
            {
                var referredPath = e.FullPath;
                if (!string.IsNullOrEmpty(referredPath))
                {
                    _changedFileQueue.Add(referredPath);
                }
            }
        }

        ~EditorRuntime()
        {
        }

        private void OnEditorQuitting()
        {
            if (_runtime == null)
            {
                return;
            }

            var runtime = _runtime;
            _runtime = null;
            _runMode = RunMode.None;
            JSScriptFinder.GetInstance().ModuleSourceChanged -= OnModuleSourceChanged;
            EditorApplication.delayCall -= OnEditorInit;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.quitting -= OnEditorQuitting;
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

            OnEditorQuitting();
            OnEditorInit();
            return true;
        }

        private void OnEditorInit()
        {
            if (_runMode == RunMode.Playing)
            {
                return;
            }

            if (_runtime == null)
            {
                EditorApplication.update += OnEditorUpdate;
                EditorApplication.quitting += OnEditorQuitting;

                var logger = new DefaultScriptLogger();
                var pathResolver = new PathResolver();
                var fileSystem = new DefaultFileSystem(logger);
                var asyncManager = new DefaultAsyncManager();

                _tick = Environment.TickCount;
                _runtime = ScriptEngine.CreateRuntime(true);
                _runtime.AddModuleResolvers();
                _runtime.Initialize(new ScriptRuntimeArgs
                {
                    fileSystem = fileSystem,
                    pathResolver = pathResolver,
                    asyncManager = asyncManager,
                    logger = logger,
                    byteBufferAllocator = new ByteBufferPooledAllocator(),
                    binder = DefaultBinder.GetBinder(_prefs.preferredBindingMethod),
                });
            }
        }

        private void OnScriptRuntimeCreated(ScriptRuntime runtime)
        {
            runtime.extraBinding += (_1, register) =>
            {
                FSWatcher.Bind(register);
            };
            runtime.OnInitializing += OnScriptRuntimeInitializing;
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
        }

        private void OnScriptRuntimeInitializing(ScriptRuntime runtime)
        {
            var tsconfig = GetTSConfig();

            if (tsconfig != null)
            {
                runtime.AddSearchPath(tsconfig.compilerOptions.outDir);
            }
            JSScriptFinder.GetInstance().ModuleSourceChanged += OnModuleSourceChanged;

            var plover = Resources.Load<TextAsset>("plover.js");
            if (plover != null)
            {
                runtime.GetMainContext().EvalSource(plover.text, "plover.js");
            }
            else
            {
                runtime.GetLogger()?.Write(LogLevel.Error, "failed to load plover.js from Resources");
            }

            if (!string.IsNullOrEmpty(_prefs.editorEntryPoint))
            {
                runtime.EvalMain(_prefs.editorEntryPoint);
            }

            foreach (var module in _prefs.editorRequires)
            {
                runtime.ResolveModule(module);
            }

            // in order to evaluate the decorator (the registration of CustomEditor), we need to load these modules before actually using
            var editorScripts = new List<JSScriptClassPathHint>();
            JSScriptFinder.GetInstance().Search(JSScriptClassType.CustomEditor, editorScripts);
            foreach (var editorScript in editorScripts)
            {
                runtime.ResolveModule(editorScript.modulePath);
            }
        }

        private void OnModuleSourceChanged(string modulePath, JSScriptClassType classTypes)
        {
            // the already loaded CustomEditor scripts could be reloaded automatically by file-watcher
            // but if it's freshly added, resolve it here
            if ((classTypes & JSScriptClassType.CustomEditor) != 0)
            {
                var runtime = ScriptEngine.GetRuntime();
                if (runtime != null && !EditorApplication.isCompiling)
                {
                    runtime.ResolveModule(modulePath);
                }
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            switch (mode)
            {
                case PlayModeStateChange.EnteredEditMode: _runMode = RunMode.Editor; EditorApplication.delayCall += OnEditorInit; break;
                case PlayModeStateChange.ExitingEditMode: OnEditorQuitting(); break;
                case PlayModeStateChange.EnteredPlayMode: _runMode = RunMode.Playing; break;
            }
        }

        private void OnEditorUpdate()
        {
            if (_runtime != null)
            {
                if (EditorApplication.isCompiling)
                {
                    OnEditorQuitting();
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
            var runtime = ScriptEngine.GetRuntime();
            if (runtime != null && !EditorApplication.isCompiling)
            {
                runtime.GetMainContext().EvalSource(code, "eval");
                return;
            }

            Debug.LogError("no running ScriptRuntime");
        }

        public static void ShowWindow(string module, string typename)
        {
            Eval($"require('UnityEditor').EditorWindow.GetWindow(require('{module}').{typename}).Show()");
        }
    }
}
#endif