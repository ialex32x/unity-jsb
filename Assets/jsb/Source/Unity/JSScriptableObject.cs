#if !JSB_UNITYLESS
using System;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using Native;
    using UnityEngine;

    [CreateAssetMenu(fileName = "js_data", menuName = "JSScriptableObject Asset", order = 100)]
    public class JSScriptableObject : ScriptableObject, ISerializationCallbackReceiver
    {
        // internal use only
        public JSBehaviourProperties _properties;

        public void OnBeforeSerialize()
        {
            Debug.Log("serialize");
        }

        public void OnAfterDeserialize()
        {
            Debug.Log("deserialize");
        }
    }
}
#endif
