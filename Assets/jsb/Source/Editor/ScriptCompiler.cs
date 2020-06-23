using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QuickJS.Editor
{
    using Native;
    using UnityEngine;
    using UnityEditor;

    /// utility helps to compile js source into bytecode
    public class ScriptCompiler : IDisposable
    {
        private JSRuntime _rt;
        private JSContext _ctx;

        public ScriptCompiler()
        {
            _rt = JSApi.JS_NewRuntime();
            _ctx = JSApi.JS_NewContext(_rt);
            JSApi.JS_AddIntrinsicOperators(_ctx);
        }

        ~ScriptCompiler()
        {
            Dispose(false);
        }

        public byte[] Compile(string filename)
        {
            return Compile(filename, Utils.TextUtils.GetNullTerminatedBytes(File.ReadAllText(filename)));
        }

        public unsafe byte[] Compile(string filename, byte[] input_bytes)
        {
            byte[] outputBytes = null;
            try
            {
                var fn_bytes = Utils.TextUtils.GetNullTerminatedBytes(filename);
                fixed (byte* input_ptr = input_bytes)
                fixed (byte* fn_ptr = fn_bytes)
                {
                    var input_len = (size_t)(input_bytes.Length - 1);
                    var rval = JSApi.JS_Eval(_ctx, input_ptr, input_len, fn_ptr, JSEvalFlags.JS_EVAL_TYPE_MODULE | JSEvalFlags.JS_EVAL_FLAG_COMPILE_ONLY);
                    if (JSApi.JS_IsException(rval))
                    {
                        _ctx.print_exception();
                    }
                    else
                    {
                        size_t psize;
                        var byteCode = JSApi.JS_WriteObject(_ctx, out psize, rval, JSApi.JS_WRITE_OBJ_BYTECODE);
                        if (byteCode != IntPtr.Zero)
                        {
                            outputBytes = new byte[psize];
                            Marshal.Copy(byteCode, outputBytes, 0, psize);
                        }
                        JSApi.js_free(_ctx, byteCode);
                    }
                }
                return outputBytes;
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                return null;
            }
        }

        public virtual void Dispose(bool bManaged)
        {
            if (_ctx.IsValid())
            {
                JSApi.JS_FreeContext(_ctx);
                _ctx = JSContext.Null;
            }

            if (_rt.IsValid())
            {
                JSApi.JS_FreeRuntime(_rt);
                _rt = JSRuntime.Null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}