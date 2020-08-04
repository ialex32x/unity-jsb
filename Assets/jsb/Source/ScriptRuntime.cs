using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using AOT;
using QuickJS.Native;
using System.Threading;
using System.Reflection;
using QuickJS.Binding;
using QuickJS.Utils;

namespace QuickJS
{
    using UnityEngine;

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
        private Queue<JSAction> _pendingGC = new Queue<JSAction>();

        private int _mainThreadId;
        private uint _class_id_alloc = JSApi.__JSB_GetClassID();

        private IFileResolver _fileResolver;
        private IFileSystem _fileSystem;
        private ObjectCache _objectCache = new ObjectCache();
        private TypeDB _typeDB;
        private TimerManager _timerManager;
        private IO.IByteBufferAllocator _byteBufferAllocator;
        private Utils.AutoReleasePool _autorelease;
        private GameObject _container;
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
            _isValid = true;
            _isRunning = true;
            _isWorker = false;
            _runtimeId = runtimeId;
            // _rwlock = new ReaderWriterLockSlim();
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            _rt = JSApi.JS_NewRuntime();
            JSApi.JS_SetRuntimeOpaque(_rt, (IntPtr)_runtimeId);
            JSApi.JS_SetModuleLoaderFunc(_rt, module_normalize, module_loader, IntPtr.Zero);
            CreateContext();
            JSApi.JS_NewClass(_rt, JSApi.JSB_GetBridgeClassID(), "CSharpClass", JSApi.class_finalizer);
        }

        public GameObject GetContainer()
        {
            if (_container == null && _isValid)
            {
                _container = new GameObject("JSRuntimeContainer");
                _container.hideFlags = HideFlags.HideInHierarchy;
                Object.DontDestroyOnLoad(_container);
            }
            return _container;
        }

        public IFileResolver GetFileResolver()
        {
            return _fileResolver;
        }

        public IFileSystem GetFileSystem()
        {
            return _fileSystem;
        }

        public void AddSearchPath(string path)
        {
            _fileResolver.AddSearchPath(path);
        }

        public void Initialize(IFileSystem fileSystem, IScriptRuntimeListener listener)
        {
            Initialize(fileSystem, new FileResolver(), listener, new UnityLogger(), new IO.ByteBufferPooledAllocator());
        }

        public void Initialize(IFileSystem fileSystem, IFileResolver resolver, IScriptRuntimeListener listener, IScriptLogger logger, IO.IByteBufferAllocator byteBufferAllocator)
        {
            if (logger == null)
            {
                throw new NullReferenceException(nameof(logger));
            }
            if (listener == null)
            {
                throw new NullReferenceException(nameof(listener));
            }
            if (fileSystem == null)
            {
                throw new NullReferenceException(nameof(fileSystem));
            }
            MethodInfo bindAll = null;
            if (!isWorker)
            {
                bindAll = typeof(Values).GetMethod("BindAll", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (bindAll == null)
                {
                    throw new Exception("Generate binding code before run");
                }
            }
            _fileResolver = resolver;
            _byteBufferAllocator = byteBufferAllocator;
            _autorelease = new Utils.AutoReleasePool();
            _fileSystem = fileSystem;
            _logger = logger;
            _timerManager = new TimerManager(_logger);
            _typeDB = new TypeDB(this, _mainContext);

            var register = new TypeRegister(this, _mainContext);
            register.RegisterType(typeof(ScriptBridge));
            // await Task.Run(() => runner.OnBind(this, register));
            if (bindAll != null)
            {
                bindAll.Invoke(null, new object[] { register });
            }
            listener.OnBind(this, register);
            TimerManager.Bind(register);
            ScriptContext.Bind(register);
            register.Finish();
            listener.OnComplete(this);
        }

        public ScriptRuntime CreateWorker(IScriptRuntimeListener listener)
        {
            if (isWorker)
            {
                throw new Exception("cannot create a worker inside a worker");
            }

            var runtime = ScriptEngine.CreateRuntime();

            runtime._isWorker = true;
            runtime.Initialize(_fileSystem, _fileResolver, listener, _logger, new IO.ByteBufferPooledAllocator());
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

        private static void _FreeValueAction(ScriptRuntime rt, JSValue value)
        {
            JSApi.JS_FreeValueRT(rt, value);
        }

        private static void _FreeValueAndDelegationAction(ScriptRuntime rt, JSValue value)
        {
            var cache = rt.GetObjectCache();
            cache.RemoveDelegate(value);
            JSApi.JS_FreeValueRT(rt, value);
        }

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
                lock (_pendingGC)
                {
                    _pendingGC.Enqueue(act);
                }
            }
        }

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
                lock (_pendingGC)
                {
                    _pendingGC.Enqueue(act);
                }
            }
        }

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
                lock (_pendingGC)
                {
                    for (int i = 0, len = values.Length; i < len; i++)
                    {
                        var act = new JSAction()
                        {
                            value = values[i],
                            callback = _FreeValueAction,
                        };
                        _pendingGC.Enqueue(act);
                    }
                }
            }
        }

        public void EvalMain(string fileName)
        {
            string resolvedPath;
            if (_fileResolver.ResolvePath(_fileSystem, fileName, out resolvedPath))
            {
                var source = _fileSystem.ReadAllBytes(resolvedPath);
                var input_bytes = TextUtils.GetShebangNullTerminatedCommonJSBytes(source);
                _mainContext.EvalMain(input_bytes, resolvedPath);
            }
        }

        // main loop
        public void Update(int ms)
        {
            if (_pendingGC.Count != 0)
            {
                CollectPendingGarbage();
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

                    var action = _pendingGC.Dequeue();
                    action.callback(this, action.value);
                }
            }
        }

        public void Shutdown()
        {
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
            _objectCache.Clear();
            _typeDB.Destroy();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            CollectPendingGarbage();

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
                Object.DestroyImmediate(_container);
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