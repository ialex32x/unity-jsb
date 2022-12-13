using System;
using System.Collections;
using System.Collections.Generic;
using QuickJS.Binding;
using QuickJS.Native;

namespace QuickJS.Utils
{
    public readonly struct TimerInfo
    {
        public readonly uint id;
        public readonly int delay;
        public readonly int deadline;
        public readonly bool once;

        public TimerInfo(uint id, int delay, int deadline, bool once)
        {
            this.id = id;
            this.delay = delay;
            this.deadline = deadline;
            this.once = once;
        }
    }

    public class DefaultTimerManager : ITimerManager
    {
        private uint _idgen;
        private TimerManager _manager = new TimerManager();
        private Dictionary<uint, SIndex> _timers = new Dictionary<uint, SIndex>();

        public DefaultTimerManager()
        {
        }

        public int now => (int)_manager.now;

        public void Update(int milliseconds)
        {
            _manager.Update((uint)milliseconds);
        }

        public void Destroy()
        {
            _manager.Clear();
            _timers.Clear();
        }

        public uint SetTimeout(ScriptFunction fn, int ms)
        {
            var id = ++_idgen;
            var index = SIndex.None;
            _manager.SetTimer(ref index, fn, (uint)ms, false);
            _timers.Add(id, index);
            return id;
        }

        public uint SetInterval(ScriptFunction fn, int ms)
        {
            var id = ++_idgen;
            var index = SIndex.None;
            _manager.SetTimer(ref index, fn, (uint)ms, true);
            _timers.Add(id, index);
            return id;
        }

        public bool ClearTimer(uint id)
        {
            if (_timers.TryGetValue(id, out var index))
            {
                _timers.Remove(id);
                _manager.ClearTimer(index);
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
                var timer = timerManager.SetInterval(func, 0);
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
                var timer = timerManager.SetInterval(func, pres);
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
                var timer = timerManager.SetTimeout(func, pres);
                return JSApi.JS_NewUint32(ctx, timer);
            }
            return JSApi.JS_UNDEFINED;
        }

        public void Bind(TypeRegister register)
        {
            var context = register.GetContext();

            context.AddGlobalFunction("setImmediate", js_set_immediate, 2);
            context.AddGlobalFunction("setInterval", js_set_interval, 3);
            context.AddGlobalFunction("setTimeout", js_set_timeout, 3);
            context.AddGlobalFunction("clearImmediate", js_clear_timer, 1);
            context.AddGlobalFunction("clearInterval", js_clear_timer, 1);
            context.AddGlobalFunction("clearTimeout", js_clear_timer, 1);
        }

        #region Enumeration Support
        IEnumerator<TimerInfo> IEnumerable<TimerInfo>.GetEnumerator() => (IEnumerator<TimerInfo>)this.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.GetEnumerator();
        public Enumerator GetEnumerator() => new Enumerator(this);
        public struct Enumerator : IEnumerator<TimerInfo>, IDisposable, IEnumerator
        {
            private DefaultTimerManager _self;
            private Dictionary<uint, SIndex>.Enumerator _e;

            public Enumerator(DefaultTimerManager self)
            {
                _self = self;
                _e = self._timers.GetEnumerator();
            }

            public TimerInfo Current
            {
                get
                {
                    var pair = _e.Current;
                    var data = _self._manager.GetTimerInfo(pair.Value);
                    return new TimerInfo(pair.Key, data.delay, data.deadline, data.once);
                }
            }

            public void Reset()
            {
                _e = _self._timers.GetEnumerator();
            }

            public bool MoveNext()
            {
                return _e.MoveNext();
            }

            public void Dispose() => _e.Dispose();

            void IDisposable.Dispose() => _e.Dispose();

            object IEnumerator.Current
            {
                get
                {
                    var pair = _e.Current;
                    var data = _self._manager.GetTimerInfo(pair.Value);
                    return (object)new TimerInfo(pair.Key, data.delay, data.deadline, data.once);
                }
            }
        }

        #endregion
    }
}