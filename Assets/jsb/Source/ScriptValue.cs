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
        }

        public static ScriptValue CreateObject(JSContext ctx)
        {
            var context = ScriptEngine.GetContext(ctx);
            var val = JSApi.JS_NewObject(ctx);
            var sv = new ScriptValue(context, val);
            JSApi.JS_FreeValue(ctx, val);
            return sv;
        }

        public void SetProperty(string key, JSValue value)
        {
            JSApi.JS_SetProperty(_context, _jsValue, _context.GetAtom(key), value);
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
                return other._jsValue.Equals(_jsValue);
            }

            return false;
        }
    }
}
