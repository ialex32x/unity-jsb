using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using QuickJS.Native;
using UnityEngine;

namespace QuickJS.Utils
{
    public class CoroutineManager : MonoBehaviour
    {
        //TODO: 临时代码
        public async void Load(ScriptContext context, string src)
        {
            var request = WebRequest.CreateHttp(src);
            request.Method = "GET";
            var rsp = await request.GetResponseAsync() as HttpWebResponse;
            var reader = new StreamReader(rsp.GetResponseStream());
            var reseponseText = await reader.ReadToEndAsync();
            if (!context.IsValid())
            {
                return;
            }
            context.EvalSourceFree(reseponseText, src);
        }

        // return promise
        public JSValue Yield(ScriptContext context, object awaitObject)
        {
            var resolving_funcs = new[] { JSApi.JS_UNDEFINED, JSApi.JS_UNDEFINED };
            var promise = JSApi.JS_NewPromiseCapability(context, resolving_funcs);

            if (awaitObject is System.Threading.Tasks.Task)
            {
                _Pending(awaitObject as System.Threading.Tasks.Task, context, resolving_funcs);
            }
            else if (awaitObject is IEnumerator)
            {
                StartCoroutine(_Pending(awaitObject as IEnumerator, context, resolving_funcs));
            }
            else
            {
                StartCoroutine(_Pending(awaitObject as YieldInstruction, context, resolving_funcs));
            }

            return promise;
        }

        private async void _Pending(Task task, ScriptContext context, JSValue[] resolving_funcs)
        {
            var safeRelease = new SafeRelease(context).Append(resolving_funcs);

            await task;

            if (!context.IsValid())
            {
                return;
            }

            var ctx = (JSContext)context;
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
                    context.GetLogger()?.Error(exception);
                }
            }

            var backVal = Binding.Values.js_push_classvalue(ctx, result);
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
            var backVal = Binding.Values.js_push_classvalue(ctx, enumerator.Current);
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
