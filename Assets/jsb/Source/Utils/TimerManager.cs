using System;
using System.Collections.Generic;
using AOT;
using QuickJS.Binding;
using QuickJS.Native;

namespace QuickJS.Utils
{
    using UnityEngine;

    public class TimerManager : Scheduler, IBinding
    {
        private uint _idgen;
        private Dictionary<uint, ulong> _timers = new Dictionary<uint, ulong>();

        public uint SetTimeout(ScriptFunction fn, int ms)
        {
            return CreateTimer(fn, ms, true);
        }

        public uint SetTimeout(Action fn, int ms)
        {
            return CreateTimer(fn, ms, true);
        }

        public uint SetInterval(ScriptFunction fn, int ms)
        {
            return CreateTimer(fn, ms, false);
        }

        public uint SetInterval(Action fn, int ms)
        {
            return CreateTimer(fn, ms, false);
        }

        private uint CreateTimer(Action fn, int ms, bool once)
        {
            var id = ++_idgen;
            var timer = this.Add(ms, once, new InvokableAction(fn));
            _timers.Add(id, timer);
            return id;
        }

        private uint CreateTimer(ScriptFunction fn, int ms, bool once)
        {
            var id = ++_idgen;
            var timer = this.Add(ms, once, fn);
            _timers.Add(id, timer);
            return id;
        }

        public bool ClearTimer(uint id)
        {
            ulong timer;
            if (_timers.TryGetValue(id, out timer))
            {
                _timers.Remove(id);
                this.Remove(timer);
                return true;
            }

            return false;
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue js_clear_timer(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc >= 1)
            {
                uint pres = JSApi.JSB_ToUint32z(ctx, argv[0]);
                if (pres > 0)
                {
                    var rt = ScriptEngine.GetRuntime(ctx);
                    var tm = rt.GetTimerManager();
                    tm.ClearTimer(pres);
                }
            }
            return JSApi.JS_UNDEFINED;
        }
        
        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue js_set_interval(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc >= 1)
            {
                var fnValue = argv[0];
                var fnArgs = JSApi.EmptyValues;
                if (JSApi.JS_IsFunction(ctx, fnValue) == 0)
                {
                    return JSApi.JS_ThrowTypeError(ctx, nameof(js_set_interval) + ": func");
                }
                int pres = 0;
                if (argc >= 2)
                {
                    if (JSApi.JS_ToInt32(ctx, out pres, argv[1]) < 0)
                    {
                        return JSApi.JS_ThrowTypeError(ctx, nameof(js_set_interval) + ": millisecs");
                    }

                    if (argc >= 3)
                    {
                        fnArgs = new JSValue[argc - 2];
                        for (var i = 2; i < argc; i++)
                        {
                            fnArgs[i - 2] = argv[i];
                        }
                    }
                }

                var context = ScriptEngine.GetContext(ctx);
                var func = new ScriptFunction(context, fnValue, this_obj, fnArgs);
                return JSApi.JS_NewUint32(ctx, context.GetTimerManager().CreateTimer(func, pres, false));
            }
            return JSApi.JS_UNDEFINED;
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue js_set_timeout(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc >= 1)
            {
                var fnValue = argv[0];
                var fnArgs = JSApi.EmptyValues;
                if (JSApi.JS_IsFunction(ctx, fnValue) == 0)
                {
                    return JSApi.JS_ThrowTypeError(ctx, nameof(js_set_timeout) + ": func");
                }
                int pres = 0;
                if (argc >= 2)
                {
                    if (JSApi.JS_ToInt32(ctx, out pres, argv[1]) < 0)
                    {
                        return JSApi.JS_ThrowTypeError(ctx, nameof(js_set_timeout) + ": millisecs");
                    }

                    if (argc >= 3)
                    {
                        fnArgs = new JSValue[argc - 2];
                        for (var i = 2; i < argc; i++)
                        {
                            fnArgs[i - 2] = argv[i];
                        }
                    }
                }

                var context = ScriptEngine.GetContext(ctx);
                var func = new ScriptFunction(context, fnValue, this_obj, fnArgs);
                return JSApi.JS_NewUint32(ctx, context.GetTimerManager().CreateTimer(func, pres, true));
            }
            return JSApi.JS_UNDEFINED;
        }

        public void Bind(TypeRegister register)
        {
            var ns = register.CreateNamespace();
            
            ns.AddFunction("setInterval", js_set_interval, 3);
            ns.AddFunction("setTimeout", js_set_timeout, 3);
            ns.AddFunction("clearInterval", js_clear_timer, 1);
            ns.AddFunction("clearTimeout", js_clear_timer, 1);
            
            ns.Close();
        }
    }
}