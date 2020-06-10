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
        private static byte[] _header;
        private static byte[] _footer;

        static ScriptRuntime()
        {
            _header = Encoding.UTF8.GetBytes("(function(exports,require,module,__filename,__dirname){");
            _footer = Encoding.UTF8.GetBytes("\n})");
        }

        public static string EnsureExtension(string fileName)
        {
            return fileName != null && (fileName.EndsWith(".js") || fileName.EndsWith(".jsx") || fileName.EndsWith(".json")) ? fileName : fileName + ".js";
        }

        public static string module_resolve(IFileSystem fs, IFileResolver resolver, string module_base_name, string module_id)
        {
            // var module_id = EnsureExtension(module_name);
            var parent_id = module_base_name;
            var resolving = module_id;
            // Debug.LogFormat("module_normalize module_id:'{0}', parent_id:'{1}'", module_id, parent_id);

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

        public static byte[] GetShebangNullTerminatedBytes(byte[] str)
        {
            var count = str.Length;

            if (str[count - 1] == 0)
            {
                count--;
            }

            var header_size = _header.Length;
            var footer_size = _footer.Length;
            var bom_size = 0;
            if (count >= 3)
            {
                // utf8 with bom
                if (str[0] == 239 && str[1] == 187 && str[2] == 191)
                {
                    bom_size = 3;
                }
            }

            var bytes = new byte[header_size + count + footer_size + 1 - bom_size];
            Array.Copy(_header, 0, bytes, 0, header_size);
            Array.Copy(str, bom_size, bytes, header_size, count - bom_size);

            if (count >= 2)
            {
                // skip shebang line (replace #! => //)
                if (str[0] == 35 && str[1] == 33)
                {
                    bytes[header_size] = 47;
                    bytes[header_size + 1] = 47;
                }
                else
                {
                    if (bom_size > 0)
                    {
                        if (count > bom_size + 1)
                        {
                            if (str[bom_size] == 35 && str[bom_size + 1] == 33)
                            {
                                bytes[header_size] = 47;
                                bytes[header_size + 1] = 47;
                            }
                        }
                    }
                }
            }

            Array.Copy(_footer, 0, bytes, header_size + count - bom_size, footer_size);
            return bytes;
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

                        var module_obj = JSApi.JS_NewObject(ctx);

                        JSApi.JS_SetProperty(ctx, module_obj, context.GetAtom("loaded"), JSApi.JS_NewBool(ctx, true));
                        JSApi.JS_SetProperty(ctx, module_obj, context.GetAtom("exports"), JSApi.JS_DupValue(ctx, rval));
                        context._new_commonjs_module(resolved_id, module_obj);

                        return rval;
                    }
                }
                else
                {
                    var input_bytes = GetShebangNullTerminatedBytes(source);
                    var module_id_atom = context.GetAtom(resolved_id);
                    var filename_obj = JSApi.JS_AtomToString(ctx, module_id_atom);
                    var module_obj = JSApi.JS_NewObject(ctx);
                    var exports_obj = JSApi.JS_NewObject(ctx);

                    JSApi.JS_SetProperty(ctx, module_obj, context.GetAtom("loaded"), JSApi.JS_NewBool(ctx, false));
                    JSApi.JS_SetProperty(ctx, module_obj, context.GetAtom("exports"), JSApi.JS_DupValue(ctx, exports_obj));
                    context._new_commonjs_module(resolved_id, JSApi.JS_DupValue(ctx, module_obj));
                    var require_obj = JSApi.JSB_NewCFunction(ctx, module_require, context.GetAtom("require"), 1,
                        JSCFunctionEnum.JS_CFUNC_generic, 0);
                    JSApi.JS_SetProperty(ctx, require_obj, context.GetAtom("moduleId"), JSApi.JS_DupValue(ctx, filename_obj));
                    var require_argv = new JSValue[5]
                    {
                        exports_obj, require_obj, module_obj, filename_obj, JSApi.JS_UNDEFINED,
                    };
                    fixed (byte* input_ptr = input_bytes)
                    fixed (byte* resolved_id_ptr = resolved_id_bytes)
                    {
                        var input_len = (size_t)(input_bytes.Length - 1);
                        var func_val = JSApi.JS_Eval(ctx, input_ptr, input_len, resolved_id_ptr,
                            JSEvalFlags.JS_EVAL_TYPE_GLOBAL | JSEvalFlags.JS_EVAL_FLAG_STRICT);
                        if (func_val.IsException())
                        {
                            JSApi.JS_FreeValue(ctx, exports_obj);
                            JSApi.JS_FreeValue(ctx, require_obj);
                            JSApi.JS_FreeValue(ctx, module_obj);
                            JSApi.JS_FreeValue(ctx, filename_obj);
                            return func_val;
                        }

                        if (JSApi.JS_IsFunction(ctx, func_val) == 1)
                        {
                            var rval = JSApi.JS_Call(ctx, func_val, JSApi.JS_UNDEFINED, require_argv.Length, require_argv);
                            if (rval.IsException())
                            {
                                JSApi.JS_FreeValue(ctx, exports_obj);
                                JSApi.JS_FreeValue(ctx, require_obj);
                                JSApi.JS_FreeValue(ctx, module_obj);
                                JSApi.JS_FreeValue(ctx, filename_obj);
                                return func_val;
                            }
                        }

                        JSApi.JS_FreeValue(ctx, func_val);
                    }

                    JSApi.JS_SetProperty(ctx, module_obj, context.GetAtom("loaded"), JSApi.JS_NewBool(ctx, true));
                    JSApi.JS_FreeValue(ctx, require_obj);
                    JSApi.JS_FreeValue(ctx, module_obj);
                    JSApi.JS_FreeValue(ctx, filename_obj);

                    return exports_obj;
                }
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        [MonoPInvokeCallback(typeof(JSModuleNormalizeFunc))]
        public static unsafe IntPtr module_normalize(JSContext ctx, string module_base_name, string module_name,
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

        [MonoPInvokeCallback(typeof(JSModuleLoaderFunc))]
        public static unsafe JSModuleDef module_loader(JSContext ctx, string module_name, IntPtr opaque)
        {
            // Debug.LogFormat("module_loader: {0}", module_name);
            var mod = JSModuleDef.Null;
            var runtime = ScriptEngine.GetRuntime(ctx);
            var fileSystem = runtime._fileSystem;
            if (fileSystem.Exists(module_name))
            {
                var source = fileSystem.ReadAllBytes(module_name);
                var input_bytes = Utils.TextUtils.GetNullTerminatedBytes(source);
                var fn_bytes = Utils.TextUtils.GetNullTerminatedBytes(module_name);

                fixed (byte* input_ptr = input_bytes)
                fixed (byte* fn_ptr = fn_bytes)
                {
                    var input_len = (size_t)(input_bytes.Length - 1);
                    var func_val = JSApi.JS_Eval(ctx, input_ptr, input_len, fn_ptr,
                        JSEvalFlags.JS_EVAL_TYPE_MODULE | JSEvalFlags.JS_EVAL_FLAG_COMPILE_ONLY);
                    if (JSApi.JS_IsException(func_val))
                    {
                        ctx.print_exception();
                        JSApi.JS_ThrowReferenceError(ctx, "Module Error");
                    }
                    else
                    {
                        mod = new JSModuleDef(func_val.u.ptr);
                        var meta = JSApi.JS_GetImportMeta(ctx, mod);
                        JSApi.JS_DefinePropertyValueStr(ctx, meta, "url",
                            JSApi.JS_NewString(ctx, $"file://{module_name}"), JSPropFlags.JS_PROP_C_W_E);
                        JSApi.JS_DefinePropertyValueStr(ctx, meta, "main",
                            JSApi.JS_NewBool(ctx, false), JSPropFlags.JS_PROP_C_W_E);
                        JSApi.JS_FreeValue(ctx, meta);
                    }
                }
            }
            else
            {
                JSApi.JS_ThrowReferenceError(ctx, "module load failed: file not found");
            }

            return mod;
        }
    }
}