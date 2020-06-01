using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AOT;
using QuickJS.Native;
using System.Threading;
using System.Reflection;
using QuickJS.Utils;

namespace QuickJS
{
    using UnityEngine;

    public partial class ScriptRuntime
    {
        public event Action<ScriptRuntime> OnDestroy;

        private JSRuntime _rt;
        private List<ScriptContext> _contexts = new List<ScriptContext>();
        private ScriptContext _mainContext;
        private Queue<JSValue> _pendingGC = new Queue<JSValue>();

        private int _mainThreadId;
        private uint _class_id_alloc = JSApi.__JSB_GetClassID();

        public uint _def_class_id;
        public uint _def_struct_id;
        public uint _def_type_id;

        private Utils.IFileResolver _fileResolver;
        private Utils.ObjectCache _objectCache = new Utils.ObjectCache();
        private Utils.TypeDB _typeDB;
        private IO.ByteBufferAllocator _byteBufferAllocator;

        public ScriptRuntime()
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            _rt = JSApi.JS_NewRuntime();
            _def_class_id = NewClassID();
            _def_struct_id = NewClassID();
            _def_type_id = NewClassID();
            JSApi.JS_SetModuleLoaderFunc(_rt, module_normalize, module_loader, IntPtr.Zero);
            _mainContext = CreateContext();
        }

        public void Initialize(MonoBehaviour runner, int step = 30)
        {
            if (runner != null)
            {
                runner.StartCoroutine(_InitializeStep(_mainContext, step));
            }
            else
            {
                var e = _InitializeStep(_mainContext, step);
                while (e.MoveNext()) ;
            }
        }

        private IEnumerator _InitializeStep(ScriptContext context, int step)
        {
            var register = new TypeRegister(context);
            var regArgs = new object[] { register };
            var bindingTypes = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int assemblyIndex = 0, assemblyCount = assemblies.Length; assemblyIndex < assemblyCount; assemblyIndex++)
            {
                var assembly = assemblies[assemblyIndex];
                try
                {
                    if (assembly.IsDynamic)
                    {
                        continue;
                    }
                    var exportedTypes = assembly.GetExportedTypes();
                    for (int i = 0, size = exportedTypes.Length; i < size; i++)
                    {
                        var type = exportedTypes[i];
#if UNITY_EDITOR
                        if (type.IsDefined(typeof(JSAutoRunAttribute), false))
                        {
                            try
                            {
                                var run = type.GetMethod("Run", BindingFlags.Static | BindingFlags.Public);
                                if (run != null)
                                {
                                    run.Invoke(null, null);
                                }
                            }
                            catch (Exception exception)
                            {
                                Debug.LogWarning($"JSAutoRun failed: {exception}");
                            }
                            continue;
                        }
#endif
                        var attributes = type.GetCustomAttributes(typeof(JSBindingAttribute), false);
                        if (attributes.Length == 1)
                        {
                            var jsBinding = attributes[0] as JSBindingAttribute;
                            if (jsBinding.Version == 0 || jsBinding.Version == ScriptEngine.VERSION)
                            {
                                bindingTypes.Add(type);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("assembly: {0}, {1}", assembly, e);
                }
            }
            var numRegInvoked = bindingTypes.Count;
            for (var i = 0; i < numRegInvoked; ++i)
            {
                var type = bindingTypes[i];
                var reg = type.GetMethod("reg");
                if (reg != null)
                {
                    reg.Invoke(null, regArgs);
                    
                    if (i % step == 0)
                    {
                        yield return null;
                    }
                }
            }
            
            // Debug.LogFormat("exported {0} classes", _exported.Count);
            _typeDB = register.Finish();
        }

        public TypeDB GetTypeDB()
        {
            return _typeDB;
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
                CollectPendingGarbage();
            }
            JSContext ctx;
            while(true)
            {
                var err = JSApi.JS_ExecutePendingJob(_rt, out ctx);
                if (err == 0)
                {
                    break;
                }
                if (err < 0) 
                {
                    ctx.print_exception();
                }
            }
        }

        private void CollectPendingGarbage()
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

            _objectCache.Clear();
            GC.Collect();
            CollectPendingGarbage();
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