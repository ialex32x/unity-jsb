using System;
using System.IO;
using System.Linq;

namespace Example.Editor
{
    using UnityEngine;
    using UnityEditor;

    public class MenuItemsBridge
    {
        [MenuItem("My Examples/TestBuild")]
        public static void ShowMyEditorWindow()
        {
            UnityEditor.BuildPipeline.BuildPlayer(new string[] { "Assets/Examples/Scenes/BasicRun.unity" }, "Build/macos.app", BuildTarget.StandaloneOSX, BuildOptions.Development);
        }
    }
}
