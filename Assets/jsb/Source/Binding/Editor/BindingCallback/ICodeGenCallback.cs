using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Binding
{
    public interface ICodeGenCallback
    {
        // return true to cancel the binding process
        bool OnTypeGenerating(TypeBindingInfo typeBindingInfo, int current, int total);

        void OnGenerateFinish();
    }
}
