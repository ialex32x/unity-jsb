using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Binding
{
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
#if JSB_UNITYLESS
            return false;
#else
            return UnityEditor.EditorUtility.DisplayCancelableProgressBar(
                "Generating",
                $"{current}/{total}: {typeBindingInfo.FullName}",
                (float)current / total);
#endif
        }

        public void OnGenerateFinish()
        {
#if !JSB_UNITYLESS
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
        }
    }
}
