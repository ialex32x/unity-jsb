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

    public partial class ScriptRuntime
    {
        [MonoPInvokeCallback(typeof(JSModuleNormalizeFunc))]
        public static unsafe IntPtr module_normalize(JSContext ctx, string module_base_name, string module_name,
            IntPtr opaque)
        {
            Debug.LogFormat("module_name: {0} [module_base_name: {1}]", module_name, module_base_name);
            var bytes = Utils.TextUtils.GetNullTerminatedBytes(module_name);
            fixed (byte* msg = bytes)
            {
                return JSApi.js_strndup(ctx, msg, bytes.Length - 1);
            }
        }

        [MonoPInvokeCallback(typeof(JSModuleLoaderFunc))]
        public static unsafe JSModuleDef module_loader(JSContext ctx, string module_name, IntPtr opaque)
        {
            Debug.LogFormat("module_loader: {0}", module_name);
            var m = JSModuleDef.Null;
            if (File.Exists(module_name))
            {
                var source = File.ReadAllText(module_name);
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
