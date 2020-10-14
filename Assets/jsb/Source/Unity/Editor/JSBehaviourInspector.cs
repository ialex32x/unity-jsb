
namespace QuickJS.Unity
{
    using UnityEditor;
    using Native;

    [CustomEditor(typeof(JSBehaviour))]
    public class JSBehaviourInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var inst = target as JSBehaviour;

            EditorGUILayout.TextField("Script Type", inst.scriptTypeName);
            inst.ForEachProperty((ctx, atom, prop) =>
            {
                var strValue = JSApi.JS_AtomToString(ctx, atom);
                var str = JSApi.GetString(ctx, strValue);
                JSApi.JS_FreeValue(ctx, strValue);

                switch (prop.tag)
                {
                    case JSApi.JS_TAG_BOOL:
                        EditorGUILayout.Toggle(str, JSApi.JS_ToBool(ctx, prop) == 1);
                        break;
                    case JSApi.JS_TAG_STRING:
                        {
                            var pres = JSApi.GetString(ctx, prop);
                            EditorGUILayout.TextField(str, pres);
                        }
                        break;
                    case JSApi.JS_TAG_FLOAT64:
                        {
                            double pres;
                            if (JSApi.JS_ToFloat64(ctx, out pres, prop) == 0)
                            {
                                EditorGUILayout.FloatField(str, (float)pres);
                            }
                            else
                            {
                                EditorGUILayout.TextField(str, "[ParseFailed]");
                            }
                        }
                        break;
                    case JSApi.JS_TAG_INT:
                        {
                            int pres;
                            if (JSApi.JS_ToInt32(ctx, out pres, prop) == 0)
                            {
                                EditorGUILayout.IntField(str, pres);
                            }
                            else
                            {
                                EditorGUILayout.TextField(str, "[ParseFailed]");
                            }
                        }
                        break;
                    default:
                        EditorGUILayout.TextField(str, "[UnknownType]");
                        break;
                }
            });
        }
    }
}