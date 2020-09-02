using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using QuickJS.Native;
using UnityEngine;

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
            _context.OnDestroy += OnDestroy;
            _context.GetObjectCache().AddScriptValue(_jsValue, this);
        }

        private void OnDestroy(ScriptContext context)
        {
            Dispose();
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
                context.OnDestroy -= OnDestroy;
                context.FreeValue(_jsValue);
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
            JSApi.JS_SetPropertyStr(_context, _jsValue, key, jsValue);
        }
    }
}
