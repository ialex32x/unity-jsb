using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using QuickJS.Native;
using UnityEngine;

namespace QuickJS
{
    public class ScriptDelegate : ScriptValue
    {
        private List<Delegate> _matches = new List<Delegate>();

        public Delegate Match(Type delegateType)
        {
            for (int i = 0, count = _matches.Count; i < count; i++)
            {
                var d = _matches[i];
                if (d.GetType() == delegateType)
                {
                    return d;
                }
            }
            return null;
        }

        public void Add(Delegate d)
        {
            if (d == null)
            {
                throw new ArgumentNullException();
            }
            _matches.Add(d);
        }

        public ScriptDelegate(ScriptContext context, JSValue jsValue) : base(context, jsValue)
        {
        }

        public unsafe JSValue Invoke(JSContext ctx)
        {
            JSValue rval = JSApi.JS_Call(ctx, _jsValue, JSApi.JS_UNDEFINED, 0, (JSValue*)0);
            return rval;
        }

        public unsafe JSValue Invoke(JSContext ctx, int argc, JSValue[] argv)
        {
            fixed (JSValue* ptr = argv)
            {
                JSValue rval = JSApi.JS_Call(ctx, _jsValue, JSApi.JS_UNDEFINED, argc, ptr);
                return rval;
            }
        }

        public unsafe JSValue Invoke(JSContext ctx, int argc, JSValue* argv)
        {
            JSValue rval = JSApi.JS_Call(ctx, _jsValue, JSApi.JS_UNDEFINED, argc, argv);
            return rval;
        }

        public unsafe JSValue Invoke(JSContext ctx, JSValue this_obj)
        {
            JSValue rval = JSApi.JS_Call(ctx, _jsValue, this_obj, 0, (JSValue*)0);
            return rval;
        }

        public unsafe JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            fixed (JSValue* ptr = argv)
            {
                JSValue rval = JSApi.JS_Call(ctx, _jsValue, this_obj, argc, ptr);
                return rval;
            }
        }

        public unsafe JSValue Invoke(JSContext ctx, JSValue this_obj, int argc, JSValue* argv)
        {
            JSValue rval = JSApi.JS_Call(ctx, _jsValue, this_obj, argc, argv);
            return rval;
        }

        protected override void Dispose(bool bManaged)
        {
            if (_context != null)
            {
                var context = _context;

                _context = null;
                context.GetRuntime().FreeDelegationValue(_jsValue);
                _jsValue = JSApi.JS_UNDEFINED;
            }
        }
    }
}