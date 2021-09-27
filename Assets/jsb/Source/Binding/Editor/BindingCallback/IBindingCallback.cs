using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Binding
{
    public interface IBindingCallback
    {
        void Begin(BindingManager bindingManager);
        void BeginStaticModule(string moduleName, int capacity);
        void AddTypeReference(string moduleName, TypeBindingInfo typeBindingInfo);
        void EndStaticModule(string moduleName);

        void AddDelegate(DelegateBridgeBindingInfo bindingInfo);
        void End();
    }
}
