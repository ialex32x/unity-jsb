using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using UnityEngine;

namespace QuickJS.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct JSContext
    {
        public static readonly JSContext Null;
        
        private unsafe void* _ctx;

        public void print_exception()
        {
            var ex = JSApi.JS_GetException(this);
            var err_fileName = JSApi.JS_GetProperty(this, ex, JSApi.JS_ATOM_fileName);
            var err_lineNumber = JSApi.JS_GetProperty(this, ex, JSApi.JS_ATOM_lineNumber);
            var err_message = JSApi.JS_GetProperty(this, ex, JSApi.JS_ATOM_message);
            var err_stack = JSApi.JS_GetProperty(this, ex, JSApi.JS_ATOM_stack);

            var fileName = this.GetString(err_fileName);
            var lineNumber = this.GetString(err_lineNumber);
            var message = this.GetString(err_message);
            var stack = this.GetString(err_stack);

            Debug.LogErrorFormat("[JS] {0}:{1} {2}\n{3}", fileName, lineNumber, message, stack);
            
            JSApi.JS_FreeValue(this, err_fileName);
            JSApi.JS_FreeValue(this, err_lineNumber);
            JSApi.JS_FreeValue(this, err_message);
            JSApi.JS_FreeValue(this, err_stack);
            JSApi.JS_FreeValue(this, ex);
        }

        public unsafe string GetString(JSValue jsValue)
        {
            size_t len;
            var ptr = JSApi.JS_ToCStringLen(this, out len, jsValue);
            if (ptr != IntPtr.Zero)
            {
                try
                {
                    if (len > 0)
                    {
                        var str_ = Encoding.UTF8.GetString((byte*) ptr.ToPointer(), len);
                        return str_;
                    }
                }
                finally
                {
                    JSApi.JS_FreeCString(this, ptr);
                }
            }

            return null;
        }

        public void SetProperty(JSValue this_obj, string name, JSCFunction fn, int length = 0)
        {
            JSApi.JS_SetPropertyStr(this, this_obj, name, JSApi.JS_NewCFunction(this, fn, name, length));
        }

        public override unsafe int GetHashCode()
        {
            return (int) _ctx;
        }
    }
}
