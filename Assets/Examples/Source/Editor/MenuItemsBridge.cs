using System;
using System.IO;
using System.Linq;

namespace Example.Editor
{
    using UnityEngine;
    using UnityEditor;

    // 调用脚本定义的 EditorWindow 的示例代码
    public class MenuItemsBridge
    {
        [MenuItem("My Examples/My Editor Window")]
        public static void ShowMyEditorWindow()
        {
            QuickJS.Unity.EditorRuntime.ShowWindow("editor/my_editor_window", "MyEditorWindow");
        }
    }
}
