using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QuickJS.Module
{
    using Native;
    using Binding;

    public class ProxyModuleRegister : IModuleRegister
    {
        public class TypeReg
        {
            public bool loaded;
            public ModuleExportsBind bind;
        }

        private Dictionary<Type, TypeReg> _types = new Dictionary<Type, TypeReg>();
        private ScriptRuntime _runtime;

        public ProxyModuleRegister(ScriptRuntime runtime)
        {
            _runtime = runtime;
        }

        public void Add(Type type, ModuleExportsBind bind)
        {
            _types[type] = new TypeReg()
            {
                loaded = false,
                bind = bind,
            };
        }

        public void Load(ScriptContext context, JSValue module_obj, JSValue exports_obj)
        {
            //TODO: create js proxy object as module.exports
        }
    }
}
