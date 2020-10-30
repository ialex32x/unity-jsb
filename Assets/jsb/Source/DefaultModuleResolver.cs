using System;
using System.Collections.Generic;

namespace QuickJS
{
    using Utils;
    using Native;

    public abstract class PathBasedModuleResolver : IModuleResolver
    {
        public PathBasedModuleResolver()
        {
        }

        // 验证模块名可接受
        protected abstract bool OnValidating(string module_id);

        protected abstract bool OnResolvingFile(IFileSystem fileSystem, IPathResolver pathResolver, string fileName, out string searchPath, out string resolvedFileName);

        public bool ResolveModule(IFileSystem fileSystem, IPathResolver pathResolver, string parent_module_id, string module_id, out string resolved_id)
        {
            if (OnValidating(module_id))
            {
                var parent_id = parent_module_id;
                var resolving = module_id;

                // 将相对目录展开
                if (module_id.StartsWith("./") || module_id.StartsWith("../") || module_id.Contains("/./") ||
                    module_id.Contains("/../"))
                {
                    // 显式相对路径直接从 parent 模块路径拼接
                    var parent_path = PathUtils.GetDirectoryName(parent_id);
                    try
                    {
                        resolving = PathUtils.ExtractPath(PathUtils.Combine(parent_path, module_id), '/');
                    }
                    catch
                    {
                        // 不能提升到源代码目录外面
                        throw new Exception(string.Format("invalid module path (out of sourceRoot): {0}", module_id));
                    }
                }

                string searchPath;
                if (OnResolvingFile(fileSystem, pathResolver, resolving, out searchPath, out resolved_id))
                {
                    return true;
                }
            }
            resolved_id = null;
            return false;
        }

        public abstract JSValue LoadModule(ScriptContext context, string resolved_id);
    }

    public class SourceModuleResolver : PathBasedModuleResolver
    {
        [Serializable]
        public class PackageConfig
        {
            public string main;
        }

        private IJsonConverter _jsonConv; // for package.json parsing

        public SourceModuleResolver(IJsonConverter jsonConv)
        {
            _jsonConv = jsonConv;
        }

        protected override bool OnValidating(string module_id)
        {
            // 接受无后缀路径
            return true;
        }

        protected override bool OnResolvingFile(IFileSystem fileSystem, IPathResolver pathResolver, string fileName, out string searchPath, out string resolvedFileName)
        {
            if (pathResolver.ResolvePath(fileSystem, fileName, out searchPath, out resolvedFileName))
            {
                return true;
            }

            // try resolve bytecode file
            if (pathResolver.ResolvePath(fileSystem, fileName + ".js.bytes", out searchPath, out resolvedFileName))
            {
                return true;
            }

            if (pathResolver.ResolvePath(fileSystem, fileName + ".js", out searchPath, out resolvedFileName))
            {
                return true;
            }

            if (pathResolver.ResolvePath(fileSystem, PathUtils.Combine(fileName, "index.js"), out searchPath, out resolvedFileName))
            {
                return true;
            }

            if (_jsonConv != null && pathResolver.ResolvePath(fileSystem, PathUtils.Combine(fileName, "package.json"), out searchPath, out resolvedFileName))
            {
                var packageData = fileSystem.ReadAllText(resolvedFileName);
                if (packageData != null)
                {
                    var packageConfig = _jsonConv.Deserialize(packageData, typeof(PackageConfig)) as PackageConfig;
                    if (packageConfig != null)
                    {
                        var main = PathUtils.Combine(searchPath, fileName, packageConfig.main);
                        if (!main.EndsWith(".js"))
                        {
                            main += ".js";
                        }
                        main = PathUtils.ExtractPath(main, '/');
                        if (fileSystem.Exists(main))
                        {
                            resolvedFileName = main;
                            return true;
                        }
                    }
                }
            }

            resolvedFileName = null;
            return false;
        }

        public override unsafe JSValue LoadModule(ScriptContext context, string resolved_id)
        {
            var fileSystem = context.GetRuntime().GetFileSystem();
            var resolved_id_bytes = Utils.TextUtils.GetNullTerminatedBytes(resolved_id);
            var dirname = PathUtils.GetDirectoryName(resolved_id);
            var source = fileSystem.ReadAllBytes(resolved_id);
            var ctx = (JSContext)context;

            if (source == null)
            {
                return JSApi.JS_ThrowInternalError(ctx, "require module load failed");
            }

            var tagValue = ScriptRuntime.TryReadByteCodeTagValue(source);

            if (tagValue == ScriptRuntime.BYTECODE_ES6_MODULE_TAG)
            {
                return JSApi.JS_ThrowInternalError(ctx, "es6 module can not be loaded by require");
            }

            var module_id_atom = context.GetAtom(resolved_id);
            var dirname_atom = context.GetAtom(dirname);
            var exports_obj = JSApi.JS_NewObject(ctx);
            var require_obj = JSApi.JSB_NewCFunction(ctx, ScriptRuntime.module_require, context.GetAtom("require"), 1, JSCFunctionEnum.JS_CFUNC_generic, 0);
            var module_obj = context._new_commonjs_module(resolved_id, exports_obj, false);
            var main_mod_obj = context._dup_commonjs_main_module();
            var filename_obj = JSApi.JS_AtomToString(ctx, module_id_atom);
            var dirname_obj = JSApi.JS_AtomToString(ctx, dirname_atom);

            JSApi.JS_SetProperty(ctx, require_obj, context.GetAtom("moduleId"), JSApi.JS_DupValue(ctx, filename_obj));
            JSApi.JS_SetProperty(ctx, require_obj, context.GetAtom("main"), main_mod_obj);
            var require_argv = new JSValue[5] { exports_obj, require_obj, module_obj, filename_obj, dirname_obj, };

            if (tagValue == ScriptRuntime.BYTECODE_COMMONJS_MODULE_TAG)
            {
                // bytecode
                fixed (byte* intput_ptr = source)
                {
                    var bytecodeFunc = JSApi.JS_ReadObject(ctx, intput_ptr + sizeof(uint), source.Length - sizeof(uint), JSApi.JS_READ_OBJ_BYTECODE);

                    if (bytecodeFunc.tag == JSApi.JS_TAG_FUNCTION_BYTECODE)
                    {
                        var func_val = JSApi.JS_EvalFunction(ctx, bytecodeFunc); // it's CallFree (bytecodeFunc)
                        if (JSApi.JS_IsFunction(ctx, func_val) != 1)
                        {
                            JSApi.JS_FreeValue(ctx, func_val);
                            JSApi.JS_FreeValue(ctx, require_argv);
                            return JSApi.JS_ThrowInternalError(ctx, "failed to require bytecode module");
                        }

                        var rval = JSApi.JS_Call(ctx, func_val, JSApi.JS_UNDEFINED, require_argv.Length, require_argv);
                        JSApi.JS_FreeValue(ctx, func_val);
                        if (rval.IsException())
                        {
                            JSApi.JS_FreeValue(ctx, require_argv);
                            return rval;
                        }
                        // success
                        JSApi.JS_FreeValue(ctx, rval);
                    }
                    else
                    {
                        JSApi.JS_FreeValue(ctx, bytecodeFunc);
                        JSApi.JS_FreeValue(ctx, require_argv);
                        return JSApi.JS_ThrowInternalError(ctx, "failed to require bytecode module");
                    }
                }
            }
            else
            {
                // source
                var input_bytes = TextUtils.GetShebangNullTerminatedCommonJSBytes(source);
                fixed (byte* input_ptr = input_bytes)
                fixed (byte* resolved_id_ptr = resolved_id_bytes)
                {
                    var input_len = (size_t)(input_bytes.Length - 1);
                    var func_val = JSApi.JS_Eval(ctx, input_ptr, input_len, resolved_id_ptr, JSEvalFlags.JS_EVAL_TYPE_GLOBAL | JSEvalFlags.JS_EVAL_FLAG_STRICT);
                    if (func_val.IsException())
                    {
                        JSApi.JS_FreeValue(ctx, require_argv);
                        return func_val;
                    }

                    if (JSApi.JS_IsFunction(ctx, func_val) == 1)
                    {
                        var rval = JSApi.JS_Call(ctx, func_val, JSApi.JS_UNDEFINED, require_argv.Length, require_argv);
                        if (rval.IsException())
                        {
                            JSApi.JS_FreeValue(ctx, func_val);
                            JSApi.JS_FreeValue(ctx, require_argv);
                            return rval;
                        }
                        JSApi.JS_FreeValue(ctx, rval);
                    }

                    JSApi.JS_FreeValue(ctx, func_val);
                }
            }

            JSApi.JS_SetProperty(ctx, module_obj, context.GetAtom("loaded"), JSApi.JS_NewBool(ctx, true));
            var exports_ = JSApi.JS_GetProperty(ctx, module_obj, context.GetAtom("exports"));
            JSApi.JS_FreeValue(ctx, require_argv);
            return exports_;
        }
    }

    public class JsonModuleResolver : PathBasedModuleResolver
    {
        public JsonModuleResolver()
        {
        }

        protected override bool OnValidating(string module_id)
        {
            // 必须指明后缀
            return module_id.EndsWith(".json") || module_id.EndsWith(".jsonc");
        }

        protected override bool OnResolvingFile(IFileSystem fileSystem, IPathResolver pathResolver, string fileName, out string searchPath, out string resolvedFileName)
        {
            if (pathResolver.ResolvePath(fileSystem, fileName, out searchPath, out resolvedFileName))
            {
                return true;
            }

            return false;
        }

        public override unsafe JSValue LoadModule(ScriptContext context, string resolved_id)
        {
            var fileSystem = context.GetRuntime().GetFileSystem();
            var resolved_id_bytes = Utils.TextUtils.GetNullTerminatedBytes(resolved_id);
            var dirname = PathUtils.GetDirectoryName(resolved_id);
            var source = fileSystem.ReadAllBytes(resolved_id);
            var ctx = (JSContext)context;

            if (source == null)
            {
                return JSApi.JS_ThrowInternalError(ctx, "require module load failed");
            }

            var input_bytes = TextUtils.GetNullTerminatedBytes(source);
            var input_bom = TextUtils.GetBomSize(source);

            fixed (byte* input_ptr = &input_bytes[input_bom])
            fixed (byte* filename_ptr = resolved_id_bytes)
            {
                var rval = JSApi.JS_ParseJSON(ctx, input_ptr, input_bytes.Length - 1 - input_bom, filename_ptr);
                if (rval.IsException())
                {
                    return rval;
                }

                var module_obj = context._new_commonjs_module(resolved_id, rval, true);
                JSApi.JS_FreeValue(ctx, module_obj);

                return rval;
            }
        }
    }

    public class StaticModuleResolver : IModuleResolver
    {
        public delegate void ModuleLoader(ScriptContext context, JSValue module_obj, JSValue exports_obj);

        private Dictionary<string, ModuleLoader> _loader = new Dictionary<string, ModuleLoader>();

        public StaticModuleResolver AddStaticModuleLoader(string module_id, Func<ScriptContext, JSValue> loader)
        {
            return AddStaticModuleLoader(module_id, (context, m, e) =>
            {
                var v = loader(context);
                JSApi.JS_SetPropertyStr(context, m, "exports", v);
            });
        }

        public StaticModuleResolver AddStaticModuleLoader(string module_id, ModuleLoader loader)
        {
            _loader.Add(module_id, loader);
            return this;
        }

        public StaticModuleResolver AddStaticModule(string module_id, ModuleRegister moduleRegister)
        {
            _loader.Add(module_id, loader);
            return this;
        }

        public bool ResolveModule(IFileSystem fileSystem, IPathResolver pathResolver, string parent_module_id, string module_id, out string resolved_id)
        {
            if (_loader.ContainsKey(module_id))
            {
                resolved_id = module_id;
                return true;
            }
            resolved_id = null;
            return false;
        }

        public JSValue LoadModule(ScriptContext context, string resolved_id)
        {
            ModuleLoader loader;
            if (_loader.TryGetValue(resolved_id, out loader))
            {
                var exports_obj = JSApi.JS_NewObject(context);
                var module_obj = context._new_commonjs_module(resolved_id, exports_obj, true);

                loader(context, module_obj, exports_obj);
                
                JSApi.JS_FreeValue(context, exports_obj);
                JSApi.JS_FreeValue(context, module_obj);
            }

            return JSApi.JS_ThrowInternalError(context, "invalid static module loader");
        }
    }
}
