using System;
using System.Collections.Generic;

namespace QuickJS.Utils
{
    public class ObjectCache
    {
        private class ObjectRef
        {
            public int next;
            public object target;
        }

        private int _freeIndex = -1;

        // id => host object
        private List<ObjectRef> _map = new List<ObjectRef>();
        // host object => jsvalue heapptr (dangerous)
        private Dictionary<object, IntPtr> _rmap = new Dictionary<object, IntPtr>(EqualityComparer.Default);
        // weak reference table for delegates
        private Dictionary<IntPtr, WeakReference> _delegateMap = new Dictionary<IntPtr, WeakReference>();

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
            _freeIndex = 0;
            _map.Clear();
            _rmap.Clear();
            _delegateMap.Clear();
        }

        public void AddJSValue(object o, IntPtr heapptr)
        {
            if (o != null)
            {
                _rmap.Add(o, heapptr);
            }
        }

        public bool TryGetJSValue(object o, out IntPtr heapptr)
        {
            if (o == null)
            {
                heapptr = IntPtr.Zero;
                return false;
            }
            return _rmap.TryGetValue(o, out heapptr);
        }

        public bool RemoveJSValue(object o)
        {
            return o != null && _rmap.Remove(o);
        }

        public void AddDelegate(IntPtr jso, ScriptDelegate o)
        {
            _delegateMap[jso] = new WeakReference(o);
            // 不能直接保留 o -> jso 的映射 (会产生o的强引用)
            // Delegate 对 ScriptDelegate 存在强引用 (首参), ScriptDelegate 对 jsobject 存在强引用
            // AddJSValue(o, jso); 
        }

        public bool TryGetDelegate(IntPtr jso, out ScriptDelegate o)
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

        public bool RemoveDelegate(IntPtr jso)
        {
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

        public int AddObject(object o)
        {
            if (o != null)
            {
                if (_freeIndex < 0)
                {
                    var freeEntry = new ObjectRef();
                    var id = _map.Count;
                    _map.Add(freeEntry);
                    freeEntry.next = -1;
                    freeEntry.target = o;
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
            if (TryGetObject(id, out o))
            {
                var entry = _map[id];
                entry.next = _freeIndex;
                entry.target = null;
                _freeIndex = id;
                // UnityEngine.Debug.LogFormat("[cache] remove object at {0}", id);
                RemoveJSValue(o);
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
                IntPtr heapptr;
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