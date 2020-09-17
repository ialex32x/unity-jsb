
namespace QuickJS.Unity
{
    using UnityEditor;

    [CustomEditor(typeof(ScriptBridge))]
    public class ScriptBridgeInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var inst = target as ScriptBridge;

            EditorGUILayout.TextField("Script Type", inst.scriptTypeName);
        }
    }
}