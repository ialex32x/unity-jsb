using QuickJS.Native;
using UnityEngine;

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

                _context = null;
                context.FreeValue(_jsValue);
                context.FreeValue(_thisValue);
                if (_args != null)
                {
                    context.FreeValues(_args);
                }
            }
        }

        public unsafe void Invoke()
        {
            if (_context == null)
            {
                return;
            }
            JSContext ctx = _context;
            var argc = _args == null ? 0 : _args.Length;
            fixed (JSValue* ptr = _args)
            {
                var rVal = JSApi.JS_Call(ctx, _jsValue, _thisValue, argc, ptr);
                if (JSApi.JS_IsException(rVal))
                {
                    ctx.print_exception();
                }

                JSApi.JS_FreeValue(ctx, rVal);
            }
        }

        public void Invoke(object arg1)
        {
            if (_context == null)
            {
                return;
            }
            var ctx = (JSContext) _context;
            var val = Binding.Values.js_push_classvalue(ctx, arg1);
            Invoke(new[] {val});
        }

        public void Invoke(JSValue[] argv)
        {
            if (_context == null)
            {
                return;
            }
            JSContext ctx = _context;
            var rVal = JSApi.JS_Call(ctx, _jsValue, _thisValue, argv.Length, argv);
            if (JSApi.JS_IsException(rVal))
            {
                ctx.print_exception();
            }

            JSApi.JS_FreeValue(ctx, rVal);
        }
    }
}