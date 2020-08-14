using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AOT;
using QuickJS.Native;
using System.Threading;

namespace QuickJS
{
    using UnityEngine;
    using Utils;

    public partial class ScriptRuntime
    {
        public const uint BYTECODE_COMMONJS_MODULE_TAG = ScriptEngine.VERSION << 8 | 0x23;
        public const uint BYTECODE_ES6_MODULE_TAG = ScriptEngine.VERSION << 8 | 0xfe;

        private static uint TryReadByteCodeTagValue(byte[] bytes)
        {
            if (bytes != null && bytes.Length > sizeof(uint))
            {
                return BitConverter.ToUInt32(bytes, 0);
            }
            return 0;
        }

        public static string module_resolve(IFileSystem fs, IFileResolver resolver, string module_base_name, string module_id)
        {
            var parent_id = module_base_name;
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

            string resolved;
            if (resolver.ResolvePath(fs, resolving, out resolved))
            {
                return resolved;
            }

            throw new Exception(string.Format("module not found: {0}", module_id));
        }

        [MonoPInvokeCallback(typeof(JSModuleNormalizeFunc))]
        public static IntPtr module_normalize(JSContext ctx, string module_base_name, string module_name,
            IntPtr opaque)
        {
            try
            {
                var runtime = ScriptEngine.GetRuntime(ctx);
                var fileResolver = runtime._fileResolver;
                var fileSystem = runtime._fileSystem;
                var resolve_to = module_resolve(fileSystem, fileResolver, module_base_name, module_name);
                return JSApi.js_strndup(ctx, resolve_to);
            }
            catch (Exception exception)
            {
                JSApi.JS_ThrowInternalError(ctx, exception.Message);
                return IntPtr.Zero;
            }
        }

        // require(id);
        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static unsafe JSValue module_require(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc < 1)
            {
                return JSApi.JS_ThrowInternalError(ctx, "require module id");
            }

            if (!argv[0].IsString())
            {
                return JSApi.JS_ThrowInternalError(ctx, "require module id (string)");
            }

            var callee = JSApi.JS_GetActiveFunction(ctx);

            if (JSApi.JS_IsFunction(ctx, callee) != 1)
            {
                return JSApi.JS_ThrowInternalError(ctx, "require != function");
            }

            var context = ScriptEngine.GetContext(ctx);
            var parent_module_id_val = JSApi.JS_GetProperty(ctx, callee, context.GetAtom("moduleId"));
            var parent_module_id = JSApi.GetString(ctx, parent_module_id_val);
            JSApi.JS_FreeValue(ctx, parent_module_id_val);

            var runtime = context.GetRuntime();
            var id = JSApi.GetString(ctx, argv[0]);

            try
            {
                var fileSystem = runtime._fileSystem;
                var fileResolver = runtime._fileResolver;
                var resolved_id = module_resolve(fileSystem, fileResolver, parent_module_id, id); // csharp exception
                var cache = context._get_commonjs_module(resolved_id);
                if (cache.IsObject())
                {
                    var exports = JSApi.JS_GetProperty(ctx, cache, context.GetAtom("exports"));
                    JSApi.JS_FreeValue(ctx, cache);
                    return exports;
                }

                var resolved_id_bytes = Utils.TextUtils.GetNullTerminatedBytes(resolved_id);
                var dirname = PathUtils.GetDirectoryName(resolved_id);
                var source = fileSystem.ReadAllBytes(resolved_id);
                if (source == null)
                {
                    return JSApi.JS_ThrowInternalError(ctx, "require module load failed");
                }

                if (resolved_id.EndsWith(".json"))
                {
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
                else
                {
                    var tagValue = TryReadByteCodeTagValue(source);

                    if (tagValue == BYTECODE_ES6_MODULE_TAG)
                    {
                        return JSApi.JS_ThrowInternalError(ctx, "es6 module can not be loaded by require");
                    }

                    var module_id_atom = context.GetAtom(resolved_id);
                    var dirname_atom = context.GetAtom(dirname);
                    var exports_obj = JSApi.JS_NewObject(ctx);
                    var require_obj = JSApi.JSB_NewCFunction(ctx, module_require, context.GetAtom("require"), 1, JSCFunctionEnum.JS_CFUNC_generic, 0);
                    var module_obj = context._new_commonjs_module(resolved_id, exports_obj, false);
                    var filename_obj = JSApi.JS_AtomToString(ctx, module_id_atom);
                    var dirname_obj = JSApi.JS_AtomToString(ctx, dirname_atom);

                    JSApi.JS_SetProperty(ctx, require_obj, context.GetAtom("moduleId"), JSApi.JS_DupValue(ctx, filename_obj));
                    var require_argv = new JSValue[5] { exports_obj, require_obj, module_obj, filename_obj, dirname_obj, };

                    if (tagValue == BYTECODE_COMMONJS_MODULE_TAG)
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
                                    runtime.FreeValues(require_argv);
                                    return JSApi.JS_ThrowInternalError(ctx, "failed to require bytecode module");
                                }

                                var rval = JSApi.JS_Call(ctx, func_val, JSApi.JS_UNDEFINED, require_argv.Length, require_argv);
                                JSApi.JS_FreeValue(ctx, func_val);
                                if (rval.IsException())
                                {
                                    runtime.FreeValues(require_argv);
                                    return rval;
                                }
                                // success
                            }
                            else
                            {
                                JSApi.JS_FreeValue(ctx, bytecodeFunc);
                                runtime.FreeValues(require_argv);
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
                                runtime.FreeValues(require_argv);
                                return func_val;
                            }

                            if (JSApi.JS_IsFunction(ctx, func_val) == 1)
                            {
                                var rval = JSApi.JS_Call(ctx, func_val, JSApi.JS_UNDEFINED, require_argv.Length, require_argv);
                                if (rval.IsException())
                                {
                                    JSApi.JS_FreeValue(ctx, func_val);
                                    runtime.FreeValues(require_argv);
                                    return rval;
                                }
                            }

                            JSApi.JS_FreeValue(ctx, func_val);
                        }
                    }

                    JSApi.JS_SetProperty(ctx, module_obj, context.GetAtom("loaded"), JSApi.JS_NewBool(ctx, true));
                    JSApi.JS_DupValue(ctx, exports_obj);
                    runtime.FreeValues(require_argv);
                    return exports_obj;
                }
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        [MonoPInvokeCallback(typeof(JSModuleLoaderFunc))]
        public static unsafe JSModuleDef module_loader(JSContext ctx, string module_name, IntPtr opaque)
        {
            // Debug.LogFormat("module_loader: {0}", module_name);
            var runtime = ScriptEngine.GetRuntime(ctx);
            var fileSystem = runtime._fileSystem;
            if (!fileSystem.Exists(module_name))
            {
                JSApi.JS_ThrowReferenceError(ctx, "module not found");
                return JSModuleDef.Null;
            }

            var source = fileSystem.ReadAllBytes(module_name);
            var tagValue = TryReadByteCodeTagValue(source);

            if (tagValue == BYTECODE_COMMONJS_MODULE_TAG)
            {
                JSApi.JS_ThrowReferenceError(ctx, "commonjs module can not be loaded by import");
                return JSModuleDef.Null;
            }

            if (tagValue == BYTECODE_ES6_MODULE_TAG)
            {
                // bytecode
                fixed (byte* intput_ptr = source)
                {
                    var modObj = JSApi.JS_ReadObject(ctx, intput_ptr + sizeof(uint), source.Length - sizeof(uint), JSApi.JS_READ_OBJ_BYTECODE);
                    if (!modObj.IsModule())
                    {
                        JSApi.JS_FreeValue(ctx, modObj);
                        JSApi.JS_ThrowReferenceError(ctx, "unsupported module object");
                        return JSModuleDef.Null;
                    }

                    if (JSApi.JS_ResolveModule(ctx, modObj) < 0)
                    {
                        // fail
                        JSApi.JS_FreeValue(ctx, modObj);
                        JSApi.JS_ThrowReferenceError(ctx, "module resolve failed");
                        return JSModuleDef.Null;
                    }

                    return _NewModuleDef(ctx, modObj, module_name);
                }
            }

            // source 
            var input_bytes = TextUtils.GetNullTerminatedBytes(source);
            var fn_bytes = TextUtils.GetNullTerminatedBytes(module_name);

            fixed (byte* input_ptr = input_bytes)
            fixed (byte* fn_ptr = fn_bytes)
            {
                var input_len = (size_t)(input_bytes.Length - 1);
                var func_val = JSApi.JS_Eval(ctx, input_ptr, input_len, fn_ptr, JSEvalFlags.JS_EVAL_TYPE_MODULE | JSEvalFlags.JS_EVAL_FLAG_COMPILE_ONLY);

                if (JSApi.JS_IsException(func_val))
                {
                    ctx.print_exception();
                    JSApi.JS_ThrowReferenceError(ctx, "module error");
                    return JSModuleDef.Null;
                }

                if (func_val.IsNullish())
                {
                    JSApi.JS_ThrowReferenceError(ctx, "module is null");
                    return JSModuleDef.Null;
                }
                
                return _NewModuleDef(ctx, func_val, module_name);
            }
        }

        private static JSModuleDef _NewModuleDef(JSContext ctx, JSValue func_val, string module_name)
        {
            var mod = new JSModuleDef(func_val.u.ptr);
            var meta = JSApi.JS_GetImportMeta(ctx, mod);
            JSApi.JS_DefinePropertyValueStr(ctx, meta, "url", JSApi.JS_NewString(ctx, $"file://{module_name}"), JSPropFlags.JS_PROP_C_W_E);
            JSApi.JS_DefinePropertyValueStr(ctx, meta, "main", JSApi.JS_NewBool(ctx, false), JSPropFlags.JS_PROP_C_W_E);
            JSApi.JS_FreeValue(ctx, meta);
            return mod;
        }
    }
}