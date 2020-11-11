using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QuickJS.Module
{
    using Native;
    using Binding;

    public class ProxyModuleRegister : IModuleRegister
    {
        private ScriptRuntime _runtime;
        private JSValue _exports = JSApi.JS_UNDEFINED;
        private List<TypeReg> _types = new List<TypeReg>();

        public ProxyModuleRegister(ScriptRuntime runtime)
        {
            _runtime = runtime;
            _runtime.OnDestroy += OnDestroy;
        }

        public void Unload()
        {
            OnDestroy(_runtime);
        }

        private void OnDestroy(ScriptRuntime runtime)
        {
            if (!_exports.IsUndefined())
            {
                var exports = _exports;
                _exports = JSApi.JS_UNDEFINED;
                _runtime.OnDestroy -= OnDestroy;
                _runtime.FreeValue(exports);
            }
        }

        public void Add(Type type, ModuleExportsBind bind, string[] ns)
        {
            _types.Add(new TypeReg()
            {
                loaded = false,
                ns = ns,
                bind = bind,
            });
        }

        public void LoadTypes(TypeRegister register)
        {
            if (_exports.IsUndefined())
            {
                _exports = JSApi.JS_NewObject(register);
                for (int i = 0, count = _types.Count; i < count; i++)
                {
                    var reg = _types[i];
                    var clazz = reg.bind(register);

                    SetExports(register, _exports, clazz.GetConstructor(), reg.ns, 0);
                }
            }
        }

        private void SetExports(TypeRegister register, JSValue thisObject, JSValue constructor, string[] ns, int index)
        {
            var ctx = (JSContext)register;
            var name = register.GetAtom(ns[index]);

            if (index == ns.Length - 1)
            {
                JSApi.JS_SetProperty(ctx, thisObject, name, constructor);
            }
            else
            {
                var tValue = JSApi.JS_GetProperty(ctx, thisObject, name);

                if (!tValue.IsObject())
                {
                    tValue = JSApi.JS_NewObject(ctx);
                    JSApi.JS_SetProperty(ctx, thisObject, name, JSApi.JS_DupValue(ctx, tValue));
                }

                SetExports(register, tValue, constructor, ns, index + 1);
                JSApi.JS_FreeValue(ctx, tValue);
            }
        }

        public void Load(ScriptContext context, JSValue module_obj, JSValue exports_obj)
        {
            var register = new TypeRegister(context);
            LoadTypes(register);
            JSApi.JS_SetProperty(context, module_obj, context.GetAtom("exports"), JSApi.JS_DupValue(context, _exports));
            register.Finish();
        }
    }
}
