using System;
using System.IO;
using System.Linq;

namespace jsb.Editor
{
    using UnityEngine;
    using UnityEditor;

    // 调用脚本定义的 EditorWindow 的示例代码
    public class MenuItemsBridge
    {
        public static void Show(string module, string typename)
        {
            QuickJS.Unity.EditorRuntime.Eval($"UnityEditor.EditorWindow.GetWindow(require('{module}').{typename}).Show()");
        }

        [MenuItem("My Examples/My Editor Window")]
        public static void ShowMyEditorWindow()
        {
            Show("editor/my_editor_window", "MyEditorWindow");
        }
    }
}
