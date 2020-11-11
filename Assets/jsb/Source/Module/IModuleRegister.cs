using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QuickJS.Module
{
    using Native;
    using Binding;

    public delegate ClassDecl ModuleExportsBind(TypeRegister register);

    public interface IModuleRegister
    {
        void Load(ScriptContext context, JSValue module_obj, JSValue exports_obj);
        void Unload();
    }
}
