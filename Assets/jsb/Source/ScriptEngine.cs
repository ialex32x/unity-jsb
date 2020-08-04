using System;
using System.Collections.Generic;
using System.Threading;
using AOT;
using QuickJS.Native;
using QuickJS.Utils;

namespace QuickJS
{
    using UnityEngine;

    public class ScriptEngine
    {
        private class ScriptRuntimeRef
        {
            public int next;
            public ScriptRuntime target;
        }

        public const uint VERSION = 0x723 + 1;

        private static int _freeSlot = -1;
        private static List<ScriptRuntimeRef> _runtimeRefs = new List<ScriptRuntimeRef>();
        private static ReaderWriterLockSlim _rwlock = new ReaderWriterLockSlim();

        public static IScriptLogger GetLogger(JSContext ctx)
        {
            return GetRuntime(ctx).GetLogger();
        }

        // unstable interface
        public static int ForEachRuntime(Action<ScriptRuntime> visitor)
        {
            var count = 0;
            try
            {
                _rwlock.EnterReadLock();
                for (int i = 0, len = _runtimeRefs.Count; i < len; ++i)
                {
                    var slot = _runtimeRefs[i];
                    if (slot.target != null)
                    {
                        count++;
                        visitor(slot.target);
                    }
                }
            }
            finally
            {
                _rwlock.ExitReadLock();
            }
            return count;
        }

        public static ObjectCache GetObjectCache(JSRuntime rt)
        {
            return GetRuntime(rt).GetObjectCache();
        }

        public static ObjectCache GetObjectCache(JSContext ctx)
        {
            return GetRuntime(ctx).GetObjectCache();
        }

        public static TypeDB GetTypeDB(JSContext ctx)
        {
            return GetRuntime(ctx).GetTypeDB();
        }

        public static IO.IByteBufferAllocator GetByteBufferAllocator(JSContext ctx)
        {
            return GetRuntime(ctx).GetByteBufferAllocator();
        }

        public static IO.ByteBuffer AllocByteBuffer(JSContext ctx, int size)
        {
            return GetByteBufferAllocator(ctx)?.Alloc(size);
        }

        public static ScriptRuntime GetRuntime(JSContext ctx)
        {
            var rt = JSApi.JS_GetRuntime(ctx);
            return GetRuntime(rt);
        }

        public static ScriptRuntime GetRuntime(JSRuntime rt)
        {
            ScriptRuntime runtime = null;
            var id = (int)JSApi.JS_GetRuntimeOpaque(rt);
            if (id > 0)
            {
                var index = id - 1;
                _rwlock.EnterReadLock();
                var slot = _runtimeRefs[index];
                runtime = slot.target;
                _rwlock.ExitReadLock();
            }
            return runtime;
        }

        public static ScriptContext GetContext(JSContext ctx)
        {
            var rt = JSApi.JS_GetRuntime(ctx);
            return GetRuntime(rt).GetContext(ctx);
        }

        public static ScriptRuntime CreateRuntime()
        {
            _rwlock.EnterWriteLock();
            ScriptRuntimeRef freeEntry;
            int slotIndex;
            if (_freeSlot < 0)
            {
                freeEntry = new ScriptRuntimeRef();
                slotIndex = _runtimeRefs.Count;
                _runtimeRefs.Add(freeEntry);
                freeEntry.next = -1;
            }
            else
            {
                slotIndex = _freeSlot;
                freeEntry = _runtimeRefs[slotIndex];
                _freeSlot = freeEntry.next;
                freeEntry.next = -1;
            }

            var runtime = new ScriptRuntime(slotIndex + 1);
            freeEntry.target = runtime;
            runtime.OnAfterDestroy += OnRuntimeAfterDestroy;
            _rwlock.ExitWriteLock();

            return runtime;
        }

        public static void Shutdown()
        {
            _rwlock.EnterWriteLock();
            var len = _runtimeRefs.Count;
            var copylist = new List<ScriptRuntime>(len);
            for (int i = 0; i < len; ++i)
            {
                var runtime = _runtimeRefs[i].target;
                if (runtime != null)
                {
                    copylist.Add(runtime);
                }
            }
            _rwlock.ExitWriteLock();

            for (int i = 0, count = copylist.Count; i < count; ++i)
            {
                var runtime = copylist[i];
                runtime.Shutdown();
            }
        }

        private static void OnRuntimeAfterDestroy(int runtimeId)
        {
            if (runtimeId <= 0)
            {
                return;
            }
            _rwlock.EnterWriteLock();
            var index = runtimeId - 1;
            var freeEntry = _runtimeRefs[index];
            freeEntry.next = _freeSlot;
            freeEntry.target = null;
            _freeSlot = index;
            _rwlock.ExitWriteLock();
        }
    }
}
