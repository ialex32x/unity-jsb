using System;
using System.Collections.Generic;

namespace QuickJS.Utils
{
    using Native;

    public class ObjectCache
    {
        private class ObjectRef
        {
            public int next;
            public object target;
            public bool disposable;
        }

        private bool _disposed;
        private int _freeIndex = -1;

        // id => host object
        private List<ObjectRef> _map = new List<ObjectRef>();

        // host object => jsvalue heapptr (dangerous, no ref count)
        private Dictionary<object, JSValue> _rmap = new Dictionary<object, JSValue>(EqualityComparer.Default);

        // 刻意与 ScriptValue 隔离
        private JSWeakMap<ScriptDelegate> _delegateMap = new JSWeakMap<ScriptDelegate>();
        private JSWeakMap<ScriptValue> _scriptValueMap = new JSWeakMap<ScriptValue>();
        private JSWeakMap<ScriptPromise> _scriptPromiseMap = new JSWeakMap<ScriptPromise>();

        private IScriptLogger _logger;

        public ObjectCache(IScriptLogger logger)
        {
            _logger = logger;
        }

        public int GetManagedObjectCount()
        {
            return _map.Count;
        }

        public int GetJSObjectCount()
        {
            return _rmap.Count;
        }

        public int GetDelegateCount()
        {
            return _delegateMap.Count;
        }

        public int GetScriptValueCount()
        {
            return _scriptValueMap.Count;
        }

        public int GetScriptPromiseCount()
        {
            return _scriptPromiseMap.Count;
        }

        public void Destroy()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _freeIndex = 0;
            _map.Clear();
            _rmap.Clear();
            _delegateMap.Clear();
            _scriptValueMap.Clear();
            _scriptPromiseMap.Clear();
        }

        /// <summary>
        /// 建立 object to jsvalue 的映射. 
        /// 外部必须自己保证 object 存在的情况下对应的 js value 不会被释放.
        /// </summary>
        public void AddJSValue(object o, JSValue heapptr)
        {
            if (_disposed)
            {
                return;
            }
            if (o != null)
            {
#if JSB_DEBUG
                if (_logger != null)
                {
                    JSValue oldPtr;
                    if (TryGetJSValue(o, out oldPtr))
                    {
                        _logger.Write(LogLevel.Assert, "exists object => js value mapping {0}: {1} => {2}", o, oldPtr, heapptr);
                    }
                }
#endif
                _rmap[o] = heapptr;
            }
        }

        public bool TryGetJSValue(object o, out JSValue heapptr)
        {
            if (o == null)
            {
                heapptr = JSApi.JS_UNDEFINED;
                return false;
            }
            return _rmap.TryGetValue(o, out heapptr);
        }

        public bool RemoveJSValue(object o)
        {
            if (_disposed)
            {
                return false;
            }
            return o != null && _rmap.Remove(o);
        }

        public int AddObject(object o, bool disposable)
        {
            if (!_disposed && o != null)
            {
                if (_freeIndex < 0)
                {
                    var freeEntry = new ObjectRef();
                    var id = _map.Count;
                    _map.Add(freeEntry);
                    freeEntry.next = -1;
                    freeEntry.target = o;
                    freeEntry.disposable = disposable;
                    return id;
                }
                else
                {
                    var id = _freeIndex;
                    var freeEntry = _map[id];
                    _freeIndex = freeEntry.next;
                    freeEntry.next = -1;
                    freeEntry.target = o;
                    freeEntry.disposable = disposable;
                    return id;
                }
            }
            return -1;
        }

        public bool SetObjectDisposable(int id, bool disposable)
        {
            if (id >= 0 && id < _map.Count)
            {
                var entry = _map[id];
                if (entry.next == -1)
                {
                    entry.disposable = disposable;
                    return true;
                }
            }
            
            return false;
        }

        public bool TryGetObject(int id, out object o)
        {
            if (id >= 0 && id < _map.Count)
            {
                var entry = _map[id];
                if (entry.next == -1)
                {
                    o = entry.target;
                    return true;
                }
            }
            o = null;
            return false;
        }

        public bool RemoveObject(int id)
        {
            object o;
            return RemoveObject(id, out o);
        }

        public bool RemoveObject(int id, out object o)
        {
            if (TryGetObject(id, out o))
            {
                var entry = _map[id];
                var disposable = entry.disposable;
                entry.next = _freeIndex;
                entry.target = null;
                _freeIndex = id;
                RemoveJSValue(o);
                if (disposable)
                {
                    var jsf = o as IDisposable;
                    if (jsf != null)
                    {
                        jsf.Dispose();
                    }
                }
                return true;
            }
            return false;
        }

        // 覆盖已有记录, 无记录返回 false
        public bool ReplaceObject(int id, object o)
        {
            object oldValue;
            if (TryGetObject(id, out oldValue))
            {
                var entry = _map[id];
                entry.target = o;
                JSValue heapptr;
                if (oldValue != null && _rmap.TryGetValue(oldValue, out heapptr))
                {
                    _rmap.Remove(oldValue);
                    _rmap[o] = heapptr;
                }
                return true;
            }
            return false;
        }

        public bool TryGetTypedWeakObject<T>(int id, out T o)
        where T : class
        {
            object obj;
            if (TryGetObject(id, out obj))
            {
                var w = obj as WeakReference;
                o = w != null ? w.Target as T : null;
                return true;
            }
            o = null;
            return false;
        }

        public bool TryGetTypedObject<T>(int id, out T o)
        where T : class
        {
            object obj;
            if (TryGetObject(id, out obj))
            {
                o = obj as T;
                return true;
            }
            o = null;
            return false;
        }

        public bool MatchObjectType(int id, Type type)
        {
            object o;
            if (TryGetObject(id, out o))
            {
                if (o != null)
                {
                    var otype = o.GetType();
                    return otype == type || otype.IsSubclassOf(type) || type.IsAssignableFrom(otype);
                }
                return true;
            }
            return false;
        }

        #region delegate mapping 

        public void AddDelegate(JSValue jso, ScriptDelegate o)
        {
            if (_disposed)
            {
                return;
            }
            _delegateMap.Add(jso, o);
        }

        public bool TryGetDelegate(JSValue jso, out ScriptDelegate o)
        {
            return _delegateMap.TryGetValue(jso, out o);
        }

        public bool RemoveDelegate(JSValue jso)
        {
            if (_disposed)
            {
                return false;
            }
            return _delegateMap.Remove(jso);
        }

        #endregion

        #region script value mapping 

        public void AddScriptValue(JSValue jso, ScriptValue o)
        {
            if (_disposed)
            {
                return;
            }
            _scriptValueMap.Add(jso, o);
        }

        public bool TryGetScriptValue<T>(JSValue jso, out T o)
        where T : ScriptValue
        {
            ScriptValue value;
            if (_scriptValueMap.TryGetValue(jso, out value))
            {
                o = value as T;
                return true;
            }
            o = null;
            return false;
        }

        public bool RemoveScriptValue(JSValue jso)
        {
            if (_disposed)
            {
                return false;
            }
            return _scriptValueMap.Remove(jso);
        }

        #endregion 

        #region script promise mapping 

        public void AddScriptPromise(JSValue jso, ScriptPromise o)
        {
            if (_disposed)
            {
                return;
            }
            _scriptPromiseMap.Add(jso, o);
        }

        public bool TryGetScriptPromise<T>(JSValue jso, out T o)
        where T : ScriptPromise
        {
            ScriptPromise value;
            if (_scriptPromiseMap.TryGetValue(jso, out value))
            {
                o = value as T;
                return true;
            }
            o = null;
            return false;
        }

        public bool RemoveScriptPromise(JSValue jso)
        {
            if (_disposed)
            {
                return false;
            }
            return _scriptPromiseMap.Remove(jso);
        }

        #endregion 
    }
}