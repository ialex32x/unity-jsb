using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public interface IBindingCallback
    {
        void BeginStaticModule(string moduleName);
        void AddTypeReference(string moduleName, TypeBindingInfo typeBindingInfo, string[] elements, string jsName);
        void EndStaticModule(string moduleName);

        void OnPreGenerateDelegate(DelegateBridgeBindingInfo bindingInfo);
        void OnPostGenerateDelegate(DelegateBridgeBindingInfo bindingInfo);

        bool OnTypeGenerating(TypeBindingInfo typeBindingInfo, int current, int total);
        void OnGenerateFinish();
    }
}
