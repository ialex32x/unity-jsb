using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public class DefaultBindingCallback : IBindingCallback
    {
        public void BeginStaticModule(string moduleName)
        {
        }

        public void AddTypeReference(string moduleName, TypeBindingInfo typeBindingInfo, string[] elements, string jsName)
        {
        }

        public void EndStaticModule(string moduleName)
        {
        }

        public void OnPreGenerateDelegate(DelegateBridgeBindingInfo bindingInfo)
        {
        }

        public void OnPostGenerateDelegate(DelegateBridgeBindingInfo bindingInfo)
        {
        }

        public bool OnTypeGenerating(TypeBindingInfo typeBindingInfo, int current, int total)
        {
            return EditorUtility.DisplayCancelableProgressBar(
                "Generating",
                $"{current}/{total}: {typeBindingInfo.FullName}",
                (float)current / total);
        }

        public void OnGenerateFinish()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
