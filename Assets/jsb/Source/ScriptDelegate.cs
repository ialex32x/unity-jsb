using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using QuickJS.Native;
using UnityEngine;

namespace QuickJS
{
    public class ScriptDelegate : ScriptValue
    {
        public Delegate target;

        public ScriptDelegate(ScriptContext context, JSValue jsValue) : base(context, jsValue)
        {
        }

        protected override void Dispose(bool bManaged)
        {
            if (_context != null)
            {
                var context = _context;

                _context = null;
                context.GetRuntime().FreeDelegationValue(_jsValue);
            }
        }
    }
}