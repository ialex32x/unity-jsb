using System;
using System.Collections.Generic;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;
    using UnityEngine.Serialization;

    // js-bridge 配置 (editor only)
    public class Prefs
    {
        public const string PATH = "js-bridge.json";

        public string logPath = "Temp/js-bridge.log";

        private bool _dirty;
        private string _filePath;

        // 静态绑定代码的生成目录
        public string outDir = "Assets/Generated/CSharp";
        public string typescriptDir = "Assets/Generated/Typings";
        public bool debugCodegen = false;

        // 生成类型绑定代码类型前缀
        public string typeBindingPrefix = "QuickJS_";

        // 生成的绑定类所在命名空间
        public string ns = "jsb";

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

        private static string GetPlatform()
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            switch (buildTarget)
            {
                case BuildTarget.Android: return "Android";
                case BuildTarget.iOS: return "iOS";
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