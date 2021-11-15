using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QuickJS.Module
{
    using Native;
    using Binding;

    public class ValueModuleRegister : IModuleRegister
    {
        private ScriptRuntime _runtime;
        private JSValue _rawValue;

        public bool isReloadSupported => false;

        public ValueModuleRegister(ScriptRuntime runtime, JSValue bind)
        {
            _runtime = runtime;
            _rawValue = JSApi.JS_DupValue(_runtime.GetMainContext(), bind);
        }

        public void Unload()
        {
            if (!_rawValue.IsUndefined())
            {
                var rawValue = _rawValue;
                _rawValue = JSApi.JS_UNDEFINED;
                _runtime.FreeValue(rawValue);
            }
        }

        public void Load(ScriptContext context, JSValue module_obj, JSValue exports_obj)
        {
            JSApi.JS_SetPropertyStr(context, module_obj, "exports", JSApi.JS_DupValue(context, _rawValue));
        }
    }
}
