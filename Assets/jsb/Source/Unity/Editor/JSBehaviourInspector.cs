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
    }
}

#endif
