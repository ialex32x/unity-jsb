using System;
using System.Collections.Generic;
using QuickJS.Binding;
using QuickJS.Native;

namespace QuickJS.Utils
{
    public class TimerManager : Scheduler
    {
        private uint _idgen;
        private Dictionary<uint, ulong> _timers = new Dictionary<uint, ulong>();

        public TimerManager(IScriptLogger logger, int jiffies = 10, int slots = 120, int depth = 4, int prealloc = 50, int capacity = 500)
        : base(logger, jiffies, slots, depth, prealloc, capacity)
        {
        }

        public uint SetTimeout(ScriptFunction fn, int ms)
        {
            return CreateTimer(fn, ms, true);
        }

        public uint SetInterval(ScriptFunction fn, int ms)
        {
            return CreateTimer(fn, ms, false);
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
                uint pres;
                if (JSApi.JSB_ToUint32(ctx, out pres, argv[0]) >= 0)
                {
                    var tm = ScriptEngine.GetTimerManager(ctx);
                    if (tm != null)
                    {
                        tm.ClearTimer(pres);
                    }
                }
            }
            return JSApi.JS_UNDEFINED;
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static unsafe JSValue js_set_immediate(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc >= 1)
            {
                var timerManager = ScriptEngine.GetTimerManager(ctx);
                if (timerManager == null)
                {
                    return ctx.ThrowTypeError("no bound TimerManager");
                }

                ScriptFunction func;
                if (!Values.js_get_classvalue(ctx, argv[0], out func) || func == null)
                {
                    return ctx.ThrowTypeError("the first arg is not a function");
                }

                func.SetBound(this_obj);
                func.SetArguments(1, argc - 1, argv);
                var timer = timerManager.CreateTimer(func, 0, false);
                return JSApi.JS_NewUint32(ctx, timer);
            }
            return JSApi.JS_UNDEFINED;
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue js_set_interval(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc >= 1)
            {
                int pres = 0;
                if (argc >= 2)
                {
                    if (JSApi.JS_ToInt32(ctx, out pres, argv[1]) < 0)
                    {
                        return ctx.ThrowTypeError("the given interval is not a number");
                    }
                }

                var timerManager = ScriptEngine.GetTimerManager(ctx);
                if (timerManager == null)
                {
                    return ctx.ThrowTypeError("no bound TimerManager");
                }

                ScriptFunction func;
                if (!Values.js_get_classvalue(ctx, argv[0], out func) || func == null)
                {
                    return ctx.ThrowTypeError("the first arg is not a function");
                }

                func.SetBound(this_obj);
                func.SetArguments(2, argc - 2, argv);
                var timer = timerManager.CreateTimer(func, pres, false);
                return JSApi.JS_NewUint32(ctx, timer);
            }
            return JSApi.JS_UNDEFINED;
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue js_set_timeout(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc >= 1)
            {
                int pres = 0;
                if (argc >= 2)
                {
                    if (JSApi.JS_ToInt32(ctx, out pres, argv[1]) < 0)
                    {
                        return ctx.ThrowTypeError("the given interval is not a number");
                    }
                }

                var timerManager = ScriptEngine.GetTimerManager(ctx);
                if (timerManager == null)
                {
                    return ctx.ThrowTypeError("no bound TimerManager");
                }

                ScriptFunction func;
                if (!Values.js_get_classvalue(ctx, argv[0], out func) || func == null)
                {
                    return ctx.ThrowTypeError("the first arg is not a function");
                }

                func.SetBound(this_obj);
                func.SetArguments(2, argc - 2, argv);
                var timer = timerManager.CreateTimer(func, pres, true);
                return JSApi.JS_NewUint32(ctx, timer);
            }
            return JSApi.JS_UNDEFINED;
        }

        public static void Bind(TypeRegister register)
        {
            var context = register.GetContext();

            context.AddFunction("setImmediate", js_set_immediate, 2);
            context.AddFunction("setInterval", js_set_interval, 3);
            context.AddFunction("setTimeout", js_set_timeout, 3);
            context.AddFunction("clearImmediate", js_clear_timer, 1);
            context.AddFunction("clearInterval", js_clear_timer, 1);
            context.AddFunction("clearTimeout", js_clear_timer, 1);
        }
    }
}