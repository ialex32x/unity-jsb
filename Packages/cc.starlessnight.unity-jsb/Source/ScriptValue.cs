using System;
using System.Threading;
using QuickJS.Native;

namespace QuickJS
{
    //TODO unity-jsb: make all WeakMapEntry types managed with a universal WeakMap instead of being separated
    /// <summary>
    /// ScriptValue holds a strong reference of js value, so it relies on C# object finalizer (or the runtime managed object cache) to release.
    /// </summary>
    public class ScriptValue : Utils.IWeakMapEntry
    {
        protected ScriptContext _context;
        protected /*readonly*/ JSValue _jsValue;

        public JSContext ctx => _context;

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

        protected virtual void OnDispose(ScriptContext context)
        {
        }

        protected virtual void Dispose(bool bManaged)
        {
            var context = _context;
            var jsValue = _jsValue;
            if (context != null)
            {
                _context = null;
                _jsValue = JSApi.JS_UNDEFINED;
                context.GetRuntime().FreeScriptValue(jsValue);
                OnDispose(context);
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
            var key_atom = _context.GetAtom(key);
            var jsValue = Binding.Values.js_push_var(ctx, value);
            JSApi.JS_SetProperty(ctx, _jsValue, key_atom, jsValue);
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
