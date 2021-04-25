using System;
using System.Collections.Generic;

namespace QuickJS.Module
{
    using Utils;
    using Native;

    public class StaticModuleResolver : IModuleResolver
    {
        private Dictionary<string, IModuleRegister> _modRegisters = new Dictionary<string, IModuleRegister>();

        /// <summary>
        /// [临时方案] 直接载入所有绑定类型
        /// </summary>
        public void Warmup(ScriptContext context)
        {
            var typeRegister = new Binding.TypeRegister(context);

            foreach (var kv in _modRegisters)
            {
                var pmr = kv.Value as ProxyModuleRegister;
                if (pmr != null)
                {
                    pmr.LoadTypes(typeRegister);
                }
            }

            typeRegister.Finish();
        }

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

        public bool ReloadModule(ScriptContext context, string resolved_id)
        {
            // unsupported
            return false;
        }

        public JSValue LoadModule(ScriptContext context, string parent_module_id, string resolved_id)
        {
            IModuleRegister moduleRegister;
            if (_modRegisters.TryGetValue(resolved_id, out moduleRegister))
            {
                var exports_obj = JSApi.JS_NewObject(context);
                var module_obj = context._new_commonjs_resolver_module(resolved_id, "static", exports_obj, true);

                moduleRegister.Load(context, module_obj, exports_obj);

                JSApi.JS_FreeValue(context, exports_obj);
                JSApi.JS_FreeValue(context, module_obj);

                var exports = context.GetAtom("exports");
                return JSApi.JS_GetProperty(context, module_obj, exports);
            }

            return JSApi.JS_ThrowInternalError(context, "invalid static module loader");
        }
    }
}