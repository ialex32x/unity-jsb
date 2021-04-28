#if !JSB_UNITYLESS
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;
    using System.Reflection;
    using QuickJS.Binding;

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
        [MenuItem("JS Bridge/Generate Bindings And Type Definition")]
        public static void GenerateBindingsAndTypeDefinition()
        {
            var bm = new BindingManager(LoadPrefs());
            bm.Collect();
            bm.Generate(TypeBindingFlags.Default);
            bm.Cleanup();
            bm.Report();
            AssetDatabase.Refresh();
        }

        [MenuItem("JS Bridge/Generate Type Definition")]
        public static void GenerateTypeDefinition()
        {
            var bm = new BindingManager(LoadPrefs());
            bm.Collect();
            bm.Generate(TypeBindingFlags.TypeDefinition);
            bm.Cleanup();
            bm.Report();
            AssetDatabase.Refresh();
        }

        public static Prefs LoadPrefs()
        {
            string filePath;
            return LoadPrefs(out filePath);
        }

        public static Prefs LoadPrefs(out string filePath)
        {
            var pathlist = Prefs.PATH.Split(';');
            foreach (var path in pathlist)
            {
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        var json = Utils.TextUtils.NormalizeJson(System.IO.File.ReadAllText(path));
                        Debug.Log($"load prefs({path}): {json}");
                        var prefs = JsonUtility.FromJson<Prefs>(json);
                        filePath = path;
                        if (string.IsNullOrEmpty(prefs.typescriptDir))
                        {
                            prefs.typescriptDir = prefs.outDir;
                        }
                        return prefs;
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning(exception);
                    }
                }
            }
            var defaultPrefs = new Prefs();
            filePath = pathlist[0];
            return defaultPrefs;
        }

        public static bool IsReflectBindingSupported()
        {
            return LoadPrefs().reflectBinding;
        }

        public static void InvokeReflectBinding(ScriptRuntime runtime)
        {
            var bm = new BindingManager(LoadPrefs(), new ReflectBindingCallback(runtime));
            bm.Collect();
            bm.Generate(TypeBindingFlags.None);
            bm.Report();
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
                    var exitCode = UnityShellHelper.Run(command, "", 30);
                    Debug.Log($"{command}: {exitCode}");
                };
            };
        }

        [MenuItem("JS Bridge/Clear")]
        public static void ClearBindings()
        {
            var prefs = LoadPrefs();
            var kv = new Dictionary<string, List<string>>();
            foreach (var dir in prefs.cleanupDir)
            {
                var pdir = ReplacePathVars(dir);
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

        [MenuItem("JS Bridge/Javascript Console", false, 5003)]
        public static void ShowJSConsole()
        {
            QuickJS.Unity.EditorRuntime.ShowWindow("editor/js_console", "JSConsole");
        }

        [MenuItem("JS Bridge/Javascript Module View", false, 5003)]
        public static void ShowJSModuleView()
        {
            QuickJS.Unity.EditorRuntime.ShowWindow("editor/js_module_view", "JSModuleView");
        }

        public static string GetPlatform()
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            switch (buildTarget)
            {
                case BuildTarget.Android: return "Android";
                case BuildTarget.iOS: return "iOS";
                case BuildTarget.WSAPlayer: return "WSA"; // not supported
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64: return "Windows";
                case BuildTarget.StandaloneOSX: return "OSX";
                case BuildTarget.StandaloneLinux:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneLinuxUniversal: return "Linux";
                case BuildTarget.Switch: return "Switch";
                case BuildTarget.PS4: return "PS4";
                default: return buildTarget.ToString();
            }
        }

        /// <summary>
        /// 可以明确表明该 Type 属于 Editor 运行时
        /// </summary>
        public static bool IsExplicitEditorType(Type type)
        {
            if (type.Namespace != null && type.Namespace.StartsWith("UnityEditor"))
            {
                return true;
            }

            return IsExplicitEditorDomain(type.Assembly);
        }

        /// <summary>
        /// 可以明确表明该 Assembly 包含的类型属于 Editor 运行时
        /// </summary>
        public static bool IsExplicitEditorDomain(Assembly assembly)
        {
            var location = assembly.Location.Replace('\\', '/');
            var assetsPath = new FileInfo(Application.dataPath);
            var fullAssetsPath = assetsPath.FullName.Replace('\\', '/');

            if (location.StartsWith(fullAssetsPath, StringComparison.OrdinalIgnoreCase))
            {
                var substr = location.Substring(fullAssetsPath.Length).ToLower();
                if (substr.Contains("/editor/"))
                {
                    return true;
                }
            }

            return false;
        }

        public static string ReplacePathVars(string value)
        {
            var platform = GetPlatform();
            value = value.Replace("${platform}", platform);
            return value;
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

            using (var compiler = new UnityJSScriptCompiler())
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

        private static void CompileBytecode(UnityJSScriptCompiler compiler, string assetPath, bool commonJSModule)
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

        // https://regex101.com/r/426q4x/1
        public static Regex JSBehaviourClassNameRegex = new Regex(@"@ScriptType[\n\s]*export\s+class\s+(\w+)\s+extends", RegexOptions.Multiline | RegexOptions.Compiled);
        public static Regex JSCustomEditorClassNameRegex = new Regex(@"^\s*@CustomEditor\s*\(.*\)[\n\s]*export\s+class\s+(\w+)\s+extends", RegexOptions.Multiline | RegexOptions.Compiled);

        public static string NormalizePathString(string path)
        {
            return path.Replace('\\', '/');
        }

        // sourceFile: 需要传入 FullPath
        public static bool ResolveScriptRef(string sourceDirBase, string sourceFile, List<JSScriptClassPathHint> hints)
        {
            if (!Path.IsPathRooted(sourceFile))
            {
                sourceFile = Path.GetFullPath(sourceFile);
            }

            if (!sourceFile.EndsWith(".ts"))
            {
                // invalid 
                return false;
            }

            var sourceExt = Path.GetExtension(sourceFile);

            if (File.Exists(sourceFile))
            {
                var sourceDir = string.IsNullOrEmpty(sourceDirBase) ? "." : sourceDirBase;
                if (Path.IsPathRooted(sourceDir))
                {
                    // not implemented
                }
                else
                {
                    var appRoot = Path.GetPathRoot(Application.dataPath);
                    var sourceRoot = Path.GetPathRoot(sourceFile).Replace('\\', '/');

                    if (appRoot == sourceRoot)
                    {
                        var sourcePathNorm = sourceFile.Replace('\\', '/');
                        var appPathNorm = Path.Combine(Directory.GetParent(Application.dataPath).FullName, sourceDir).Replace('\\', '/');

                        if (sourcePathNorm.ToLower().StartsWith(appPathNorm.ToLower()))
                        {
                            var sourceSubPathNorm = sourcePathNorm[0] == '/' ? sourcePathNorm.Substring(appPathNorm.Length + 1) : sourcePathNorm.Substring(appPathNorm.Length);
                            var offset = sourceSubPathNorm[0] == '/' ? 1 : 0;
                            var modulePath = sourceSubPathNorm.Substring(offset, sourceSubPathNorm.Length - sourceExt.Length - offset);
                            GetJSScriptClasses(sourceFile, modulePath, hints);
                            return true;
                        }
                    }
                    else
                    {
                        // invalid
                    }
                }
            }

            return false;
        }

        private static void GetJSScriptClasses(string sourceFile, string modulePath, List<JSScriptClassPathHint> hints)
        {
            //TODO: need optimization?
            var text = File.ReadAllText(sourceFile);
            
            foreach (Match m in JSBehaviourClassNameRegex.Matches(text))
            {
                hints.Add(new JSScriptClassPathHint(sourceFile, modulePath, m.Groups[1].Value, JSScriptClassType.MonoBehaviour));
            }

            foreach (Match m in JSCustomEditorClassNameRegex.Matches(text))
            {
                hints.Add(new JSScriptClassPathHint(sourceFile, modulePath, m.Groups[1].Value, JSScriptClassType.Editor));
            }
        }

        static UnityHelper()
        {
            // EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        // private static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        // {
        //     if (playModeStateChange == PlayModeStateChange.EnteredEditMode)
        //     {
        //         EditorApplication.delayCall += () => ScriptEngine.Shutdown();
        //     }
        // }
    }
}
#endif
