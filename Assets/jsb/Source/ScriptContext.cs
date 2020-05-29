using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using QuickJS.Native;
using UnityEngine;

namespace QuickJS
{
    public class ScriptContext
    {
        private JSContext _ctx;
        
        public ScriptContext(JSContext ctx)
        {
            _ctx = ctx;
        }

        public void AddIntrinsicOperators()
        {
            JSApi.JS_AddIntrinsicOperators(_ctx);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JSValue GetGlobalObject()
        {
            return JSApi.JS_GetGlobalObject(_ctx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JSValue NewObject()
        {
            return JSApi.JS_NewObject(_ctx);
        }

        #region Builtins

        public void print_exception()
        {
            _ctx.print_exception();
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        private static JSValue _print(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            int i;
            var sb = new StringBuilder();
            size_t len;

            for (i = 0; i < argc; i++)
            {
                if (i != 0)
                {
                    sb.Append(' ');
                }

                var pstr = JSApi.JS_ToCStringLen(ctx, out len, argv[i]);
                if (pstr == IntPtr.Zero)
                {
                    return JSApi.JS_EXCEPTION;
                }

                var str = JSApi.GetString(pstr, len);
                if (str != null)
                {
                    sb.Append(str);
                }

                JSApi.JS_FreeCString(ctx, pstr);
            }

            sb.AppendLine();
            Debug.Log(sb.ToString());
            return JSApi.JS_UNDEFINED;
        }

        #endregion

        public void RegisterBuiltins()
        {
            var global_object = JSApi.JS_GetGlobalObject(this);

            _ctx.SetProperty(global_object, "print", _print, 1);

            JSApi.JS_FreeValue(this, global_object);
        }

        public static implicit operator JSContext(ScriptContext sc)
        {
            return sc._ctx;
        }
    }
}