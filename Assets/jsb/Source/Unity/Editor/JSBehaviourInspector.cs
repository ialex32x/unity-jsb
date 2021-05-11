#if !JSB_UNITYLESS

namespace QuickJS.Unity
{
    using UnityEditor;

    [CustomEditor(typeof(JSBehaviour))]
    public class JSBehaviourInspector : JSInspectorBase<JSBehaviour>
    {
    }
}

#endif
