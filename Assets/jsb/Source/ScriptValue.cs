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
        private ScriptContext _context;
        private JSValue _value;

        public ScriptValue(ScriptContext context, JSValue value)
        {
            JSApi.JS_DupValue(context, value);
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
                var value = _value;

                _context = null;
                _value = JSApi.JS_UNDEFINED;
                context.FreeValue(value);
            }
        }
    }
}
