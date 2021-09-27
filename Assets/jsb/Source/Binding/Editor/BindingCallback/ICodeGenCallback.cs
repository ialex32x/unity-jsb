using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Binding
{
    public enum SourceCodeType
    {
        CSharp, 
        TSD, 
    }

    public interface ICodeGenCallback
    {
        void Begin(BindingManager bindingManager);
        
        void End();

        // return true to cancel the binding process
        bool OnTypeGenerating(TypeBindingInfo typeBindingInfo, int current, int total);

        void OnGenerateFinish();

        void OnSourceCodeEmitted(CodeGenerator cg, string csOutDir, string csName, SourceCodeType type, string source);
    }
}
