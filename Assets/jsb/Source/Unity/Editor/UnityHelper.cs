using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    [Serializable]
    public class TSConfig
    {
        [Serializable]
        public class CompilerOptions
        {
            public string module;
            public string target;
            public string sourceRoot;
            public string outDir;
            public string outFile;
            public string[] typeRoots;
            public string moduleResolution;
            public string[] types;
            public bool listEmittedFiles;
            public bool experimentalDecorators;
            public bool noImplicitAny;
            public bool allowJs;
            public bool inlineSourceMap;
            public bool sourceMap;
        }
        public CompilerOptions compilerOptions;
        public bool compileOnSave;
        public string[] include;
        public string[] exclude;
    }

    [InitializeOnLoad]
    public static class UnityHelper
    {
        #region All Menu Items
        [MenuItem("JS Bridge/Generate Bindings with d.ts")]
        public static void GenerateBindings()
        {
            var bm = new BindingManager(Prefs.Load());
            bm.Collect();
            bm.Generate(TypeBindingFlags.Default);
            bm.Cleanup();
            bm.Report();
            AssetDatabase.Refresh();
        }

        [MenuItem("JS Bridge/Generate Bindings without d.ts")]
        public static void GenerateBindingsWithoutTSD()
        {
            var bm = new BindingManager(Prefs.Load());
            bm.Collect();
            bm.Generate(TypeBindingFlags.TypeDefinition);
            bm.Cleanup();
            bm.Report();
            AssetDatabase.Refresh();
        }

        // [MenuItem("JS Bridge/Compile TypeScript")]
        public static void CompileScripts()
        {
            Debug.Log("compiling typescript source...");
            EditorApplication.delayCall += () =>
            {
                EditorApplication.delayCall += () =>
                {
#if UNITY_EDITOR_WIN
                    string command = "tsc.cmd";
#else
                    string command = "tsc";
#endif
                    var exitCode = ShellHelper.Run(command, "", 30);
                    Debug.Log($"{command}: {exitCode}");
                };
            };
        }

        [MenuItem("JS Bridge/Clear")]
        public static void ClearBindings()
        {
            var prefs = Prefs.Load();
            var kv = new Dictionary<string, List<string>>();
            foreach (var dir in prefs.cleanupDir)
            {
                var pdir = Prefs.ReplacePathVars(dir);
                kv[pdir] = new List<string>();
            }
            BindingManager.Cleanup(kv, null);
            AssetDatabase.Refresh();
        }

        [MenuItem("JS Bridge/Prefs ...", false, 5001)]
        public static void OpenPrefsEditor()
        {
            EditorWindow.GetWindow<PrefsEditor>().Show();
        }

        [MenuItem("JS Bridge/Console ...", false, 5002)]
        public static void OpenJSConsole()
        {
            EditorWindow.GetWindow<EditorRuntimeConsole>().Show();
        }

        public static bool CheckAnyScriptExists()
        {
            var objects = Selection.objects;
            for (var i = 0; i < objects.Length; ++i)
            {
                var obj = objects[i];
                var assetPath = AssetDatabase.GetAssetPath(obj);
                if (CheckAnyScripts(assetPath, 5))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool CheckAnyScripts(string assetPath, int maxDepth)
        {
            if (maxDepth <= 0)
            {
                // Debug.LogWarningFormat("max depth limited: {0}", assetPath);
                return false;
            }

            if (Directory.Exists(assetPath))
            {
                foreach (var subDir in Directory.GetDirectories(assetPath))
                {
                    if (CheckAnyScripts(subDir, maxDepth - 1))
                    {
                        return true;
                    }
                }

                foreach (var subFile in Directory.GetFiles(assetPath))
                {
                    if (CheckAnyScripts(subFile, maxDepth))
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(assetPath) && assetPath.EndsWith(".js"))
                {
                    return true;
                }
            }
            return false;
        }

        [MenuItem("Assets/JS Bridge/Compile (bytecode)", true)]
        public static bool CompileBytecodeValidate()
        {
            return CheckAnyScriptExists();
        }

        [MenuItem("Assets/JS Bridge/Compile (bytecode)")]
        public static void CompileBytecode()
        {
            CompileBytecode(null);
            AssetDatabase.Refresh();
        }

        // workspace: path of tsconfig.json
        public static void CompileBytecode(string workspace)
        {
            var commonJSModule = false;
            var tsconfigPath = string.IsNullOrEmpty(workspace) ? "tsconfig.json" : Path.Combine(workspace, "tsconfig.json");
            if (File.Exists(tsconfigPath))
            {
                var text = Utils.TextUtils.NormalizeJson(File.ReadAllText(tsconfigPath));
                var tsconfig = JsonUtility.FromJson<TSConfig>(text);
                var module = tsconfig.compilerOptions.module;
                if (module == "commonjs")
                {
                    commonJSModule = true;
                    Debug.LogFormat("read tsconfig.json: compile as commonjs module mode");
                }
            }
            else
            {
                Debug.LogFormat("no tsconfig.json found, compile as ES6 module mode");
            }

            using (var compiler = new ScriptCompiler())
            {
                var objects = Selection.objects;
                for (var i = 0; i < objects.Length; ++i)
                {
                    var obj = objects[i];
                    var assetPath = AssetDatabase.GetAssetPath(obj);
                    CompileBytecode(compiler, assetPath, commonJSModule);
                }
            }
        }

        private static void CompileBytecode(ScriptCompiler compiler, string assetPath, bool commonJSModule)
        {
            if (Directory.Exists(assetPath))
            {
                foreach (var subDir in Directory.GetDirectories(assetPath))
                {
                    CompileBytecode(compiler, subDir, commonJSModule);
                }

                foreach (var subFile in Directory.GetFiles(assetPath))
                {
                    CompileBytecode(compiler, subFile, commonJSModule);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(assetPath) && assetPath.EndsWith(".js"))
                {
                    var outPath = assetPath + ".bytes";
                    var bytes = File.ReadAllBytes(assetPath);
                    var bytecode = compiler.Compile(assetPath, bytes, commonJSModule);
                    if (bytecode != null)
                    {
                        File.WriteAllBytes(outPath, bytecode);
                        Debug.LogFormat("compile {0}({1}) => {2}({3})", assetPath, bytes.Length, outPath, bytecode.Length);
                    }
                    else
                    {
                        Debug.LogErrorFormat("compilation failed: {0}", assetPath);
                        if (File.Exists(outPath))
                        {
                            File.Delete(outPath);
                        }
                    }
                }
            }
        }

        #endregion

        static UnityHelper()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.delayCall += () => ScriptEngine.Shutdown();
            }
        }
    }
}
