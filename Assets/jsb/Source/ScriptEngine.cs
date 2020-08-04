using System;
using System.Collections.Generic;
using AOT;
using QuickJS.Native;
using QuickJS.Utils;

namespace QuickJS
{
    using UnityEngine;

    // 暂时只做单实例
    public class ScriptEngine
    {
        // private class ScriptRuntimeRef
        // {
        //     public int next;
        //     public ScriptRuntime target;
        // }

        public const uint VERSION = 0x723;

        private static ScriptRuntime _mainRuntime;
        // private static List<ScriptRuntimeRef> _runtimeRefs = new List<ScriptRuntimeRef>();

        public static IScriptLogger GetLogger(JSContext ctx)
        {
            return _mainRuntime.GetLogger();
        }

        // unstable interface
        public static ScriptRuntime GetRuntime()
        {
            return _mainRuntime;
        }

        public static ObjectCache GetObjectCache(JSRuntime rt)
        {
            return _mainRuntime.GetObjectCache();
        }

        public static ObjectCache GetObjectCache(JSContext ctx)
        {
            return GetRuntime(ctx)?.GetObjectCache();
        }

        public static TypeDB GetTypeDB(JSContext ctx)
        {
            return GetRuntime(ctx)?.GetTypeDB();
        }

        public static IO.IByteBufferAllocator GetByteBufferAllocator(JSContext ctx)
        {
            return GetRuntime(ctx)?.GetByteBufferAllocator();
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
            return _mainRuntime;
        }

        public static ScriptContext GetContext(JSContext ctx)
        {
            return _mainRuntime.GetContext(ctx);
        }

        public static ScriptRuntime CreateRuntime()
        {
            _mainRuntime = new ScriptRuntime(1);
            _mainRuntime.OnAfterDestroy += OnRuntimeAfterDestroy;
            return _mainRuntime;
        }

        //TODO: destroy all runtime
        public static void Destroy()
        {
            if (_mainRuntime != null)
            {
                _mainRuntime.Destroy();
            }
        }

        private static void OnRuntimeAfterDestroy(ScriptRuntime runtime)
        {
            _mainRuntime = null;
        }
    }
}
