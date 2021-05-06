using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Binding
{
    public interface IBindingCallback
    {
        void BeginStaticModule(string moduleName);
        void AddTypeReference(string moduleName, TypeBindingInfo typeBindingInfo);
        void EndStaticModule(string moduleName);

        void AddDelegate(DelegateBridgeBindingInfo bindingInfo);
    }
}
