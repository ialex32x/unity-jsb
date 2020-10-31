using System;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    // js-bridge 配置 (editor only)
    public class Prefs
    {
        public const string PATH = "js-bridge.json";

        public string logPath = "Logs/js-bridge.log";

        private bool _dirty;
        private string _filePath;

        // 静态绑定代码的生成目录
        public string outDir = "Assets/Generated/${platform}";
        public string typescriptDir = "Assets/Generated/Typings";
        public string xmlDocDir = "Assets/Generated/Docs";

        /// <summary>
        /// 默认启用编辑器脚本执行相关功能 (JSEditorWindow 等)
        /// </summary>
        public bool editorScripting = false;

        /// <summary>
        /// 代码生成调试开关, 生成的代码将完全在注释中, 方便反复生成对比查问题
        /// </summary>
        public bool debugCodegen = true;

        /// <summary>
        /// 是否将 C# ToString() 自动转换为 toString()
        /// </summary>
        public bool optToString = true;

        /// <summary>
        /// 是否输出到单个 d.ts 声明文件中
        /// </summary>
        public bool singleTSD = true;

        /// <summary>
        /// 即使只有唯一匹配的方法/函数, 也进行类型检查 
        /// </summary>
        public bool alwaysCheckArgType = false;

        /// <summary>
        /// 即使只有唯一匹配的方法/函数, 也进行参数数量检查 
        /// </summary>
        public bool alwaysCheckArgc = true;

        // 生成类型绑定代码类型前缀
        public string typeBindingPrefix = "QuickJS_";

        /// <summary>
        /// 生成的绑定类所在命名空间 (in C#)
        /// </summary>
        public string ns = "jsb";

        /// <summary>
        /// 为没有命名空间的类型, 指定一个模块名
        /// </summary>
        public string defaultJSModule = "global";

        public List<string> skipExtras = new List<string>(new string[]
        {
            "FairyGUI",
            "UnityEditor",
            "Example",
        });

        public List<string> cleanupDir = new List<string>(new string[]
        {
            "Assets/Generated",
        });

        public string procOutDir => ReplacePathVars(outDir);
        public string procTypescriptDir => ReplacePathVars(typescriptDir);

        public string workspace = ".";

        // 尝试生成 Assembly 对应帮助内容
        public bool doc = true;

        // // ts 代码的目录 (例如自动生成的 Delegate 泛型, 需要放在 ts 源码目录)
        // public string tsDir = "Assets/Scripts/Source/duktape";
        // public string jsDir = "Assets/Scripts/Generated/duktape";

        public string extraExt = ""; // 生成文件的额外后缀

        public string newLineStyle = "";

        public string newline
        {
            get
            {
                if (newLineStyle == null)
                {
                    return Environment.NewLine;
                }
                switch (newLineStyle.ToLower())
                {
                    case "cr": return "\r";
                    case "lf": return "\n";
                    case "crlf": return "\r\n";
                    default: return Environment.NewLine;
                }
            }
        }

        public string tab = "    ";

        // 默认不导出任何类型, 需要指定导出类型列表
        public List<string> explicitAssemblies = new List<string>(new string[]
        {
            // "Assembly-CSharp-firstpass",
            "Assembly-CSharp",
        });

        // 默认导出所有类型, 过滤黑名单
        public List<string> implicitAssemblies = new List<string>(new string[]
        {
            // "UnityEngine",
            // "UnityEngine.CoreModule",
            // "UnityEngine.UIModule",
            // "UnityEngine.TextRenderingModule",
            // "UnityEngine.TextRenderingModule",
            // "UnityEngine.UnityWebRequestWWWModule",
            // "UnityEngine.Physics2DModule",
            // "UnityEngine.AnimationModule",
            // "UnityEngine.TextRenderingModule",
            // "UnityEngine.IMGUIModule",
            // "UnityEngine.UnityWebRequestModule",
            // "UnityEngine.PhysicsModule",
            // "UnityEngine.UI",
        });

        // type.FullName 前缀满足以下任意一条时不会被导出
        public List<string> typePrefixBlacklist = new List<string>(new string[]
        {
            "JetBrains.",
            "Unity.Collections.",
            "Unity.Jobs.",
            "Unity.Profiling.",
            "UnityEditor.",
            "UnityEditorInternal.",
            "UnityEngineInternal.",
            "UnityEditor.Experimental.",
            "UnityEngine.Experimental.",
            "Unity.IO.LowLevel.",
            "Unity.Burst.",
            "UnityEngine.Assertions.",
        });

        public Prefs MarkAsDirty()
        {
            if (!_dirty)
            {
                _dirty = true;
                EditorApplication.delayCall += Save;
            }
            return this;
        }

        public static Prefs Load()
        {
            var pathlist = PATH.Split(';');
            foreach (var path in pathlist)
            {
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        var json = Utils.TextUtils.NormalizeJson(System.IO.File.ReadAllText(path));
                        Debug.Log($"load prefs({path}): {json}");
                        var prefs = JsonUtility.FromJson<Prefs>(json);
                        prefs._filePath = path;
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
            defaultPrefs._filePath = pathlist[0];
            return defaultPrefs;
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

        public static string ReplacePathVars(string value)
        {
            var platform = GetPlatform();
            value = value.Replace("${platform}", platform);
            return value;
        }

        public void Save()
        {
            if (_dirty)
            {
                _dirty = false;
                try
                {
                    var json = JsonUtility.ToJson(this, true);
                    System.IO.File.WriteAllText(_filePath, json);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(exception);
                }
            }
        }
    }
}