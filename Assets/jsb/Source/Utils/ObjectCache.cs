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
            public bool finalizer;
        }

        private bool _disposing;
        private int _freeIndex = -1;

        // id => host object
        private List<ObjectRef> _map = new List<ObjectRef>();

        // host object => jsvalue heapptr (dangerous, no ref count)
        private Dictionary<object, JSValue> _rmap = new Dictionary<object, JSValue>(EqualityComparer.Default);

        // weak reference table for delegates (dangerous, no ref count)
        private Dictionary<JSValue, WeakReference> _delegateMap = new Dictionary<JSValue, WeakReference>();

        // weak reference table for script values (dangerous, no ref count)
        // private Dictionary<JSValue, WeakReference> _valueMap = new Dictionary<JSValue, WeakReference>();

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

        public void Clear()
        {
            _disposing = true;
            _freeIndex = 0;
            _map.Clear();
            _rmap.Clear();

            var delegateMapSize = _delegateMap.Values.Count;
            var delegates = new WeakReference[delegateMapSize];
            _delegateMap.Values.CopyTo(delegates, 0);
            _delegateMap.Clear();
            for (var i = 0; i < delegateMapSize; i++)
            {
                var d = delegates[i].Target as ScriptDelegate;
                if (d != null)
                {
                    d.Dispose();
                }
            }

            // var valueMapSize = _valueMap.Values.Count;
            // var values = new WeakReference[valueMapSize];
            // _valueMap.Values.CopyTo(values, 0);
            // _valueMap.Clear();
            // for (var i = 0; i < valueMapSize; i++)
            // {
            //     var d = values[i].Target as ScriptValue;
            //     if (d != null)
            //     {
            //         d.Dispose();
            //     }
            // }
        }

        /// <summary>
        /// 建立 object to jsvalue 的映射. 
        /// 外部必须自己保证 object 存在的情况下对应的 js value 不会被释放.
        /// </summary>
        public void AddJSValue(object o, JSValue heapptr)
        {
            if (_disposing)
            {
                return;
            }
            if (o != null)
            {
#if JSB_DEBUG
                if (RemoveJSValue(o))
                {
                    UnityEngine.Debug.LogErrorFormat("exists object => js value mapping {0}: {1}", o, heapptr);
                }
#endif
                _rmap.Add(o, heapptr);
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
            if (_disposing)
            {
                return false;
            }
            return o != null && _rmap.Remove(o);
        }

        public void AddDelegate(JSValue jso, ScriptDelegate o)
        {
            if (_disposing)
            {
                return;
            }
            ScriptDelegate old;
            if (TryGetDelegate(jso, out old))
            {
                old.Dispose();
            }
            _delegateMap[jso] = new WeakReference(o);
            // 不能直接保留 o -> jso 的映射 (会产生o的强引用)
            // Delegate 对 ScriptDelegate 存在强引用 (首参), ScriptDelegate 对 jsobject 存在强引用
            // AddJSValue(o, jso); 
        }

        public bool TryGetDelegate(JSValue jso, out ScriptDelegate o)
        {
            WeakReference weakRef;
            if (_delegateMap.TryGetValue(jso, out weakRef))
            {
                o = weakRef.Target as ScriptDelegate;
                return o != null;
            }
            o = null;
            return false;
        }

        public bool RemoveDelegate(JSValue jso)
        {
            if (_disposing)
            {
                return false;
            }
            WeakReference weakRef;
            var r = false;
            if (_delegateMap.TryGetValue(jso, out weakRef))
            {
                r = true;
                _delegateMap.Remove(jso);
                // RemoveJSValue(weakRef.Target);
            }
            return r;
        }

        public int AddObject(object o, bool finalizer)
        {
            if (!_disposing && o != null)
            {
                if (_freeIndex < 0)
                {
                    var freeEntry = new ObjectRef();
                    var id = _map.Count;
                    _map.Add(freeEntry);
                    freeEntry.next = -1;
                    freeEntry.target = o;
                    freeEntry.finalizer = finalizer;
                    // UnityEngine.Debug.LogFormat("[cache] (new) add object at {0}", id);
                    return id;
                }
                else
                {
                    var id = _freeIndex;
                    var freeEntry = _map[id];
                    _freeIndex = freeEntry.next;
                    freeEntry.next = -1;
                    freeEntry.target = o;
                    freeEntry.finalizer = finalizer;
                    // UnityEngine.Debug.LogFormat("[cache] (reuse) add object at {0} [{1}]", id, o.GetType());
                    return id;
                }
            }
            return -1;
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
                var finalizer = entry.finalizer;
                entry.next = _freeIndex;
                entry.target = null;
                _freeIndex = id;
                // UnityEngine.Debug.LogFormat("[cache] remove object at {0}", id);
                RemoveJSValue(o);
                if (finalizer)
                {
                    var jsf = o as IScriptFinalize;
                    if (jsf != null)
                    {
                        jsf.OnJSFinalize();
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
                // UnityEngine.Debug.LogFormat("[cache] replace object at {0}", id);
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
    }
}