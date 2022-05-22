using System;
using QuickJS.Native;

namespace QuickJS
{
    public abstract class ScriptPromise : Utils.IWeakMapEntry
    {
        private ScriptContext _context;
        private JSValue _promise;
        private JSValue _on_resolve;
        private JSValue _on_reject;

        public JSValue promiseValue => _promise;
        public JSValue onResolveValue => _on_resolve;
        public JSValue onRejectValue => _on_reject;

        public ScriptPromise(JSContext ctx)
        : this(ScriptEngine.GetContext(ctx))
        {
        }

        public ScriptPromise(ScriptContext context)
        {
            _context = context;
            _promise = JSApi.JS_NewPromiseCapability(_context, out _on_resolve, out _on_reject);
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
            var context = _context;
            if (context != null)
            {
                _context = null;
                var on_resolve = _on_resolve;
                var on_reject = _on_reject;
                var promise = _promise;
                _on_resolve = JSApi.JS_UNDEFINED;
                _on_reject = JSApi.JS_UNDEFINED;
                _promise = JSApi.JS_UNDEFINED;

                context.GetRuntime().FreeScriptPromise(promise, on_resolve, on_reject);
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
            Invoke(_on_reject, value);
        }

        /// <summary>
        /// 完成此 Promise
        /// </summary>
        /// <param name="index">0 表示成功, 1 表示失败</param>
        /// <param name="value">传参给回调</param>
        protected unsafe void Invoke(JSValue callback, object value)
        {
            if (_context == null)
            {
                throw new NullReferenceException("already released");
            }
            var context = _context;
            var ctx = (JSContext)_context;
            if (JSApi.JS_IsFunction(ctx, callback) != 1)
            {
                Dispose();
                return;
            }

            var backVal = Binding.Values.js_push_var(ctx, value);
            if (backVal.IsException())
            {
                var ex = ctx.GetExceptionString();
                Dispose();
                throw new JSException(ex);
            }

            var argv = stackalloc[] { backVal };
            var rval = JSApi.JS_Call(ctx, callback, JSApi.JS_UNDEFINED, 1, argv);
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
            Invoke(onResolveValue, value);
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
            Invoke(onResolveValue, value);
        }
    }
}
