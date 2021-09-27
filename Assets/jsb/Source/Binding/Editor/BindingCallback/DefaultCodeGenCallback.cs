using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Binding
{
    public class DefaultCodeGenCallback : ICodeGenCallback
    {
        private BindingManager _bindingManager;

        public void Begin(BindingManager bindingManager)
        {
            _bindingManager = bindingManager;
        }

        public void End()
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

        public void OnSourceCodeEmitted(CodeGenerator cg, string csOutDir, string csName, SourceCodeType type, string source)
        {
            if (!Directory.Exists(csOutDir))
            {
                Directory.CreateDirectory(csOutDir);
            }

            var csPath = Path.Combine(csOutDir, csName);
            cg.WriteAllText(csPath, source);
            _bindingManager.AddOutputFile(csOutDir, csPath);
        }
    }
}
