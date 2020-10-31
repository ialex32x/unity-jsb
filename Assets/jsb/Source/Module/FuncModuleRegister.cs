using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QuickJS.Module
{
    using Native;
    using Binding;

    public class FuncModuleRegister : IModuleRegister
    {
        private ModuleExportsBind _bind;

        public FuncModuleRegister(ModuleExportsBind bind)
        {
            _bind = bind;
        }

        public void Load(ScriptContext context, JSValue module_obj, JSValue exports_obj)
        {
            var rt = context.GetRuntime();
            var register = new TypeRegister(rt, context, JSApi.JS_DupValue(context, exports_obj));
            _bind(register);
            register.Finish();
        }
    }
}
