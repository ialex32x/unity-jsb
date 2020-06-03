using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using QuickJS.Native;
using UnityEngine;

namespace QuickJS
{
    public class ScriptValue : IDisposable
    {
        protected ScriptContext _context;
        protected /*readonly*/ JSValue _jsValue;

        public ScriptValue(ScriptContext context, JSValue jsValue)
        {
            _context = context;
            _jsValue = jsValue;
            JSApi.JS_DupValue(context, jsValue);
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
                var other = (ScriptValue) obj;
                return other._jsValue.Equals(_jsValue);
            }

            return false;
        }
    }
}
