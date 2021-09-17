using System;
using System.Collections.Generic;

namespace QuickJS.Binding
{
    /// <summary>
    /// [EDITOR_ONLY] configuration for BindingManager
    /// you can use a json file at './js-bridge.json' without modifying the source file 'Prefs.cs'
    /// </summary>
    public class Prefs
    {
        /// <summary>
        /// location of this configuration (optional)
        /// </summary>
        public const string PATH = "js-bridge.json";

        public string filePath;

        #region Configurable Fields

        /// <summary>
        /// the log file generated by the binding process
        /// </summary>
        public string logPath = "Logs/js-bridge.log";

        /// <summary>
        /// JSBehaviourScriptRef.sourceFile 将基于 sourceDir 拆出对应的 modulePath
        /// </summary>
        public string sourceDir = "Scripts/src";

        /// <summary>
        /// the output directory for generating static binding code
        /// </summary>
        public string outDir = "Assets/Generated/${platform}";

        /// <summary>
        /// 绑定代码对应 ts 声明文件生成目录
        /// </summary>
        public string typescriptDir = "Assets/Generated/Typings";

        /// <summary>
        /// 编辑器搜索脚本源码时匹配的后缀名
        /// </summary>
        public string typescriptExt = ".ts";

        /// <summary>
        /// Assembly-CSharp.dll 对应的 XmlDoc 生成目录
        /// </summary>
        public string xmlDocDir = "Assets/Generated/Docs";

        /// <summary>
        /// 生成绑定代码时输出一份 JS 模块列表信息
        /// </summary>
        public string jsModulePackInfoPath = "jsb-modules.json";

        /// <summary>
        /// 是否生成 typescript 声明文件中的文档注释
        /// </summary>
        public bool genTypescriptDoc = true;

        /// <summary>
        /// 默认启用编辑器脚本执行相关功能 (JSEditorWindow 等)
        /// </summary>
        public bool editorScripting = true;

        /// <summary>
        /// [EDITOR_ONLY] preferred to bind types by reflection in editor without generating any binding code (useful for development stage in editor)
        /// </summary>
        public bool reflectBinding = true;

        /// <summary>
        /// 启用运算符重载转换 (禁用后运算符将以 op_* 的形式导出为函数)
        /// </summary>
        public bool enableOperatorOverloading = true;

        /// <summary>
        /// 编辑器运行时脚本入口
        /// </summary>
        public string editorEntryPoint = "editor/main";

        /// <summary>
        /// Asset Postprocessor(s) implemented in scripts
        /// </summary>
        public List<string> assetPostProcessors = new List<string>(new string[]
        {
            "editor/asset_postprocessor",
        });

        public List<string> editorRequires = new List<string>(new string[]
        {
            "plover/editor/js_reload",
        });

        /// <summary>
        /// generate totally commented staticbind code for more conveniently debugging the codegen process itself
        /// </summary>
        public bool debugCodegen = false;

        /// <summary>
        /// omit all delegates with ByRef parameter
        /// </summary>
        public bool skipDelegateWithByRefParams = false;

        /// <summary>
        /// output more details to the log file
        /// </summary>
        public bool verboseLog = true;

        /// <summary>
        /// automatically rename ToString() into toString()
        /// </summary>
        public bool optToString = true;

        /// <summary>
        /// [DEPREACATED] output type declaration into a single file
        /// </summary>
        public bool singleTSD = true;

        /// <summary>
        /// enable parameter type checking for methods (even if no overloading exists)
        /// </summary>
        public bool alwaysCheckArgType = false;

        /// <summary>
        /// 即使只有唯一匹配的方法/函数, 也进行参数数量检查 
        /// </summary>
        public bool alwaysCheckArgc = true;

        /// <summary>
        /// [EXPERIMENTAL, UNFINISHED] generate obfuscated binding code
        /// </summary>
        public bool randomizedBindingCode = false;

        // 生成类型绑定代码类型前缀
        public string typeBindingPrefix = "QuickJS_";

        /// <summary>
        /// 生成的绑定类所在命名空间 (in C#)
        /// </summary>
        public string ns = "jsb";

        /// <summary>
        /// 为没有命名空间的 C# 类型, 指定一个默认模块名
        /// </summary>
        public string defaultJSModule = "global";

        /// <summary>
        /// 生成文件的额外后缀
        /// </summary>
        public string extraExt = "";

        /// <summary>
        /// 生成代码中的换行符风格 (cr, lf, crlf), 不指定时将使用当前平台默认风格
        /// </summary>
        public string newLineStyle = "";

        /// <summary>
        /// 生成代码中的缩进
        /// </summary>
        public string tab = "    ";

        /// <summary>
        /// 跳过指定的 BindingProcess
        /// </summary>
        public List<string> skipExtras = new List<string>(new string[]
        {
            // "FairyGUI",
            // "UnityEditor",
            // "Example",
        });

        /// <summary>
        /// 执行代码生成后对指定的目录进行文件清理 (未在本次生成文件列表中的文件将被删除)
        /// </summary>
        public List<string> cleanupDir = new List<string>(new string[]
        {
            "Assets/Generated",
        });

        // 默认不导出任何类型, 需要指定导出类型列表
        public List<string> explicitAssemblies = new List<string>(new string[]
        {
            // "Assembly-CSharp-firstpass",
            "Assembly-CSharp",
        });

        /// <summary>
        /// 在此列表中指定的指定 Assembly 将默认导出其所包含的所有类型
        /// </summary>
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

        // extremely strong impact on performance 
        // public List<string> typePrefixBlacklist = new List<string>(new string[]
        // {
        //     "JetBrains.",
        //     "Unity.Collections.",
        //     "Unity.Jobs.",
        //     "Unity.Profiling.",
        //     "UnityEditor.",
        //     "UnityEditorInternal.",
        //     "UnityEngineInternal.",
        //     "UnityEditor.Experimental.",
        //     "UnityEngine.Experimental.",
        //     "Unity.IO.LowLevel.",
        //     "Unity.Burst.",
        //     "UnityEngine.Assertions.",
        // });

        public List<string> typeFullNameBlacklist = new List<string>(new string[]
        {
            "System.SpanExtensions",
            "UnityEditor.AppleMobileArchitecture",
            "UnityEngine.AndroidJavaException",
            "UnityEngine.AndroidJavaProxy",
            "UnityEngine.AndroidJavaObject",
            "UnityEngine.AndroidJavaClass",
            "UnityEngine.AndroidJNIHelper",
            "UnityEngine.AndroidJNI",
            "UnityEngine.AndroidActivityIndicatorStyle",
            "UnityEngine.Android.AndroidDevice",
            "UnityEngine.Android.Permission",
            "UnityEngine.AndroidInput",
            "UnityEditor.AndroidETC2Fallback",
            "UnityEditor.AndroidBuildSystem",
            "UnityEditor.AndroidBuildType",
            "UnityEditor.AndroidMinification",
            "UnityEditor.AndroidArchitecture",
            "UnityEditor.AndroidSdkVersions",
            "UnityEditor.AndroidPreferredInstallLocation",
            "UnityEditor.AndroidShowActivityIndicatorOnLoading",
            "UnityEditor.AndroidGamepadSupportLevel",
            "UnityEditor.AndroidSplashScreenScale",
            "UnityEditor.AndroidBlitType",
            "UnityEditor.AndroidETC2FallbackOverride",
            "UnityEditor.Android.IPostGenerateGradleAndroidProject",
            "UnityEditor.WSASubtarget",
            "UnityEditor.WSASDK",
            "UnityEditor.WSAUWPBuildType",
            "UnityEditor.WSABuildAndRunDeployTarget",
            "UnityEditor.WSABuildType",
            "UnityEditor.HumanTemplate", 
            "UnityEditor.TakeInfo", 
            "UnityEditor.L10n", 
            "UnityEditor.Build.Reporting", 
            "UnityEditor.TypeCache", 
            "UnityEditor.SceneManagement.ObjectOverride", 
            "UnityEditor.SceneManagement.PrefabOverride", 
            "UnityEditor.SceneManagement.AddedGameObject", 
            "UnityEditor.SceneManagement.AddedComponent", 
            "UnityEditor.SceneManagement.RemovedComponent", 
        });

        public List<string> namespaceBlacklist = new List<string>(new string[]
        {
            "TreeEditor",
            "UnityEditor.U2D", 
            "UnityEditor.Rendering", 
            "UnityEditor.AssetImporters", 
            "UnityEditor.Audio", 
            "UnityEditor.Build.Player", 
            "Unity.CodeEditor",
            "UnityEditor.Sprites",
            "UnityEditor.Experimental",
            "UnityEngine.Assertions",
            "UnityEngine.Experimental.AI",
            "UnityEngine.Experimental.Animations",
            "UnityEngine.Experimental.AssetBundlePatching",
            "UnityEngine.Experimental.Audio",
            "UnityEngine.Experimental.Networking.PlayerConnection",
            "UnityEngine.Experimental.GlobalIllumination",
            "UnityEngine.Experimental.Playables",
            "UnityEngine.Experimental.Rendering",
            "UnityEngine.Experimental.TerrainAPI",
            "UnityEngine.Experimental.XR",
            "UnityEngine.Experimental.Video",
            "UnityEngine.Jobs",
            "Unity.Jobs",
            "Unity.Profiling",
            "Unity.Profiling.LowLevel",
            "Unity.Jobs.LowLevel.Unsafe",
            "Unity.Collections.LowLevel.Unsafe",
            "UnityEngine.Apple.ReplayKit",
            "UnityEditor.VisualStudioIntegration",
            "UnityEditor.Profiling.Memory.Experimental",
            "UnityEditor.Profiling",
            "UnityEditor.UIElements",
            "UnityEditor.Animations",
            "UnityEditor.Experimental.AssetImporters",
            "UnityEditor.Experimental.SceneManagement",
            "UnityEngineInternal",
            "UnityEditorInternal",
            "UnityEditorInternal.VersionControl",
            "JetBrains.Annotations",
            "Unity.IO.LowLevel.Unsafe",
            "Unity.Collections",
            "UnityEditor.UnityLinker",
            "UnityEditor.Il2Cpp",
            "UnityEditor.Experimental.Rendering",
            "UnityEditor.Experimental.GraphView",
            "UnityEditor.Experimental.Licensing",
            "UnityEditor.Experimental.TerrainAPI",
            "UnityEditor.XR",
            "UnityEditor.XR.Daydream",
            "UnityEditorInternal.Profiling.Memory.Experimental",
            "UnityEditorInternal.Profiling.Memory.Experimental.FileFormat",
            "UnityEditor.Build.Content",
            "UnityEditor.AnimatedValues",
            "UnityEditor.AI",
            "UnityEditor.Macros",
            "UnityEditor.Experimental.Networking.PlayerConnection",
            "UnityEditor.Compilation",
            "UnityEditor.Networking.PlayerConnection",
            "UnityEditor.VersionControl",
            "UnityEditor.Localization.Editor",
            "UnityEditor.CrashReporting",
            "UnityEditor.ShaderProfiler",
            "UnityEditor.MemoryProfiler",
            "UnityEditor.ShortcutManagement",
            "UnityEditorInternal.VR",
            "UnityEditor.Presets",
            "UnityEditor.PackageManager",
            "UnityEditor.PackageManager.UI",
            "UnityEditor.PackageManager.Requests",
        });

        public List<string> assemblyBlacklist = new List<string>(new string[]
        {
            "ExCSS.Unity",
            "Unity.Cecil",
            "Unity.Cecil.Mdb",
            "Unity.Cecil.Pdb",
            "Unity.Cecil.Rocks",
            "Unity.CecilTools",
        });

        #endregion

        #region Runtime Methods

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

        #endregion
    }
}