using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QuickJS.Module
{
    using Native;
    using Binding;

    public class RawModuleRegister : IModuleRegister
    {
        private RawModuleBind _bind;

        public RawModuleRegister(RawModuleBind bind)
        {
            _bind = bind;
        }

        public void Load(ScriptContext context, JSValue module_obj, JSValue exports_obj)
        {
            var v = _bind(context);
            JSApi.JS_SetPropertyStr(context, module_obj, "exports", v);
        }
    }
}
