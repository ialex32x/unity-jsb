using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QuickJS.Module
{
    using Native;
    using Binding;

    public delegate void ModuleExportsBind(TypeRegister register);

    public delegate JSValue RawModuleBind(ScriptContext context);

    public delegate void ModuleLoader(ScriptContext context, JSValue module_obj, JSValue exports_obj);

    public interface IModuleRegister
    {
        void Load(ScriptContext context, JSValue module_obj, JSValue exports_obj);
    }
}