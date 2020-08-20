using UnityEditor;

namespace QuickJS.Editor
{
    using UEditor = UnityEditor.Editor;

    [CustomEditor(typeof(ScriptBridge))]
    public class ScriptBridgeInspector : UEditor
    {
        public override void OnInspectorGUI()
        {
            var inst = target as ScriptBridge;

            EditorGUILayout.TextField("Script Type", inst.scriptTypeName);
        }
    }
}