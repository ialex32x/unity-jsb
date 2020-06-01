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
        public static string EnsureExtension(string fileName)
        {
            return fileName != null && fileName.EndsWith(".js") ? fileName : fileName + ".js";
        }

        [MonoPInvokeCallback(typeof(JSModuleNormalizeFunc))]
        public static unsafe IntPtr module_normalize(JSContext ctx, string module_base_name, string module_name,
            IntPtr opaque)
        {
            var module_id = EnsureExtension(module_name);
            var parent_id = module_base_name;
            var resolve_to = module_id;
            Debug.LogFormat("module_normalize module_id:'{0}', parent_id:'{1}'", module_id, parent_id);

            if (module_id.StartsWith("./") || module_id.StartsWith("../") || module_id.Contains("/./") || module_id.Contains("/../"))
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
                    JSApi.JS_ThrowInternalError(ctx, string.Format("invalid module path (out of sourceRoot): {0}", module_id));
                    return IntPtr.Zero;
                }
            }

            if (resolve_to != null)
            {
                return JSApi.js_strndup(ctx, resolve_to);
            }
            JSApi.JS_ThrowInternalError(ctx, string.Format("cannot find module: {0}", module_id));
            return IntPtr.Zero;
        }

        [MonoPInvokeCallback(typeof(JSModuleLoaderFunc))]
        public static unsafe JSModuleDef module_loader(JSContext ctx, string module_name, IntPtr opaque)
        {
            Debug.LogFormat("module_loader: {0}", module_name);
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
