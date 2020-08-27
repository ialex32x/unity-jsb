using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using QuickJS.Native;
using UnityEngine;

namespace QuickJS
{
    public class ScriptPromise
    {
        private ScriptContext _context;
        private JSValue _promise;
        private JSValue[] _resolving_funcs;

        public ScriptPromise(ScriptContext context)
        {
            _context = context;
            _resolving_funcs = new[] { JSApi.JS_UNDEFINED, JSApi.JS_UNDEFINED };
            _promise = JSApi.JS_NewPromiseCapability(_context, _resolving_funcs);
            _context.OnDestroy += OnDestroy;
        }

        public static implicit operator JSValue(ScriptPromise value)
        {
            return value._promise;
        }

        public JSValue GetValue()
        {
            return _context != null ? _promise : JSApi.JS_UNDEFINED;
        }

        public void Resolve(object value = null)
        {
            Invoke(0, value);
        }

        public void Reject(object value = null)
        {
            Invoke(1, value);
        }

        private unsafe void Invoke(int index, object value)
        {
            if (_context == null)
            {
                throw new NullReferenceException("already released");
            }
            var context = _context;
            var ctx = (JSContext)_context;
            var backVal = Binding.Values.js_push_var(ctx, value);
            if (backVal.IsException())
            {
                var ex = ctx.GetExceptionString();
                Release();
                throw new JSException(ex);
            }

            var argv = stackalloc[] { backVal };
            var rval = JSApi.JS_Call(ctx, _resolving_funcs[index], JSApi.JS_UNDEFINED, 1, argv);
            JSApi.JS_FreeValue(ctx, backVal);
            if (rval.IsException())
            {
                var ex = ctx.GetExceptionString();
                Release();
                throw new JSException(ex);
            }

            JSApi.JS_FreeValue(ctx, rval);
            Release();
            context.GetRuntime().ExecutePendingJob();
        }

        private void OnDestroy(ScriptContext context)
        {
            Release();
        }

        public void Release()
        {
            if (_context != null)
            {
                var context = _context;
                _context = null;
                context.OnDestroy -= OnDestroy;
                var len = _resolving_funcs.Length;
                for (var i = 0; i < len; i++)
                {
                    JSApi.JS_FreeValue(context, _resolving_funcs[i]);
                    _resolving_funcs[i] = JSApi.JS_UNDEFINED;
                }
                JSApi.JS_FreeValue(context, _promise);
                _promise = JSApi.JS_UNDEFINED;
            }
        }
    }
}
