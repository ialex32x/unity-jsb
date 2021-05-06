using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Binding
{
    public class DefaultCodeGenCallback : ICodeGenCallback
    {
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
