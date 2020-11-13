using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace QuickJS.Unity
{
    using UnityEngine;
    using Native;
    using Utils;

    public class DefaultAsyncManager : IAsyncManager
    {
        private int _mainThreadId;
        private UnityCoroutineContext _mb = null;

        public DefaultAsyncManager()
        {

        }

        public void Initialize(int mainThreadId)
        {
            _mainThreadId = mainThreadId;
        }

        private void GetUnityContext()
        {
            if (_mb == null && _mainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                var container = new UnityEngine.GameObject("JSRuntimeContainer");
                container.hideFlags = UnityEngine.HideFlags.HideInHierarchy;
                UnityEngine.Object.DontDestroyOnLoad(container);
                _mb = container.AddComponent<UnityCoroutineContext>();
            }
        }

        public void Destroy()
        {
            if (_mb != null)
            {
                UnityEngine.Object.DestroyImmediate(_mb.gameObject);
                _mb = null;
            }
        }

        // return promise
        public JSValue Yield(ScriptContext context, object awaitObject)
        {
            if (awaitObject is System.Threading.Tasks.Task)
            {
                var resolving_funcs = new[] { JSApi.JS_UNDEFINED, JSApi.JS_UNDEFINED };
                var promise = JSApi.JS_NewPromiseCapability(context, resolving_funcs);

                _Pending(awaitObject as System.Threading.Tasks.Task, context, resolving_funcs);
                return promise;
            }
            else if (awaitObject is IEnumerator)
            {
                //TODO: 合并 IEnumerator 和 YieldInstruction 写法
                GetUnityContext();

                if (_mb == null)
                {
                    return JSApi.JS_ThrowInternalError(context, "no MonoBehaviour for Coroutines");
                }

                if (_mainThreadId != Thread.CurrentThread.ManagedThreadId)
                {
                    return JSApi.JS_ThrowInternalError(context, "not supported on background thread");
                }

                var resolving_funcs = new[] { JSApi.JS_UNDEFINED, JSApi.JS_UNDEFINED };
                var promise = JSApi.JS_NewPromiseCapability(context, resolving_funcs);

                _mb.StartCoroutine(_Pending(awaitObject as IEnumerator, context, resolving_funcs));
                return promise;
            }
            else
            {
                GetUnityContext();
                
                if (_mb == null)
                {
                    return JSApi.JS_ThrowInternalError(context, "no MonoBehaviour for Coroutines");
                }

                if (_mainThreadId != Thread.CurrentThread.ManagedThreadId)
                {
                    return JSApi.JS_ThrowInternalError(context, "not supported on background thread");
                }

                var resolving_funcs = new[] { JSApi.JS_UNDEFINED, JSApi.JS_UNDEFINED };
                var promise = JSApi.JS_NewPromiseCapability(context, resolving_funcs);

                _mb.StartCoroutine(_Pending(awaitObject as YieldInstruction, context, resolving_funcs));
                return promise;
            }

        }

        private async void _Pending(Task task, ScriptContext context, JSValue[] resolving_funcs)
        {
            var safeRelease = new SafeRelease(context).Append(resolving_funcs);

            //TODO: 修改任务等待
            await task;

            if (!context.IsValid())
            {
                return;
            }

            object result = null;
            var taskType = task.GetType();
            if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                try
                {
                    result = taskType.GetProperty("Result").GetValue(task);
                }
                catch (Exception exception)
                {
                    context.GetLogger()?.WriteException(exception);
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
            var rval = JSApi.JS_Call(ctx, resolving_funcs[0], JSApi.JS_UNDEFINED, 1, argv);
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

        private IEnumerator _Pending(YieldInstruction instruction, ScriptContext context, JSValue[] resolving_funcs)
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

                if (current is YieldInstruction)
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
    }
}
