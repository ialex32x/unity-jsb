using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AOT;
using QuickJS.Native;
using System.Threading;
using System.Reflection;

namespace QuickJS
{
    using UnityEngine;

    public partial class ScriptRuntime
    {
        public event Action<ScriptRuntime> OnDestroy;

        private JSRuntime _rt;
        private List<ScriptContext> _contexts = new List<ScriptContext>();
        private Queue<JSValue> _pendingGC = new Queue<JSValue>();

        private int _mainThreadId;
        private uint _class_id_alloc = JSApi.__JSB_GetClassID();

        private Utils.IFileResolver _fileResolver;
        private Utils.ObjectCache _objectCache = new Utils.ObjectCache();
        private IO.ByteBufferAllocator _byteBufferAllocator;

        public ScriptRuntime()
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            _rt = JSApi.JS_NewRuntime();
            JSApi.JS_SetModuleLoaderFunc(_rt, module_normalize, module_loader, IntPtr.Zero);
        }

        public Utils.ObjectCache GetObjectCache()
        {
            return _objectCache;
        }

        public JSClassID NewClassID()
        {
            return _class_id_alloc++;
        }

        public ScriptContext CreateContext()
        {
            var context = new ScriptContext(this);
            context.OnDestroy += OnContextDestroy;
            _contexts.Add(context);
            return context;
        }

        private void OnContextDestroy(ScriptContext context)
        {
            _contexts.Remove(context);
        }

        public ScriptContext GetContext(JSContext ctx)
        {
            for (int i = 0, count = _contexts.Count; i < count; i++)
            {
                var context = _contexts[i];
                if (context.IsContext(ctx))
                {
                    return context;
                }
            }
            return null;
        }

        public void FreeValue(JSValue value)
        {
            if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                JSApi.JS_FreeValueRT(_rt, value);
            }
            else
            {
                lock (_pendingGC)
                {
                    _pendingGC.Enqueue(value);
                }
            }
        }

        public void Update(float deltaTime)
        {
            if (_pendingGC.Count != 0)
            {
                lock (_pendingGC)
                {
                    while (true)
                    {
                        if (_pendingGC.Count == 0)
                        {
                            break;
                        }
                        var value = _pendingGC.Dequeue();
                        JSApi.JS_FreeValueRT(_rt, value);
                    }
                }
            }
        }

        public void Destroy()
        {
            try
            {
                OnDestroy?.Invoke(this);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            for (int i = 0, count = _contexts.Count; i < count; i++)
            {
                var context = _contexts[i];
                context.Destroy();
            }
            _contexts.Clear();
            JSApi.JS_FreeRuntime(_rt);
            _rt = JSRuntime.Null;
        }

        public static implicit operator JSRuntime(ScriptRuntime se)
        {
            return se._rt;
        }
    }
}