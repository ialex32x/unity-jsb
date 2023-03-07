using System;
using System.Collections.Generic;

namespace QuickJS.Utils
{
    public interface IObjectCollectionEntry
    {
        void OnCollectionReleased();
    }

    /// <summary>
    /// A collection of entries who need to be notified at the stage of releasing script runtime.
    /// </summary>
    public class ObjectCollection
    {
        public const int DefaultSize = 32;

        public struct Handle
        {
            public int id;
            public int tag;
        }

        private struct ObjectEntry
        {
            public int next;
            public int tag;
            public WeakReference<IObjectCollectionEntry> target;
        }

        private int _freeIndex = -1;
        private int _activeCount = 0;
        private int _allocatedCount = 0;
        private ObjectEntry[] _entries = new ObjectEntry[DefaultSize];

        public int count => _activeCount;

        public void Clear()
        {
            if (_activeCount > 0)
            {
                for (int i = 0; i < _allocatedCount; ++i)
                {
                    ref var entry = ref _entries[i];
                    IObjectCollectionEntry target;
                    if (entry.target.TryGetTarget(out target))
                    {
#if JSB_DEBUG && !JSB_UNITYLESS
                        UnityEngine.Debug.LogWarningFormat("releasing collection entry: {0}", target);
#endif
                        target.OnCollectionReleased();
                    }
#if JSB_DEBUG && !JSB_UNITYLESS
                    else if (entry.next == -1)
                    {
                        UnityEngine.Debug.LogWarningFormat("null collection entry");
                    }
#endif
                }
                _allocatedCount = 0;
            }
        }

        public void AddObject(IObjectCollectionEntry o, out Handle handle)
        {
            if (o != null)
            {
                ++_activeCount;
                if (_freeIndex < 0)
                {
                    var id = _allocatedCount++;
                    var oldSize = _entries.Length;
                    if (id < oldSize)
                    {
                        ref var freeEntry = ref _entries[id];
                        ++freeEntry.tag;
                        freeEntry.next = -1;
                        if (freeEntry.target == null)
                        {
                            freeEntry.target = new WeakReference<IObjectCollectionEntry>(o);
                        }
                        else
                        {
                            freeEntry.target.SetTarget(o);
                        }
                        handle = new Handle() { id = id, tag = freeEntry.tag };
                    }
                    else
                    {
                        Array.Resize(ref _entries, oldSize <= 1024 ? oldSize * 2 : oldSize + 128);
                        ref var freeEntry = ref _entries[id];
                        ++freeEntry.tag;
                        freeEntry.next = -1;
                        freeEntry.target = new WeakReference<IObjectCollectionEntry>(o);
                        handle = new Handle() { id = id, tag = freeEntry.tag };
                    }
                }
                else
                {
                    var id = _freeIndex;
                    ref var freeEntry = ref _entries[id];
                    _freeIndex = freeEntry.next;
                    ++freeEntry.tag;
                    freeEntry.next = -1;
                    freeEntry.target.SetTarget(o);
                    handle = new Handle() { id = id, tag = freeEntry.tag };
                }
            }
            else
            {
                handle = new Handle();
            }
        }

        public bool IsHandleValid(Handle handle)
        {
            var id = handle.id;
            if (id >= 0 && id < _allocatedCount)
            {
                ref var entry = ref _entries[id];
                if (entry.next == -1 && entry.tag == handle.tag)
                {
                    return true;
                }
            }
            return false;
        }

        public bool TryGetObject(Handle handle, out IObjectCollectionEntry o)
        {
            var id = handle.id;
            if (id >= 0 && id < _allocatedCount)
            {
                ref var entry = ref _entries[id];
                if (entry.next == -1 && entry.tag == handle.tag)
                {
                    return entry.target.TryGetTarget(out o);
                }
            }
            o = null;
            return false;
        }

        public bool RemoveObject(Handle handle)
        {
            IObjectCollectionEntry o;
            return RemoveObject(handle, out o);
        }

        public bool RemoveObject(Handle handle, out IObjectCollectionEntry o)
        {
            if (TryGetObject(handle, out o))
            {
                ref var entry = ref _entries[handle.id];
                entry.next = _freeIndex;
                entry.target.SetTarget(null);
                ++entry.tag;
                _freeIndex = handle.id;
                --_activeCount;
                return true;
            }
            return false;
        }
    }
}
