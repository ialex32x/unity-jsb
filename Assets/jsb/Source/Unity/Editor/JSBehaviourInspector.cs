#if !JSB_UNITYLESS

namespace QuickJS.Unity
{
    using UnityEditor;

    [CustomEditor(typeof(JSBehaviour))]
    public class JSBehaviourInspector : JSInspectorBase<JSBehaviour>
    {
        protected override JSScriptClassType GetScriptClassType()
        {
            return JSScriptClassType.MonoBehaviour;
        }

        protected override void DrawSourceView()
        {
            base.DrawSourceView();

            var updatableOld = _target is JSBehaviourFull;
            var updatableNew = EditorGUILayout.Toggle("Updatable", updatableOld);

            if (updatableNew != updatableOld)
            {
                var mb = _target;
                var name = typeof(JSBehaviour).Name;
                var assetGuids = AssetDatabase.FindAssets($"t:Script {name}");
                foreach (var assetGuid in assetGuids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                    var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                    UnityEngine.Debug.LogFormat("{0} {1} {2}", asset.GetType(), asset.name, asset.GetClass() == typeof(JSBehaviour));
                }
                // var ms = MonoScript.FromMonoBehaviour(mb);
                // var prop = serializedObject.FindProperty("m_Script");
                // serializedObject.Update();
                // prop.objectReferenceValue = monoScript;
            }
        }
    }
}

#endif
