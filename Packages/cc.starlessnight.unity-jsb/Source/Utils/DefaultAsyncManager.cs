using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace QuickJS.Utils
{
    using Native;
    using Utils;

    public class DefaultAsyncManager : IAsyncManager
    {
        private class JSTaskCompletionArgs
        {
            public System.Threading.Tasks.Task task;
            public SafeRelease safeRelease;

            public JSTaskCompletionArgs(System.Threading.Tasks.Task task, SafeRelease safeRelease)
            {
                this.task = task;
                this.safeRelease = safeRelease;
            }
        }

        private int _mainThreadId;

#if !JSB_UNITYLESS
        private Unity.UnityCoroutineContext _mb = null;
#endif

        public DefaultAsyncManager()
        {

        }

        public void Initialize(int mainThreadId)
        {
            _mainThreadId = mainThreadId;
        }

#if !JSB_UNITYLESS
        private void GetUnityContext()
        {
            if (_mb == null && _mainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                var container = new UnityEngine.GameObject("JSRuntimeContainer");
                container.hideFlags = UnityEngine.HideFlags.HideInHierarchy;
                UnityEngine.Object.DontDestroyOnLoad(container);
                _mb = container.AddComponent<Unity.UnityCoroutineContext>();
            }
        }
#endif

        public void Destroy()
        {
#if !JSB_UNITYLESS
            if (_mb != null)
            {
                UnityEngine.Object.DestroyImmediate(_mb.gameObject);
                _mb = null;
            }
#endif
        }

        // return promise
        public JSValue Yield(ScriptContext context, object awaitObject)
        {
            var ctx = (JSContext)context;
            if (awaitObject is System.Threading.Tasks.Task)
            {
                var resolving_funcs = new[] { JSApi.JS_UNDEFINED, JSApi.JS_UNDEFINED };
                var promise = JSApi.JS_NewPromiseCapability(ctx, resolving_funcs);
                var safeRelease = new SafeRelease(context).Append(resolving_funcs);
                var task = awaitObject as System.Threading.Tasks.Task;
                var runtime = context.GetRuntime();

#if JSB_COMPATIBLE
                task.ContinueWith(antecedent => runtime.EnqueueAction(_OnTaskCompleted, new JSTaskCompletionArgs(task, safeRelease)));
#else
                task.GetAwaiter().OnCompleted(() =>
                {
                    runtime.EnqueueAction(_OnTaskCompleted, new JSTaskCompletionArgs(task, safeRelease));
                });
#endif
                return promise;
            }
#if !JSB_UNITYLESS
            else if (awaitObject is IEnumerator)
            {
                //TODO: 合并 IEnumerator 和 YieldInstruction 写法
                GetUnityContext();

                if (_mb == null)
                {
                    return ctx.ThrowInternalError("no MonoBehaviour for Coroutines");
                }

                if (_mainThreadId != Thread.CurrentThread.ManagedThreadId)
                {
                    return ctx.ThrowInternalError("not supported on background thread");
                }

                var resolving_funcs = new[] { JSApi.JS_UNDEFINED, JSApi.JS_UNDEFINED };
                var promise = JSApi.JS_NewPromiseCapability(ctx, resolving_funcs);

                _mb.StartCoroutine(_Pending(awaitObject as IEnumerator, context, resolving_funcs));
                return promise;
            }
            else
            {
                GetUnityContext();

                if (_mb == null)
                {
                    return ctx.ThrowInternalError("no MonoBehaviour for Coroutines");
                }

                if (_mainThreadId != Thread.CurrentThread.ManagedThreadId)
                {
                    return ctx.ThrowInternalError("not supported on background thread");
                }

                var resolving_funcs = new[] { JSApi.JS_UNDEFINED, JSApi.JS_UNDEFINED };
                var promise = JSApi.JS_NewPromiseCapability(ctx, resolving_funcs);

                _mb.StartCoroutine(_Pending(awaitObject as UnityEngine.YieldInstruction, context, resolving_funcs));
                return promise;
            }
#else 
            return ctx.ThrowInternalError("not supported await object");
#endif // !JSB_UNITYLESS
        }

        private static void _OnTaskCompleted(ScriptRuntime runtime, JSAction action)
        {
            if (!runtime.isValid || !runtime.isRunning)
            {
                return;
            }
            var context = runtime.GetMainContext();
            var logger = runtime.GetLogger();
            var args = (JSTaskCompletionArgs)action.args;
            var task = args.task;
            var safeRelease = args.safeRelease;

            if (!safeRelease.isValid)
            {
                logger?.Write(LogLevel.Error, "pormise func has already been released");
                return;
            }

            object result = null;
            var taskType = task.GetType();

            if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                try
                {
                    result = taskType.GetProperty("Result").GetValue(task, null);
                }
                catch (Exception exception)
                {
                    logger?.WriteException(exception);
                }
            }

            var ctx = (JSContext)context;
            var backVal = Binding.Values.js_push_var(ctx, result);
            if (backVal.IsException())
            {
                ctx.print_exception();
                safeRelease.Release();
                return;
            }

            var argv = new[] { backVal };
            var rval = JSApi.JS_Call(ctx, safeRelease[0], JSApi.JS_UNDEFINED, 1, argv);
            JSApi.JS_FreeValue(ctx, backVal);
            if (rval.IsException())
            {
                ctx.print_exception();
                safeRelease.Release();
                return;
            }

            JSApi.JS_FreeValue(ctx, rval);
            safeRelease.Release();

            context.GetRuntime().ExecutePendingJob();
        }

#if !JSB_UNITYLESS
        private IEnumerator _Pending(UnityEngine.YieldInstruction instruction, ScriptContext context, JSValue[] resolving_funcs)
        {
            var safeRelease = new SafeRelease(context).Append(resolving_funcs);

            yield return instruction;

            if (!context.IsValid())
            {
                yield break;
            }

            var ctx = (JSContext)context;
            var backVal = Binding.Values.js_push_classvalue(ctx, instruction);
            if (backVal.IsException())
            {
                ctx.print_exception();
                safeRelease.Release();
                yield break;
            }

            var argv = new[] { backVal };
            var rval = JSApi.JS_Call(ctx, resolving_funcs[0], JSApi.JS_UNDEFINED, 1, argv);
            JSApi.JS_FreeValue(ctx, backVal);
            if (rval.IsException())
            {
                ctx.print_exception();
                safeRelease.Release();
                yield break;
            }

            JSApi.JS_FreeValue(ctx, rval);
            safeRelease.Release();

            context.GetRuntime().ExecutePendingJob();
        }

        private IEnumerator _Pending(IEnumerator enumerator, ScriptContext context, JSValue[] resolving_funcs)
        {
            var safeRelease = new SafeRelease(context).Append(resolving_funcs);

            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;

                if (current is UnityEngine.YieldInstruction)
                {
                    yield return current;
                }

                yield return null;
            }

            if (!context.IsValid())
            {
                yield break;
            }

            var ctx = (JSContext)context;
            var backVal = Binding.Values.js_push_var(ctx, enumerator.Current);
            if (backVal.IsException())
            {
                ctx.print_exception();
                safeRelease.Release();
                yield break;
            }

            var argv = new[] { backVal };
            var rval = JSApi.JS_Call(ctx, resolving_funcs[0], JSApi.JS_UNDEFINED, 1, argv);
            JSApi.JS_FreeValue(ctx, backVal);
            if (rval.IsException())
            {
                ctx.print_exception();
                safeRelease.Release();
                yield break;
            }

            JSApi.JS_FreeValue(ctx, rval);
            safeRelease.Release();

            context.GetRuntime().ExecutePendingJob();
        }
#endif // !JSB_UNITYLESS
    }
}
