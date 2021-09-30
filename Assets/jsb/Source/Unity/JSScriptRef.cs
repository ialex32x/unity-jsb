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
        /// <summary>
        /// sourceFile will be broken down into modulePath and className by the editor, 
        /// but not necessary for instantiating the script class instance
        /// </summary>
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
