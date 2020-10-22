using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using QuickJS.Native;
using System.Threading;
using System.Reflection;
using QuickJS.Binding;
using QuickJS.Utils;

namespace QuickJS
{
    public partial class ScriptRuntime
    {
        private class ScriptContextRef
        {
            public int next;
            public ScriptContext target;
        }

        public event Action<ScriptRuntime> OnDestroy;
        public event Action<int> OnAfterDestroy;
        public event Action OnUpdate;
        public Func<JSContext, string, string, int, string> OnSourceMap;

        // private Mutext _lock;
        private JSRuntime _rt;
        private int _runtimeId;
        private bool _withStacktrace;
        private IScriptLogger _logger;
        private int _freeContextSlot = -1;
        // private ReaderWriterLockSlim _rwlock;
        private List<ScriptContextRef> _contextRefs = new List<ScriptContextRef>();
        private ScriptContext _mainContext;
        private Queue<JSAction> _pendingActions = new Queue<JSAction>();

        private int _mainThreadId;
        private uint _class_id_alloc = JSApi.__JSB_GetClassID();

        private IScriptRuntimeListener _listener;
        private IFileSystem _fileSystem;
        private IPathResolver _pathResolver;
        private List<IModuleResolver> _moduleResolvers = new List<IModuleResolver>();
        private ObjectCache _objectCache;
        private TypeDB _typeDB;
        private TimerManager _timerManager;
        private IO.IByteBufferAllocator _byteBufferAllocator;
        private Utils.AutoReleasePool _autorelease;
        private UnityEngine.GameObject _container;

        private bool _isValid; // destroy 调用后立即 = false
        private bool _isRunning;
        private bool _isWorker;

        public bool withStacktrace
        {
            get { return _withStacktrace; }
            set { _withStacktrace = value; }
        }

        public bool isWorker { get { return _isWorker; } }

        public int id { get { return _runtimeId; } }

        public bool isRunning { get { return _isRunning; } }

        public ScriptRuntime(int runtimeId)
        {
            _runtimeId = runtimeId;
            _isWorker = false;
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public ICoroutineManager CreateCoroutineManager()
        {
            if (_container == null && _isValid)
            {
                _container = new UnityEngine.GameObject("JSRuntimeContainer");
                _container.hideFlags = UnityEngine.HideFlags.HideInHierarchy;
                UnityEngine.Object.DontDestroyOnLoad(_container);
            }
            if (_container != null)
            {
                return _container.AddComponent<Unity.DefaultCoroutineManager>();
            }

            return null;
        }

        public IFileSystem GetFileSystem()
        {
            return _fileSystem;
        }

        public IPathResolver GetPathResolver()
        {
            return _pathResolver;
        }

        public void AddSearchPath(string path)
        {
            _pathResolver.AddSearchPath(path);
        }

        public T AddModuleResolver<T>(T moduleResolver)
        where T : IModuleResolver
        {
            _moduleResolvers.Add(moduleResolver);
            return moduleResolver;
        }

        public T FindModuleResolver<T>()
        {
            for (int i = 0, count = _moduleResolvers.Count; i < count; i++)
            {
                var resolver = _moduleResolvers[i];
                if (resolver is T)
                {
                    return (T)resolver;
                }
            }
            return default(T);
        }

        public string ResolveFilePath(string parent_module_id, string module_id)
        {
            string resolved_id;
            for (int i = 0, count = _moduleResolvers.Count; i < count; i++)
            {
                var resolver = _moduleResolvers[i];
                if (resolver.ResolveModule(_fileSystem, _pathResolver, parent_module_id, module_id, out resolved_id))
                {
                    return resolved_id;
                }
            }

            return null;
        }

        public JSValue ResolveModule(ScriptContext context, string parent_module_id, string module_id)
        {
            for (int i = 0, count = _moduleResolvers.Count; i < count; i++)
            {
                var resolver = _moduleResolvers[i];
                string resolved_id;
                if (resolver.ResolveModule(_fileSystem, _pathResolver, parent_module_id, module_id, out resolved_id))
                {
                    JSValue mod;
                    if (context.LoadModuleCache(resolved_id, out mod))
                    {
                        var ctx = (JSContext)context;
                        var exports = JSApi.JS_GetProperty(ctx, mod, context.GetAtom("exports"));
                        JSApi.JS_FreeValue(ctx, mod);
                        return exports;
                    }

                    return resolver.LoadModule(context, resolved_id);
                }
            }

            return JSApi.JS_ThrowInternalError(context, "module can not be resolved");
        }

        public void Initialize(IFileSystem fileSystem, IPathResolver resolver, IScriptRuntimeListener listener, IScriptLogger logger, IO.IByteBufferAllocator byteBufferAllocator)
        {
            if (logger == null)
            {
                throw new NullReferenceException(nameof(logger));
            }

            if (fileSystem == null)
            {
                throw new NullReferenceException(nameof(fileSystem));
            }

            MethodInfo bindAll = null;
            if (!isWorker)
            {
                if (listener == null)
                {
                    throw new NullReferenceException(nameof(listener));
                }

                bindAll = typeof(Values).GetMethod("BindAll", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (bindAll == null)
                {
                    throw new Exception("generate binding code before run");
                }

                var codeGenVersionField = typeof(Values).GetField("CodeGenVersion");
                if (codeGenVersionField == null || !codeGenVersionField.IsStatic || !codeGenVersionField.IsLiteral || codeGenVersionField.FieldType != typeof(uint))
                {
                    throw new Exception("binding code version mismatch");
                }

                var codeGenVersion = (uint)codeGenVersionField.GetValue(null);
                if (codeGenVersion != ScriptEngine.VERSION)
                {
                    if (logger != null)
                    {
                        logger.Write(LogLevel.Warn, "CodeGenVersion: {0} != {1}", codeGenVersion, ScriptEngine.VERSION);
                    }
                }
            }

            _isValid = true;
            _isRunning = true;
            // _rwlock = new ReaderWriterLockSlim();
            _rt = JSApi.JS_NewRuntime();
            JSApi.JS_SetHostPromiseRejectionTracker(_rt, JSApi.PromiseRejectionTracker, IntPtr.Zero);
            JSApi.JS_SetRuntimeOpaque(_rt, (IntPtr)_runtimeId);
            JSApi.JS_SetModuleLoaderFunc(_rt, module_normalize, module_loader, IntPtr.Zero);
            CreateContext();
            JSApi.JS_NewClass(_rt, JSApi.JSB_GetBridgeClassID(), "CSharpClass", JSApi.class_finalizer);

            _listener = listener;
            _pathResolver = resolver;
            _byteBufferAllocator = byteBufferAllocator;
            _autorelease = new Utils.AutoReleasePool();
            _fileSystem = fileSystem;
            _logger = logger;
            _objectCache = new ObjectCache(_logger);
            _timerManager = new TimerManager(_logger);
            _typeDB = new TypeDB(this, _mainContext);
            listener.OnCreate(this);

            var register = new TypeRegister(this, _mainContext, _mainContext.GetGlobalObject());
            register.RegisterType(typeof(Unity.JSBehaviour));
#if UNITY_EDITOR
            register.RegisterType(typeof(Unity.JSEditorWindow));
#endif
            // await Task.Run(() => runner.OnBind(this, register));
            if (bindAll != null)
            {
                bindAll.Invoke(null, new object[] { register });
            }
            listener.OnBind(this, register);
            if (!_isWorker)
            {
                JSWorker.Bind(register);
            }
            TimerManager.Bind(register);
            ScriptContext.Bind(register);
            register.Finish();
            listener.OnComplete(this);
        }

        public ScriptRuntime CreateWorker()
        {
            if (isWorker)
            {
                throw new Exception("cannot create a worker inside a worker");
            }

            var runtime = ScriptEngine.CreateRuntime();

            runtime._isWorker = true;
            runtime.Initialize(_fileSystem, _pathResolver, _listener, _logger, new IO.ByteBufferPooledAllocator());
            return runtime;
        }

        public void AutoRelease(Utils.IReferenceObject referenceObject)
        {
            _autorelease.AutoRelease(referenceObject);
        }

        public IO.IByteBufferAllocator GetByteBufferAllocator()
        {
            return _byteBufferAllocator;
        }

        public TimerManager GetTimerManager()
        {
            return _timerManager;
        }

        public IScriptLogger GetLogger()
        {
            return _logger;
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
            // _rwlock.EnterWriteLock();
            ScriptContextRef freeEntry;
            int slotIndex;
            if (_freeContextSlot < 0)
            {
                freeEntry = new ScriptContextRef();
                slotIndex = _contextRefs.Count;
                _contextRefs.Add(freeEntry);
                freeEntry.next = -1;
            }
            else
            {
                slotIndex = _freeContextSlot;
                freeEntry = _contextRefs[slotIndex];
                _freeContextSlot = freeEntry.next;
                freeEntry.next = -1;
            }

            var context = new ScriptContext(this, slotIndex + 1);
            freeEntry.target = context;
            context.OnDestroy += OnContextDestroy;

            if (_mainContext == null)
            {
                _mainContext = context;
            }
            // _rwlock.ExitWriteLock();

            context.RegisterBuiltins();
            return context;
        }

        private void OnContextDestroy(ScriptContext context)
        {
            // _rwlock.EnterWriteLock();
            var id = context.id;
            if (id > 0)
            {
                var index = id - 1;
                var entry = _contextRefs[index];
                entry.next = _freeContextSlot;
                entry.target = null;
                _freeContextSlot = index;
            }
            // _rwlock.ExitWriteLock();
        }

        public ScriptContext GetMainContext()
        {
            return _mainContext;
        }

        public ScriptContext GetContext(JSContext ctx)
        {
            // _rwlock.EnterReadLock();
            ScriptContext context = null;
            var id = (int)JSApi.JS_GetContextOpaque(ctx);
            if (id > 0)
            {
                var index = id - 1;
                if (index < _contextRefs.Count)
                {
                    context = _contextRefs[index].target;
                }
            }
            // _rwlock.ExitReadLock();
            return context;
        }

        private static void _FreeValueAction(ScriptRuntime rt, JSAction action)
        {
            JSApi.JS_FreeValueRT(rt, action.value);
        }

        private static void _FreeValueAndDelegationAction(ScriptRuntime rt, JSAction action)
        {
            var cache = rt.GetObjectCache();
            cache.RemoveDelegate(action.value);
            JSApi.JS_FreeValueRT(rt, action.value);
        }

        private static void _FreeValueAndScriptValueAction(ScriptRuntime rt, JSAction action)
        {
            var cache = rt.GetObjectCache();
            cache.RemoveScriptValue(action.value);
            JSApi.JS_FreeValueRT(rt, action.value);
        }

        private static void _FreeValueAndScriptPromiseAction(ScriptRuntime rt, JSAction action)
        {
            var cache = rt.GetObjectCache();
            cache.RemoveScriptPromise(action.value);
            JSApi.JS_FreeValueRT(rt, action.value);
        }

        // 可在 GC 线程直接调用此方法
        public void FreeDelegationValue(JSValue value)
        {
            if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                _objectCache.RemoveDelegate(value);
                if (_rt != JSRuntime.Null)
                {
                    JSApi.JS_FreeValueRT(_rt, value);
                }
            }
            else
            {
                var act = new JSAction()
                {
                    value = value,
                    callback = _FreeValueAndDelegationAction,
                };

                lock (_pendingActions)
                {
                    _pendingActions.Enqueue(act);
                }
            }
        }

        // 可在 GC 线程直接调用此方法
        public void FreeScriptValue(JSValue value)
        {
            if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                _objectCache.RemoveScriptValue(value);
                if (_rt != JSRuntime.Null)
                {
                    JSApi.JS_FreeValueRT(_rt, value);
                }
            }
            else
            {
                var act = new JSAction()
                {
                    value = value,
                    callback = _FreeValueAndScriptValueAction,
                };

                lock (_pendingActions)
                {
                    _pendingActions.Enqueue(act);
                }
            }
        }

        // 可在 GC 线程直接调用此方法
        public void FreeScriptPromise(JSValue value)
        {
            if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                _objectCache.RemoveScriptPromise(value);
                if (_rt != JSRuntime.Null)
                {
                    JSApi.JS_FreeValueRT(_rt, value);
                }
            }
            else
            {
                var act = new JSAction()
                {
                    value = value,
                    callback = _FreeValueAndScriptPromiseAction,
                };
                lock (_pendingActions)
                {
                    _pendingActions.Enqueue(act);
                }
            }
        }

        // 可在 GC 线程直接调用此方法
        public void FreeValue(JSValue value)
        {
            if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                if (_rt != JSRuntime.Null)
                {
                    JSApi.JS_FreeValueRT(_rt, value);
                }
            }
            else
            {
                var act = new JSAction()
                {
                    value = value,
                    callback = _FreeValueAction,
                };
                lock (_pendingActions)
                {
                    _pendingActions.Enqueue(act);
                }
            }
        }

        // 可在 GC 线程直接调用此方法
        public void FreeValues(JSValue[] values)
        {
            if (values == null)
            {
                return;
            }
            if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                for (int i = 0, len = values.Length; i < len; i++)
                {
                    JSApi.JS_FreeValueRT(_rt, values[i]);
                }
            }
            else
            {
                lock (_pendingActions)
                {
                    for (int i = 0, len = values.Length; i < len; i++)
                    {
                        var act = new JSAction()
                        {
                            value = values[i],
                            callback = _FreeValueAction,
                        };
                        _pendingActions.Enqueue(act);
                    }
                }
            }
        }

        // 可在 GC 线程直接调用此方法
        public unsafe void FreeValues(int argc, JSValue* values)
        {
            if (argc == 0)
            {
                return;
            }
            if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                for (int i = 0; i < argc; i++)
                {
                    JSApi.JS_FreeValueRT(_rt, values[i]);
                }
            }
            else
            {
                lock (_pendingActions)
                {
                    for (int i = 0; i < argc; i++)
                    {
                        var act = new JSAction()
                        {
                            value = values[i],
                            callback = _FreeValueAction,
                        };
                        _pendingActions.Enqueue(act);
                    }
                }
            }
        }

        public bool EnqueueAction(JSActionCallback callback, object args)
        {
            lock (_pendingActions)
            {
                if (!_isValid)
                {
                    return false;
                }
                _pendingActions.Enqueue(new JSAction() { callback = callback, args = args });
            }
            return true;
        }

        public bool EnqueueAction(JSAction action)
        {
            lock (_pendingActions)
            {
                if (!_isValid)
                {
                    return false;
                }
                _pendingActions.Enqueue(action);
            }
            return true;
        }

        public void EvalFile(string fileName)
        {
            EvalFile(fileName, typeof(void));
        }

        public T EvalFile<T>(string fileName)
        {
            return (T)EvalFile(fileName, typeof(T));
        }

        public object EvalFile(string fileName, Type returnType)
        {
            var resolvedPath = ResolveFilePath("", fileName);
            if (resolvedPath != null)
            {
                var source = _fileSystem.ReadAllBytes(resolvedPath);
                return _mainContext.EvalSource(source, resolvedPath, returnType);
            }
            else
            {
                throw new Exception("can not resolve file path");
            }
        }

        public void EvalMain(string fileName)
        {
            EvalMain(fileName, typeof(void));
        }

        public T EvalMain<T>(string fileName)
        {
            return (T)EvalMain(fileName, typeof(T));
        }

        public object EvalMain(string fileName, Type returnType)
        {
            var resolvedPath = ResolveFilePath("", fileName);
            if (resolvedPath != null)
            {
                var source = _fileSystem.ReadAllBytes(resolvedPath);
                return _mainContext.EvalMain(source, resolvedPath, returnType);
            }
            else
            {
                throw new UnexpectedException(fileName, "can not resolve file path");
            }
        }

        public bool IsMainThread()
        {
            return _mainThreadId == Thread.CurrentThread.ManagedThreadId;
        }

        // main loop
        public void Update(int ms)
        {
            if (!_isValid || !_isRunning)
            {
                return;
            }

#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isCompiling)
            {
                ScriptEngine.Shutdown();
                _logger?.Write(LogLevel.Warn, "assembly reloading, shutdown script engine immediately");
                return;
            }
#endif
            if (_pendingActions.Count != 0)
            {
                ExecutePendingActions();
            }

            OnUpdate?.Invoke(); //TODO: optimize
            ExecutePendingJob();

            // poll here;
            _timerManager.Update(ms);

            if (_autorelease != null)
            {
                _autorelease.Drain();
            }
        }

        public void ExecutePendingJob()
        {
            JSContext ctx;
            while (true)
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

        private void ExecutePendingActions()
        {
            lock (_pendingActions)
            {
                while (true)
                {
                    if (_pendingActions.Count == 0)
                    {
                        break;
                    }

                    var action = _pendingActions.Dequeue();
                    action.callback(this, action);
                }
            }
        }

        ~ScriptRuntime()
        {
            Shutdown();
        }

        public void Shutdown()
        {
            //TODO: lock?
            _isRunning = false;
            if (!_isWorker)
            {
                Destroy();
            }
        }

        public void Destroy()
        {
            lock (this)
            {
                if (!_isValid)
                {
                    return;
                }
                _isValid = false;
                try
                {
                    OnDestroy?.Invoke(this);
                }
                catch (Exception e)
                {
                    _logger?.WriteException(e);
                }
            }

            _timerManager.Destroy();
            _objectCache.Destroy();
            _typeDB.Destroy();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            ExecutePendingActions();

            // _rwlock.EnterWriteLock();
            for (int i = 0, count = _contextRefs.Count; i < count; i++)
            {
                var contextRef = _contextRefs[i];
                contextRef.target.Destroy();
            }

            _contextRefs.Clear();
            _mainContext = null;
            // _rwlock.ExitWriteLock();


            if (_container != null)
            {
                if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
                {
                    UnityEngine.Object.DestroyImmediate(_container);
                }
                _container = null;
            }

            JSApi.JS_FreeRuntime(_rt);
            var id = _runtimeId;
            _runtimeId = -1;
            _rt = JSRuntime.Null;

            try
            {
                OnAfterDestroy?.Invoke(id);
            }
            catch (Exception e)
            {
                _logger?.WriteException(e);
            }
        }

        public static implicit operator JSRuntime(ScriptRuntime se)
        {
            return se._rt;
        }
    }
}