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

    public class JSWorker : Values, IScriptFinalize
    {
        private Thread _thread;
        private ScriptContext _parent;
        private ScriptRuntime _worker;
        private Queue<IO.ByteBuffer> _messageQueue;

        private JSWorker()
        {
        }

        private void Release()
        {
            lock (_messageQueue)
            {
                while (true)
                {
                    var buf = _messageQueue.Dequeue();
                    if (buf == null)
                    {
                        break;
                    }
                    buf.Release();
                }
            }
        }

        public void OnJSFinalize()
        {
            Release();
        }

        private void OnWorkerAfterDestroy(int id)
        {
            Release();
        }

        private void Start(JSContext ctx, JSValue value, string scriptPath)
        {
            var parent = ScriptEngine.GetRuntime(ctx);
            var runtime = parent.CreateWorker();

            if (runtime == null)
            {
                throw new NullReferenceException();
            }

            _messageQueue = new Queue<ByteBuffer>();
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
            var tick = Environment.TickCount;
            var list = new List<IO.ByteBuffer>();
            var context = _worker.GetMainContext();
            var globalObject = context.GetGlobalObject();
            var onmessage = JSApi.JS_GetPropertyStr(context, globalObject, "onmessage");

            while (_worker.isRunning)
            {
                lock (_messageQueue)
                {
                    list.AddRange(_messageQueue);
                    _messageQueue.Clear();
                }

                if (list.Count == 0)
                {
                    Thread.Yield();
                }
                else
                {
                    for (int i = 0, count = list.Count; i < count; i++)
                    {
                        var buf = list[i];

                        //TODO: restore js object 

                        buf.Release();
                    }
                }

                var now = Environment.TickCount;
                var dt = now - tick;
                tick = now;

                _worker.Update(dt);
            }

            JSApi.JS_FreeValue(context, onmessage);
            JSApi.JS_FreeValue(context, globalObject);
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
