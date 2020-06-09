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
            return fileName != null && fileName.EndsWith(".js") ? fileName : fileName + ".js";
        }

        public static string module_resolve(string module_base_name, string module_name)
        {
            var module_id = EnsureExtension(module_name);
            var parent_id = module_base_name;
            var resolve_to = module_id;
            // Debug.LogFormat("module_normalize module_id:'{0}', parent_id:'{1}'", module_id, parent_id);

            if (module_id.StartsWith("./") || module_id.StartsWith("../") || module_id.Contains("/./") ||
                module_id.Contains("/../"))
            {
                // 显式相对路径直接从 parent 模块路径拼接
                var parent_path = PathUtils.GetDirectoryName(parent_id);
                try
                {
                    resolve_to = PathUtils.ExtractPath(PathUtils.Combine(parent_path, module_id), '/');
                }
                catch
                {
                    // 不能提升到源代码目录外面
                    throw new Exception(string.Format("invalid module path (out of sourceRoot): {0}", module_id));
                }
            }

            if (resolve_to != null)
            {
                return resolve_to;
            }

            throw new Exception(string.Format("module not found: {0}", module_id));
        }

        public class CommonJSModule
        {
            public int index;
            public string module_id;
            public JSValue module;
        }

        private Dictionary<string, CommonJSModule> _modules = new Dictionary<string, CommonJSModule>();
        private List<string> _moduleIndeices = new List<string>();

        private CommonJSModule AddCommonJSModule(string module_id, JSValue module)
        {
            var mod = new CommonJSModule()
            {
                index = _moduleIndeices.Count,
                module_id = module_id,
                module = module,
            };
            _moduleIndeices.Add(module_id);
            _modules[module_id] = mod;
            return mod;
        }

        private CommonJSModule FindCommonJSModule(int index)
        {
            if (index < 0 || index >= _moduleIndeices.Count)
            {
                return null;
            }

            CommonJSModule module;
            var module_id = _moduleIndeices[index];
            if (_modules.TryGetValue(module_id, out module))
            {
                return module;
            }

            return null;
        }

        private CommonJSModule FindCommonJSModule(string module_id)
        {
            CommonJSModule module;
            if (_modules.TryGetValue(module_id, out module))
            {
                return module;
            }

            return null;
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
        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        public static unsafe JSValue module_require(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv,
            int magic)
        {
            if (argc < 1)
            {
                return JSApi.JS_ThrowInternalError(ctx, "require module id");
            }

            if (!argv[0].IsString())
            {
                return JSApi.JS_ThrowInternalError(ctx, "require module id (string)");
            }

            var context = ScriptEngine.GetContext(ctx);
            var runtime = context.GetRuntime();
            var parent_mod = runtime.FindCommonJSModule(magic);
            var parent_module_id = parent_mod != null ? parent_mod.module_id : "";
            var id = JSApi.GetString(ctx, argv[0]);

            try
            {
                var resolved_id = module_resolve(parent_module_id, id); // csharp exception
                var cache = runtime.FindCommonJSModule(resolved_id);
                if (cache != null)
                {
                    return JSApi.JS_GetProperty(ctx, cache.module, context.GetAtom("exports"));
                }

                var resolved_id_bytes = Utils.TextUtils.GetNullTerminatedBytes(resolved_id);
                var fileResolver = runtime._fileResolver;
                var source = fileResolver.ReadAllBytes(resolved_id);
                if (source == null)
                {
                    return JSApi.JS_ThrowInternalError(ctx, "require module load failed");
                }

                var input_bytes = GetShebangNullTerminatedBytes(source);

                var filename_obj = JSApi.JS_NewString(ctx, resolved_id);
                var module_obj = JSApi.JS_NewObject(ctx);
                var exports_obj = JSApi.JS_NewObject(ctx);
                JSApi.JS_SetProperty(ctx, module_obj, context.GetAtom("loaded"), JSApi.JS_NewBool(ctx, false));
                JSApi.JS_SetProperty(ctx, module_obj, context.GetAtom("exports"), JSApi.JS_DupValue(ctx, exports_obj));
                runtime.AddCommonJSModule(resolved_id, JSApi.JS_DupValue(ctx, module_obj));
                var require_obj = JSApi.JSB_NewCFunctionMagic(ctx, module_require, context.GetAtom("require"), 1,
                    JSCFunctionEnum.JS_CFUNC_generic_magic, parent_mod != null ? parent_mod.index : -1);
                var require_argv = new JSValue[5]
                {
                    exports_obj, require_obj, module_obj, filename_obj, JSApi.JS_UNDEFINED,
                };
                fixed (byte* input_ptr = input_bytes)
                fixed (byte* resolved_id_ptr = resolved_id_bytes)
                {
                    var input_len = (size_t) (input_bytes.Length - 1);
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
            catch (Exception exception)
            {
                return JSApi.JS_ThrowInternalError(ctx, exception.Message);
            }
        }

        [MonoPInvokeCallback(typeof(JSModuleNormalizeFunc))]
        public static unsafe IntPtr module_normalize(JSContext ctx, string module_base_name, string module_name,
            IntPtr opaque)
        {
            try
            {
                var resolve_to = module_resolve(module_base_name, module_name);
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
            var m = JSModuleDef.Null;
            if (File.Exists(module_name))
            {
                var runtime = ScriptEngine.GetRuntime(ctx);
                var fileResolver = runtime._fileResolver;
                var source = fileResolver.ReadAllBytes(module_name);
                var input_bytes = Utils.TextUtils.GetNullTerminatedBytes(source);
                var fn_bytes = Utils.TextUtils.GetNullTerminatedBytes(module_name);

                fixed (byte* input_ptr = input_bytes)
                fixed (byte* fn_ptr = fn_bytes)
                {
                    var input_len = (size_t) (input_bytes.Length - 1);
                    var func_val = JSApi.JS_Eval(ctx, input_ptr, input_len, fn_ptr,
                        JSEvalFlags.JS_EVAL_TYPE_MODULE | JSEvalFlags.JS_EVAL_FLAG_COMPILE_ONLY);
                    if (JSApi.JS_IsException(func_val))
                    {
                        ctx.print_exception();
                        JSApi.JS_ThrowReferenceError(ctx, "Module Error");
                    }
                    else
                    {
                        m = new JSModuleDef(func_val.u.ptr);
                        var meta = JSApi.JS_GetImportMeta(ctx, m);
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
                JSApi.JS_ThrowReferenceError(ctx, "File Not Found");
            }

            return m;
        }
    }
}