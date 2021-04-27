using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Binding
{
    public interface IBindingCallback
    {
        void BeginStaticModule(string moduleName);
        void AddTypeReference(string moduleName, TypeBindingInfo typeBindingInfo, string[] elements, string jsName);
        void EndStaticModule(string moduleName);

        void OnPreGenerateDelegate(DelegateBridgeBindingInfo bindingInfo);
        void OnPostGenerateDelegate(DelegateBridgeBindingInfo bindingInfo);

        // return true to cancel the binding process
        bool OnTypeGenerating(TypeBindingInfo typeBindingInfo, int current, int total);

        void OnGenerateFinish();
    }
}
