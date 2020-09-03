using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using QuickJS.Native;
using UnityEngine;

namespace QuickJS
{
    public abstract class ScriptPromise : IDisposable
    {
        private ScriptContext _context;
        private JSValue _promise;
        private JSValue[] _resolving_funcs;

        public ScriptPromise(JSContext ctx)
        : this(ScriptEngine.GetContext(ctx))
        {
        }

        public ScriptPromise(ScriptContext context)
        {
            _context = context;
            _resolving_funcs = new[] { JSApi.JS_UNDEFINED, JSApi.JS_UNDEFINED };
            _promise = JSApi.JS_NewPromiseCapability(_context, _resolving_funcs);
            _context.GetObjectCache().AddScriptPromise(_promise, this);
        }

        ~ScriptPromise()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool bManaged)
        {
            if (_context != null)
            {
                var context = _context;
                _context = null;
                context.FreeValues(_resolving_funcs);
                _resolving_funcs = null;
                context.GetRuntime().FreeScriptPromise(_promise);
                _promise = JSApi.JS_UNDEFINED;
            }
        }

        public static implicit operator JSValue(ScriptPromise value)
        {
            return value.GetPromiseValue();
        }

        public JSValue GetPromiseValue()
        {
            return _context != null ? _promise : JSApi.JS_UNDEFINED;
        }

        public void Reject(object value = null)
        {
            Invoke(1, value);
        }

        /// <summary>
        /// 完成此 Promise
        /// </summary>
        /// <param name="index">0 表示成功, 1 表示失败</param>
        /// <param name="value">传参给回调</param>
        protected unsafe void Invoke(int index, object value)
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
                Dispose();
                throw new JSException(ex);
            }

            var argv = stackalloc[] { backVal };
            var rval = JSApi.JS_Call(ctx, _resolving_funcs[index], JSApi.JS_UNDEFINED, 1, argv);
            JSApi.JS_FreeValue(ctx, backVal);
            if (rval.IsException())
            {
                var ex = ctx.GetExceptionString();
                Dispose();
                throw new JSException(ex);
            }

            JSApi.JS_FreeValue(ctx, rval);
            Dispose();
            context.GetRuntime().ExecutePendingJob();
        }
    }

    public class TypedScriptPromise<TResult> : ScriptPromise
    {
        public TypedScriptPromise(JSContext ctx)
        : base(ctx)
        {
        }

        public TypedScriptPromise(ScriptContext context)
        : base(context)
        {
        }

        public void Resolve(TResult value)
        {
            Invoke(0, value);
        }
    }

    public class AnyScriptPromise : ScriptPromise
    {
        public AnyScriptPromise(JSContext ctx)
        : base(ctx)
        {
        }

        public AnyScriptPromise(ScriptContext context)
        : base(context)
        {
        }

        public void Resolve(object value = null)
        {
            Invoke(0, value);
        }
    }
}
