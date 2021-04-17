using System;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using Native;
    using UnityEngine;

    public enum EJSBehaviourLoadState
    {
        Unset, // 未设置 (待定)
        Unresolved, // 未载入
        Resolved, // 已载入
        Failed, // 无法载入
        Dynamic, // created by game-runtime 
    }

    [Serializable]
    public struct JSBehaviourScriptRef
    {
        // 编辑器通过 sourceFile 尝试拆解出正确的 modulePath 和 className
        public string sourceFile;

        public string modulePath;
        public string className;
        public EJSBehaviourLoadState state; // (待定)
    }
}
