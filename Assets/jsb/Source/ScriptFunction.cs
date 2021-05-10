using System;
using QuickJS.Native;

namespace QuickJS
{
    public class ScriptFunction : ScriptValue, Utils.Invokable
    {
        private JSValue _thisValue;
        private JSValue[] _args;

        public ScriptFunction(ScriptContext context, JSValue fnValue)
            : base(context, fnValue)
        {
            _thisValue = JSApi.JS_UNDEFINED;
        }

        public ScriptFunction(ScriptContext context, JSValue fnValue, JSValue thisValue)
            : base(context, fnValue)
        {
            _thisValue = thisValue;
            JSApi.JS_DupValue(_context, _thisValue);
        }

        public ScriptFunction(ScriptContext context, JSValue fnValue, JSValue thisValue, JSValue[] args)
            : base(context, fnValue)
        {
            JSContext ctx = context;
            _thisValue = thisValue;
            _args = args;
            JSApi.JS_DupValue(_context, _thisValue);
            for (int i = 0, count = _args.Length; i < count; i++)
            {
                JSApi.JS_DupValue(ctx, _args[i]);
            }
        }

        protected override void Dispose(bool bManaged)
        {
            if (_context != null)
            {
                var context = _context;

                base.Dispose(bManaged);
                context.FreeValue(_thisValue);
                _thisValue = JSApi.JS_UNDEFINED;
                if (_args != null)
                {
                    context.FreeValues(_args);
                    _args = null;
                }
            }
        }

        public unsafe void Invoke()
        {
            Invoke(typeof(void));
        }

        public unsafe T Invoke<T>()
        {
            return (T)Invoke(typeof(T));
        }

        private unsafe object Invoke(Type resultType)
        {
            if (_context == null)
            {
                return null;
            }
            JSContext ctx = _context;
            var argc = _args == null ? 0 : _args.Length;
            fixed (JSValue* ptr = _args)
            {
                var rVal = JSApi.JS_Call(ctx, _jsValue, _thisValue, argc, ptr);
                if (JSApi.JS_IsException(rVal))
                {
                    var ex = ctx.GetExceptionString();
                    throw new JSException(ex);
                }

                object resultObject = null;
                Binding.Values.js_get_var(ctx, rVal, resultType, out resultObject);
                JSApi.JS_FreeValue(ctx, rVal);
                return resultObject;
            }
        }

        public void Invoke(object arg1)
        {
            Invoke(typeof(void), arg1);
        }

        public T Invoke<T>(object arg1)
        {
            return (T)Invoke(typeof(T), arg1);
        }

        public unsafe object Invoke(Type resultType, object arg1)
        {
            if (_context == null)
            {
                return null;
            }
            var ctx = (JSContext)_context;
            var val = Binding.Values.js_push_var(ctx, arg1);
            var args = stackalloc[] { val };
            var rVal = _Invoke(1, args);
            if (JSApi.JS_IsException(rVal))
            {
                var ex = ctx.GetExceptionString();
                JSApi.JS_FreeValue(ctx, val);
                throw new JSException(ex);
            }
            object rObj = null;
            Binding.Values.js_get_var(ctx, rVal, resultType, out rObj);
            JSApi.JS_FreeValue(ctx, rVal);
            JSApi.JS_FreeValue(ctx, val);
            return rObj;
        }

        public void Invoke(params object[] parameters)
        {
            Invoke(typeof(void), parameters);
        }

        public T Invoke<T>(params object[] parameters)
        {
            return (T)Invoke(typeof(T), parameters);
        }

        public unsafe object Invoke(Type resultType, params object[] parameters)
        {
            if (_context == null)
            {
                return null;
            }
            var ctx = (JSContext)_context;
            var count = parameters.Length;
            var args = stackalloc JSValue[count];
            for (var i = 0; i < count; i++)
            {
                args[i] = Binding.Values.js_push_var(ctx, parameters[i]);
            }
            var rVal = _Invoke(count, args);
            if (JSApi.JS_IsException(rVal))
            {
                var ex = ctx.GetExceptionString();
                _context.FreeValues(count, args);
                throw new JSException(ex);
            }
            object rObj = null;
            Binding.Values.js_get_var(ctx, rVal, resultType, out rObj);
            JSApi.JS_FreeValue(ctx, rVal);
            _context.FreeValues(count, args);
            return rObj;
        }

        // unsafe primitive call, will not change ref count of jsvalue in argv
        public unsafe JSValue _Invoke(int argc, JSValue* argv)
        {
            if (_context == null)
            {
                return JSApi.JS_UNDEFINED;
            }
            JSContext ctx = _context;
            var rVal = JSApi.JS_Call(ctx, _jsValue, _thisValue, argc, argv);
            return rVal;
        }
    }
}