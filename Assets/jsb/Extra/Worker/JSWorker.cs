using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections;
using System.Collections.Generic;

namespace QuickJS.Extra
{
    using AOT;
    using QuickJS;
    using QuickJS.IO;
    using QuickJS.Utils;
    using QuickJS.Native;
    using QuickJS.Binding;
    using System.Threading;

    public class JSWorker : Values, IScriptRuntimeListener, IScriptFinalize
    {
        private Thread _thread;
        private ScriptContext _parent;
        private ScriptRuntime _worker;

        private JSWorker()
        {
        }

        public void OnJSFinalize()
        {
        }

        private void OnWorkerAfterDestroy(int id)
        {
        }

        public void OnBind(ScriptRuntime runtime, TypeRegister register)
        {
            QuickJS.Extra.WebSocket.Bind(register);
            QuickJS.Extra.XMLHttpRequest.Bind(register);
        }

        public void OnComplete(ScriptRuntime runtime)
        {
        }

        private void Start(JSContext ctx, JSValue value, string scriptPath)
        {
            var parent = ScriptEngine.GetRuntime(ctx);
            var runtime = parent.CreateWorker(this);

            if (runtime == null)
            {
                throw new NullReferenceException();
            }

            _parent = parent.GetContext(ctx);
            _worker = runtime;
            _worker.OnAfterDestroy += OnWorkerAfterDestroy;
            _worker.EvalMain(scriptPath);
            _thread = new Thread(new ThreadStart(Run));
            _thread.Priority = ThreadPriority.Lowest;
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void Run()
        {
            // onmessage = JSApi.JS_GetProperty(ctx, "onmessage");
            var tick = Environment.TickCount;

            while (_worker.isRunning)
            {
                Thread.Yield();

                var now = Environment.TickCount;
                var dt = now - tick;
                tick = now;

                _worker.Update(dt);
            }

            _worker.Destroy();
        }

        private static JSValue _js_worker_ctor(JSContext ctx, JSValue new_target, int argc, JSValue[] argv, int magic)
        {
            if (argc < 1 || !argv[0].IsString())
            {
                return JSApi.JS_ThrowInternalError(ctx, "invalid parameter");
            }
            var scriptPath = JSApi.GetString(ctx, argv[0]);
            var worker = new JSWorker();
            var val = NewBridgeClassObject(ctx, new_target, worker, magic);
            try
            {
                if (val.IsObject())
                {
                    worker.Start(ctx, val, scriptPath);
                }
            }
            catch (Exception e)
            {
                JSApi.JS_FreeValue(ctx, val);
                return JSApi.ThrowException(ctx, e);
            }
            return val;
        }

        private static JSValue _js_worker_postMessage(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            return JSApi.JS_UNDEFINED;
        }

        private static JSValue _js_worker_terminate(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            return JSApi.JS_UNDEFINED;
        }

        private static JSValue _js_worker_onmessage_get(JSContext ctx, JSValue this_obj)
        {
            return JSApi.JS_UNDEFINED;
        }

        private static JSValue _js_worker_onmessage_set(JSContext ctx, JSValue this_obj, JSValue new_value)
        {
            return JSApi.JS_UNDEFINED;
        }

        public static void Bind(TypeRegister register)
        {
            var ns = register.CreateNamespace();
            var cls = ns.CreateClass("Worker", typeof(JSWorker), _js_worker_ctor);
            cls.AddMethod(false, "postMessage", _js_worker_postMessage, 1);
            cls.AddProperty(false, "onmessage", _js_worker_onmessage_get, _js_worker_onmessage_set);
            cls.AddMethod(false, "terminate", _js_worker_terminate);
            cls.Close();
            ns.Close();
        }
    }
}
