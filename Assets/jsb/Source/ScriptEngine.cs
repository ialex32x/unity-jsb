using System;
using System.IO;
using System.Text;
using AOT;
using QuickJS.Native;
using UnityEngine;

namespace QuickJS
{
    public class ScriptEngine
    {
        private static ScriptEngine _engine;

        private uint _class_id_alloc = JSApi.__JSB_GetClassID();
        private JSRuntime _rt;
        // private ScriptContext _main;

        public static ScriptEngine GetInstance()
        {
            return _engine;
        }

        public ScriptEngine()
        {
            _engine = this;
            _rt = JSApi.JS_NewRuntime();
            Init();
        }

        [MonoPInvokeCallback(typeof(JSModuleNormalizeFunc))]
        public static unsafe IntPtr module_normalize(JSContext ctx, string module_base_name, string module_name,
            IntPtr opaque)
        {
            Debug.LogFormat("module_name: {0} [module_base_name: {1}]", module_name, module_base_name);
            var bytes = Encoding.UTF8.GetBytes(module_name + "\0");
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
                var input_bytes = Encoding.UTF8.GetBytes(source + "\0");
                var input_len = (size_t) (input_bytes.Length - 1);
                var fn_bytes = Encoding.UTF8.GetBytes(module_name + "\0");
            
                fixed (byte* input_ptr = input_bytes)
                fixed (byte* fn_ptr = fn_bytes)
                {
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

        public void Init()
        {
            JSApi.JS_SetModuleLoaderFunc(_rt, module_normalize, module_loader, IntPtr.Zero);
            // JSApi.JS_SetModuleLoaderFunc(_rt, null, module_loader, IntPtr.Zero);
        }

        public JSClassID NewClassID()
        {
            return _class_id_alloc++;
        }

        public void FreeValue(JSValue value)
        {
            JSApi.JS_FreeValueRT(_rt, value);
        }

        public ScriptContext NewContext()
        {
            var ctx = JSApi.JS_NewContext(_rt);
            return new ScriptContext(ctx);
        }

        public static implicit operator JSRuntime(ScriptEngine se)
        {
            return se._rt;
        }
    }
}