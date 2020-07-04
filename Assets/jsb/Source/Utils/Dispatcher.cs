using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using AOT;
using QuickJS.Native;

namespace QuickJS.Utils
{
    //TODO: 未完成, 待定
    public class Dispatcher
    {
        protected class Handler
        {
            private JSValue _fn;
            private JSValue _caller;
            private bool _once;

            public Handler(JSValue caller, JSValue fn, bool once)
            {
                _fn = fn;
                _caller = caller;
                _once = once;
            }

            public bool IsTarget(JSValue caller)
            {
                return _caller == caller;
            }

            public bool IsTarget(JSValue caller, JSValue fn)
            {
                return _caller == caller && _fn == fn;
            }

            public JSValue Invoke(JSContext ctx, JSValue[] argv)
            {
                var rval = JSApi.JS_Call(ctx, _fn, _caller, argv.Length, argv);
                return rval;
            }

            public void Dispose()
            {
                //TODO: free
            }
        }

        private List<Handler> _handlers = new List<Handler>();

        [JSCFunction]
        public void Add(JSContext ctx, JSContext this_obj, int argc, JSValue[] argv)
        {
            // new Handler()
        }

        [JSCFunction]
        public void Remove(JSContext ctx, JSContext this_obj, int argc, JSValue[] argv)
        {
        }

        [JSCFunction]
        public void Emit(JSContext ctx, JSContext this_obj, int argc, JSValue[] argv)
        {
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}
