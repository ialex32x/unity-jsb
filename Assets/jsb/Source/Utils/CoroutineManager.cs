using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuickJS.Native;
using UnityEngine;

namespace QuickJS.Utils
{
    public class CoroutineManager : MonoBehaviour
    {
        // return promise
        public JSValue Yield(ScriptContext context, YieldInstruction instruction)
        {
            var resolving_funcs = new[] { JSApi.JS_UNDEFINED, JSApi.JS_UNDEFINED };
            var promise = JSApi.JS_NewPromiseCapability(context, resolving_funcs);
            StartCoroutine(_Pending(instruction, context, resolving_funcs));
            return promise;
        }

        public JSValue Yield(ScriptContext context, Task task)
        {
            var resolving_funcs = new[] { JSApi.JS_UNDEFINED, JSApi.JS_UNDEFINED };
            var promise = JSApi.JS_NewPromiseCapability(context, resolving_funcs);
            _Pending(task, context, resolving_funcs);
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
                    context.GetLogger().Error(exception);
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
    }
}
