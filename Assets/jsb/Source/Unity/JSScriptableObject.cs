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
        // 在编辑器运行时下与 js 脚本建立链接关系
        public JSScriptRef scriptRef;

        [SerializeField]
        private JSScriptProperties _properties;

        // internal use only
        public JSScriptProperties properties => _properties;

        private JSContext _ctx = JSContext.Null;
        private JSValue _this_obj = JSApi.JS_UNDEFINED;

        public static implicit operator JSValue(JSScriptableObject self)
        {
            return self._this_obj;
        }

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
