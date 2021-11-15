using System;
using System.Collections.Generic;

namespace QuickJS.Module
{
    using Utils;
    using Native;

    public class StaticModuleResolver : IModuleResolver
    {
        private Dictionary<string, IModuleRegister> _modRegisters = new Dictionary<string, IModuleRegister>();

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
            IModuleRegister oldRegister;
            if (_modRegisters.TryGetValue(module_id, out oldRegister))
            {
                oldRegister.Unload();
            }
            _modRegisters[module_id] = moduleRegister;
            return this;
        }

        public bool ContainsModule(IFileSystem fileSystem, IPathResolver pathResolver, string resolved_id)
        {
            return _modRegisters.ContainsKey(resolved_id);
        }

        public bool ResolveModule(IFileSystem fileSystem, IPathResolver pathResolver, string parent_module_id, string module_id, out string resolved_id)
        {
            if (_modRegisters.ContainsKey(module_id))
            {
                resolved_id = module_id;
                return true;
            }
            resolved_id = null;
            return false;
        }

        public bool ReloadModule(ScriptContext context, string resolved_id, JSValue module_obj, out JSValue exports_obj)
        {
            // unsupported
            exports_obj = JSApi.JS_UNDEFINED;
            return false;
        }

        public JSValue LoadModule(ScriptContext context, string parent_module_id, string resolved_id, bool set_as_main)
        {
            IModuleRegister moduleRegister;
            if (_modRegisters.TryGetValue(resolved_id, out moduleRegister))
            {
                var ctx = (JSContext)context;
                var exports_obj = JSApi.JS_NewObject(ctx);
                var module_obj = context._new_commonjs_resolver_module(resolved_id, "static", exports_obj, false, set_as_main);

                moduleRegister.Load(context, module_obj, exports_obj);
                
                JSApi.JS_SetProperty(ctx, module_obj, context.GetAtom("loaded"), JSApi.JS_NewBool(ctx, true));
                var rval = JSApi.JS_GetProperty(ctx, module_obj, context.GetAtom("exports"));
                
                JSApi.JS_FreeValue(ctx, exports_obj);
                JSApi.JS_FreeValue(ctx, module_obj);

                return rval;
            }

            return JSApi.JS_ThrowInternalError(context, "invalid static module loader");
        }

        public IModuleRegister GetModuleRegister(string module_id)
        {
            IModuleRegister moduleRegister;
            return _modRegisters.TryGetValue(module_id, out moduleRegister) ? moduleRegister : null;
        }
    }
}