using System;
using System.Collections.Generic;

namespace QuickJS.Module
{
    using Native;
    using Binding;

    public class ProxyModuleRegister : IModuleRegister
    {
        // Key: FullName in module
        // Value: Type
        private Dictionary<string, Type> _typeTree; // root space

        public bool isReloadSupported => false;

        public ProxyModuleRegister()
        {
            _typeTree = new Dictionary<string, Type>();
        }

        public void Unload()
        {
        }

        public void Add(Type type, string[] ns)
        {
            for (int i = 0, length = ns.Length; i < length; ++i)
            {
                var intermediate = string.Join(".", ns, 0, i + 1);
                if (!_typeTree.TryGetValue(intermediate, out var slot) || slot == null)
                {
                    _typeTree[intermediate] = i != length - 1 ? null : type;
                }
            }
        }

        private JSValue LoadType(ScriptContext context, string typePath)
        {
            Type type;
            if (_typeTree.TryGetValue(typePath, out type))
            {
                return type != null ? context.GetTypeDB().GetConstructorOf(type) : JSApi.JS_NewObject(context);
            }
            return JSApi.JS_UNDEFINED;
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        private static JSValue js_load_type(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc != 2 || !argv[0].IsString() || !argv[1].IsString())
            {
                return ctx.ThrowInternalError("string expected");
            }

            var module_id = JSApi.GetString(ctx, argv[0]);
            var type_path = JSApi.GetString(ctx, argv[1]);

            if (module_id == null || type_path == null)
            {
                return ctx.ThrowInternalError("get invalid string");
            }

            var context = ScriptEngine.GetContext(ctx);
            var runtime = context.GetRuntime();
            var proxy = runtime.FindModuleResolver<StaticModuleResolver>()?.GetModuleRegister<ProxyModuleRegister>(module_id);
            return proxy != null ? proxy.LoadType(context, type_path) : JSApi.JS_UNDEFINED;
        }

        // the given exports object is ignored, type loader uses a Proxy object as new exports
        public unsafe JSValue Load(ScriptContext context, string resolved_id, JSValue module_obj, JSValue exports_obj)
        {
            var ctx = (JSContext)context;
            var sourceString = @"(function (module_id, load_type) {
                let new_proxy;
                new_proxy = function (t, last) {
                    if (typeof t === 'undefined') {
                        throw new Error(`type '${last}' does not exist`);
                    }
                    return new Proxy(t, {
                        set: function (target, p, value) {
                            if (typeof p !== 'string') {
                                throw new Error(`symbol is not acceptable`);
                            }
                            if (typeof target[p] !== 'undefined') {
                                target[p] = value;
                                return true;
                            }
                            let type_path = typeof last === 'string' ? last + '.' + p : p;
                            throw new Error(`'${type_path}' is not writable`);
                        }, 
                        get: function (target, p) {
                            let o = target[p];
                            if (typeof o === 'undefined' && typeof p === 'string') {
                                let type_path = typeof last === 'string' ? last + '.' + p : p;
                                o = target[p] = new_proxy(load_type(module_id, type_path), type_path)
                            }
                            return o;
                        }
                    })
                };
                return new_proxy({}); 
            })";
            var proxyGen = ScriptRuntime.EvalSource(ctx, sourceString, "eval", false);
            var argv = stackalloc JSValue[2]
            {
                ctx.NewString(resolved_id),
                JSApi.JSB_NewCFunction(ctx, js_load_type, context.GetAtom("$LoadType"), 2),
            };
            var retVal = JSApi.JS_Call(ctx, proxyGen, JSApi.JS_UNDEFINED, 2, argv);
            JSApi.JS_FreeValue(ctx, proxyGen);
            JSApi.JS_FreeValue(ctx, argv[0]);
            JSApi.JS_FreeValue(ctx, argv[1]);
            JSApi.JS_SetProperty(ctx, module_obj, context.GetAtom("exports"), JSApi.JS_DupValue(ctx, retVal));
            return retVal;
        }
    }
}
