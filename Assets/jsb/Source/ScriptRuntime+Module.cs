using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using QuickJS.Native;
using System.Threading;

namespace QuickJS
{
    using Utils;

    public partial class ScriptRuntime
    {
        public const uint BYTECODE_COMMONJS_MODULE_TAG = ScriptEngine.VERSION << 8 | 0x23;
        public const uint BYTECODE_ES6_MODULE_TAG = ScriptEngine.VERSION << 8 | 0xfe;

        public static uint TryReadByteCodeTagValue(byte[] bytes)
        {
            if (bytes != null && bytes.Length > sizeof(uint))
            {
                return TextUtils.ToHostByteOrder(BitConverter.ToUInt32(bytes, 0));
            }
            return 0;
        }

        [MonoPInvokeCallback(typeof(JSModuleNormalizeFunc))]
        public static IntPtr module_normalize(JSContext ctx, string module_base_name, string module_name, IntPtr opaque)
        {
            try
            {
                var runtime = ScriptEngine.GetRuntime(ctx);
                var resolve_to = runtime.ResolveFilePath(module_base_name, module_name);
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

            // callee is the function <'require'> of current module
            var callee = JSApi.JS_GetActiveFunction(ctx);

            if (JSApi.JS_IsFunction(ctx, callee) != 1)
            {
                return JSApi.JS_ThrowInternalError(ctx, "require != function");
            }

            var context = ScriptEngine.GetContext(ctx);
            var runtime = context.GetRuntime();
            var parent_module_id_val = JSApi.JS_GetProperty(ctx, callee, context.GetAtom("moduleId"));
            var parent_module_id = JSApi.GetString(ctx, parent_module_id_val);
            JSApi.JS_FreeValue(ctx, parent_module_id_val);

            try
            {
                var module_id = JSApi.GetString(ctx, argv[0]);
                return runtime.ResolveModule(context, parent_module_id, module_id, false);
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        public static unsafe JSValue EvalSource(JSContext ctx, byte[] source, string fileName, bool bModule)
        {
            var tagValue = TryReadByteCodeTagValue(source);

            if (tagValue == BYTECODE_ES6_MODULE_TAG)
            {
                return JSApi.JS_ThrowInternalError(ctx, "eval does not support es6 module bytecode");
            }

            if (tagValue == BYTECODE_COMMONJS_MODULE_TAG)
            {
                fixed (byte* intput_ptr = source)
                {
                    var bytecodeFunc = JSApi.JS_ReadObject(ctx, intput_ptr + sizeof(uint), source.Length - sizeof(uint), JSApi.JS_READ_OBJ_BYTECODE);

                    if (bytecodeFunc.tag == JSApi.JS_TAG_FUNCTION_BYTECODE)
                    {
                        var func_val = JSApi.JS_EvalFunction(ctx, bytecodeFunc); // it's CallFree (bytecodeFunc)
                        if (JSApi.JS_IsFunction(ctx, func_val) != 1)
                        {
                            JSApi.JS_FreeValue(ctx, func_val);
                            return JSApi.JS_ThrowInternalError(ctx, "failed to eval bytecode module");
                        }

                        var rval = JSApi.JS_Call(ctx, func_val, JSApi.JS_UNDEFINED);
                        JSApi.JS_FreeValue(ctx, func_val);
                        return rval;
                    }

                    JSApi.JS_FreeValue(ctx, bytecodeFunc);
                    return JSApi.JS_ThrowInternalError(ctx, "failed to eval bytecode module");
                }
            }

            var input_bytes = Utils.TextUtils.GetNullTerminatedBytes(source);
            var fn_bytes = Utils.TextUtils.GetNullTerminatedBytes(fileName);

            fixed (byte* input_ptr = input_bytes)
            fixed (byte* fn_ptr = fn_bytes)
            {
                var input_len = (size_t)(input_bytes.Length - 1);
                var evalFlags = JSEvalFlags.JS_EVAL_FLAG_STRICT;

                if (bModule)
                {
                    evalFlags |= JSEvalFlags.JS_EVAL_TYPE_MODULE;
                }

                return JSApi.JS_Eval(ctx, input_ptr, input_len, fn_ptr, evalFlags);
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