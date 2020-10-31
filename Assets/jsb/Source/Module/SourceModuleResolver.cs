using System;
using System.Collections.Generic;

namespace QuickJS.Module
{
    using Utils;
    using Native;

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
}
