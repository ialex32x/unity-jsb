using System;
using System.Collections.Generic;

namespace QuickJS.Module
{
    using Utils;
    using Native;

    public class StaticModuleResolver : IModuleResolver
    {
        private Dictionary<string, IModuleRegister> _register = new Dictionary<string, IModuleRegister>();

        public StaticModuleResolver AddStaticModule(string module_id, ModuleExportsBind bind)
        {
            return AddStaticModule(module_id, new FuncModuleRegister(bind));
        }

        public StaticModuleResolver AddStaticModule(string module_id, RawModuleBind bind)
        {
            return AddStaticModule(module_id, new RawModuleRegister(bind));
        }

        public StaticModuleResolver AddStaticModule(string module_id, IModuleRegister moduleRegister)
        {
            _register.Add(module_id, moduleRegister);
            return this;
        }

        public bool ResolveModule(IFileSystem fileSystem, IPathResolver pathResolver, string parent_module_id, string module_id, out string resolved_id)
        {
            if (_register.ContainsKey(module_id))
            {
                resolved_id = module_id;
                return true;
            }
            resolved_id = null;
            return false;
        }

        public JSValue LoadModule(ScriptContext context, string resolved_id)
        {
            IModuleRegister moduleRegister;
            if (_register.TryGetValue(resolved_id, out moduleRegister))
            {
                var exports_obj = JSApi.JS_NewObject(context);
                var module_obj = context._new_commonjs_module(resolved_id, exports_obj, true);

                moduleRegister.Load(context, module_obj, exports_obj);
                
                JSApi.JS_FreeValue(context, exports_obj);
                JSApi.JS_FreeValue(context, module_obj);
            }

            return JSApi.JS_ThrowInternalError(context, "invalid static module loader");
        }
    }
}