using QuickJS.Native;
using UnityEngine;

namespace QuickJS
{
    public class ScriptFunction : ScriptValue, Utils.Invokable
    {
        private JSValue _thisValue;
        private JSValue[] _args;
        
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

                _context = null;
                context.FreeValue(_jsValue);
                context.FreeValue(_thisValue);
                context.FreeValues(_args);
            }
        }

        public void Invoke()
        {
            JSContext ctx = _context;
            if (_args == null)
            {
                var rVal = JSApi.JS_Call(ctx, _jsValue, _thisValue, 0, JSApi.EmptyValues);
                if (JSApi.JS_IsException(rVal))
                {
                    ctx.print_exception();
                }
            
                JSApi.JS_FreeValue(ctx, rVal);
            }
            else
            {
                var rVal = JSApi.JS_Call(ctx, _jsValue, _thisValue, _args.Length, _args);
                if (JSApi.JS_IsException(rVal))
                {
                    ctx.print_exception();
                }
            
                JSApi.JS_FreeValue(ctx, rVal);
            }
        }
    }
}