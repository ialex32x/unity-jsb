using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QuickJS.Module
{
    using Native;
    using Binding;

    public class AMDModuleRegister : IModuleRegister
    {
        private JSContext _ctx;
        private string[] _deps;
        private JSValue _loader;

        public bool isReloadSupported => false;

        public AMDModuleRegister(JSContext ctx, string[] deps, JSValue loader)
        {
            _ctx = ctx;
            _deps = deps;
            _loader = JSApi.JS_DupValue(ctx, loader);
        }

        public void Unload()
        {
            if (_ctx.IsValid())
            {
                if (!_loader.IsUndefined())
                {
                    JSApi.JS_FreeValue(_ctx, _loader);
                    _loader = JSApi.JS_UNDEFINED;
                }

                _ctx = JSContext.Null;
            }
        }

        public unsafe void Load(ScriptContext context, JSValue module_obj, JSValue exports_obj)
        {
            var len = _deps.Length;
            var values = stackalloc JSValue[len];
            var ctx = (JSContext)context;
            var require_obj = context._CreateRequireFunction(module_obj);
            var filename_obj = JSApi.JS_GetProperty(ctx, module_obj, context.GetAtom("filename"));
            var dirname_obj = JSApi.JS_NULL;
            var require_argv = stackalloc JSValue[5] { JSApi.JS_DupValue(ctx, exports_obj), JSApi.JS_DupValue(ctx, require_obj), JSApi.JS_DupValue(ctx, module_obj), filename_obj, dirname_obj, };

            for (var i = 0; i < len; ++i)
            {
                var dep_id = _deps[i];
                switch (dep_id)
                {
                    case "require": values[i] = JSApi.JS_DupValue(ctx, require_obj); break;
                    case "exports": values[i] = JSApi.JS_DupValue(ctx, exports_obj); break;
                    default:
                        var rval = JSApi.JS_Call(ctx, require_obj, JSApi.JS_UNDEFINED, 5, require_argv);
                        JSApi.JS_FreeValue(ctx, rval);
                        break;
                }
            }

            JSApi.JS_FreeValue(ctx, require_obj);
            for (var i = 0; i < len; ++i)
            {
                JSApi.JS_FreeValue(ctx, values[i]);
            }
            for (var i = 0; i < 5; ++i)
            {
                JSApi.JS_FreeValue(ctx, require_argv[i]);
            }
        }
    }
}
