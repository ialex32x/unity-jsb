using System;
using QuickJS.Native;

namespace QuickJS
{
    public class ScriptValue : IDisposable
    {
        protected ScriptContext _context;
        protected /*readonly*/ JSValue _jsValue;

        public JSContext ctx
        {
            get { return _context; }
        }

        public ScriptValue(ScriptContext context, JSValue jsValue)
        {
            _context = context;
            _jsValue = jsValue;
            JSApi.JS_DupValue(context, jsValue);
            _context.GetObjectCache().AddScriptValue(_jsValue, this);
        }

        public static implicit operator JSValue(ScriptValue value)
        {
            return value._jsValue;
        }

        ~ScriptValue()
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
                context.GetRuntime().FreeScriptValue(_jsValue);
                _jsValue = JSApi.JS_UNDEFINED;
            }
        }

        public override int GetHashCode()
        {
            return _jsValue.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ScriptValue)
            {
                var other = (ScriptValue)obj;
                return other._jsValue == _jsValue;
            }

            if (obj is JSValue)
            {
                var other = (JSValue)obj;
                return other == _jsValue;
            }

            return false;
        }

        public T GetProperty<T>(string key)
        {
            var ctx = (JSContext)_context;
            var propVal = JSApi.JS_GetPropertyStr(ctx, _jsValue, key);
            if (propVal.IsException())
            {
                var ex = ctx.GetExceptionString();
                throw new JSException(ex);
            }

            object o;
            if (Binding.Values.js_get_var(ctx, propVal, typeof(T), out o))
            {
                JSApi.JS_FreeValue(ctx, propVal);
                return (T)o;
            }
            JSApi.JS_FreeValue(ctx, propVal);
            throw new JSException("invalid cast");
        }

        public void SetProperty(string key, object value)
        {
            var ctx = (JSContext)_context;
            var jsValue = Binding.Values.js_push_var(ctx, value);
            JSApi.JS_SetPropertyStr(ctx, _jsValue, key, jsValue);
        }

        public override string ToString()
        {
            if (_context == null)
            {
                return null;
            }
            return JSApi.GetString(_context, _jsValue);
        }

        public string JSONStringify()
        {
            if (_context == null)
            {
                return null;
            }
            var ctx = (JSContext)_context;
            var rval = JSApi.JS_JSONStringify(ctx, _jsValue, JSApi.JS_UNDEFINED, JSApi.JS_UNDEFINED);
            var str = JSApi.GetString(ctx, rval);
            JSApi.JS_FreeValue(ctx, rval);
            return str;
        }
    }
}
