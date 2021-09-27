using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace QuickJS.Binding
{
    public class DefaultCodeGenCallback : ICodeGenCallback
    {
        private BindingManager _bindingManager;

        public void OnCodeGenBegin(BindingManager bindingManager)
        {
            _bindingManager = bindingManager;
        }

        public void OnCodeGenEnd()
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

        public void OnSourceCodeEmitted(CodeGenerator cg, string codeOutDir, string codeName, SourceCodeType type, string source)
        {
            if (!Directory.Exists(codeOutDir))
            {
                Directory.CreateDirectory(codeOutDir);
            }

            var filename = codeName;
            switch (type)
            {
                case SourceCodeType.CSharp: filename += ".cs"; break;
                case SourceCodeType.TSD: filename += "d.ts" + _bindingManager.prefs.extraExtForTypescript; break;
            }
            var csPath = Path.Combine(codeOutDir, filename);
            cg.WriteAllText(csPath, source);
            _bindingManager.AddOutputFile(codeOutDir, csPath);
        }

        public void OnGenerateBindingList(CodeGenerator cg, IEnumerable<IGrouping<string, TypeBindingInfo>> modules)
        {
            cg.GenerateBindingList(typeof(Values).Namespace, typeof(Values).Name, modules, true);
        }
    }
}
