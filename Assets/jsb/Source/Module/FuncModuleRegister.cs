using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QuickJS.Module
{
    using Native;
    using Binding;

    // 一个绑定函数代表一个类型注册为一个模块
    public class FuncModuleRegister : IModuleRegister
    {
        private ModuleExportsBind _bind;

        public FuncModuleRegister(ModuleExportsBind bind)
        {
            _bind = bind;
        }

        public void Load(ScriptContext context, JSValue module_obj, JSValue exports_obj)
        {
            var register = new TypeRegister(context);
            var clazz = _bind(register);
            JSApi.JS_SetProperty(context, module_obj, register.GetAtom("exports"), clazz.GetConstructor());
            register.Finish();
        }
    }
}
