#if !JSB_UNITYLESS
using System;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using Native;
    using UnityEngine;

    [Serializable]
    public struct JSScriptRef
    {
        // 编辑器通过 sourceFile 尝试拆解出正确的 modulePath 和 className
        public string sourceFile;

        public string modulePath;
        public string className;

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(modulePath) && string.IsNullOrEmpty(className);
        }

        public static string ToClassPath(string modulePath, string className)
        {
            if (string.IsNullOrEmpty(modulePath))
            {
                return className ?? string.Empty;
            }
            
            return string.IsNullOrEmpty(className) ? string.Empty : $"{modulePath.Replace('/', '.')}.{className}";
        }

        public string ToClassPath()
        {
            return ToClassPath(modulePath, className);
        }
    }
}
#endif
