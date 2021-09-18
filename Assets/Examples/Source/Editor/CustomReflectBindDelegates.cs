using System.Reflection;
using System;

namespace Example.Editor
{
    using QuickJS;
    using QuickJS.Native;
    using QuickJS.Binding;

    /// <summary>
    /// [TEMP][ONLY REQUIRED IN ReflectBind mode] need to manually write code for delegates which have by-ref parameters 
    /// </summary>
    public static class CustomReflectBindDelegates
    {
        public static unsafe RT Call<T1, T2, T3, RT>(QuickJS.ScriptDelegate fn, T1 a1, ref T2 a2, out T3 a3)
        {
            if (!fn.isValid)
            {
                throw new Exception("invalid script delegate handle");
            }
            var ctx = fn.ctx;
            var argv = stackalloc JSValue[3];
            argv[0] = ReflectBindValueOp.js_push_tvar<T1>(ctx, a1);
            if (argv[0].IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            argv[1] = JSApi.JS_NewObject(ctx);
            if (argv[1].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                throw new Exception(ctx.GetExceptionString());
            }
            var context = ScriptEngine.GetContext(ctx);
            JSApi.JS_SetProperty(ctx, argv[1], context.GetAtom("value"), ReflectBindValueOp.js_push_tvar<T2>(ctx, a2));
            argv[2] = JSApi.JS_NewObject(ctx);
            if (argv[2].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                JSApi.JS_FreeValue(ctx, argv[1]);
                throw new Exception(ctx.GetExceptionString());
            }
            var rval = fn.Invoke(ctx, 3, argv);
            if (rval.IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                JSApi.JS_FreeValue(ctx, argv[1]);
                JSApi.JS_FreeValue(ctx, argv[2]);
                throw new Exception(ctx.GetExceptionString());
            }
            var refVal1 = Values.js_read_wrap(ctx, argv[1]);
            if (refVal1.IsException())
            {
                JSApi.JS_FreeValue(ctx, rval);
                JSApi.JS_FreeValue(ctx, argv[0]);
                JSApi.JS_FreeValue(ctx, argv[1]);
                JSApi.JS_FreeValue(ctx, argv[2]);
                throw new Exception(ctx.GetExceptionString());
            }
            if (!ReflectBindValueOp.js_get_tvar<T2>(ctx, refVal1, out a2))
            {
                JSApi.JS_FreeValue(ctx, rval);
                JSApi.JS_FreeValue(ctx, argv[0]);
                JSApi.JS_FreeValue(ctx, argv[1]);
                JSApi.JS_FreeValue(ctx, argv[2]);
                JSApi.JS_FreeValue(ctx, refVal1);
                throw new ParameterException(typeof(CustomReflectBindDelegates), "FuncCall", typeof(T2), 1);
            }
            JSApi.JS_FreeValue(ctx, refVal1);
            var refVal2 = Values.js_read_wrap(ctx, argv[2]);
            if (refVal2.IsException())
            {
                JSApi.JS_FreeValue(ctx, rval);
                JSApi.JS_FreeValue(ctx, argv[0]);
                JSApi.JS_FreeValue(ctx, argv[1]);
                JSApi.JS_FreeValue(ctx, argv[2]);
                throw new Exception(ctx.GetExceptionString());
            }
            if (!ReflectBindValueOp.js_get_tvar<T3>(ctx, refVal2, out a3))
            {
                JSApi.JS_FreeValue(ctx, rval);
                JSApi.JS_FreeValue(ctx, argv[0]);
                JSApi.JS_FreeValue(ctx, argv[1]);
                JSApi.JS_FreeValue(ctx, argv[2]);
                JSApi.JS_FreeValue(ctx, refVal2);
                throw new ParameterException(typeof(CustomReflectBindDelegates), "FuncCall", typeof(T3), 2);
            }
            JSApi.JS_FreeValue(ctx, refVal2);
            RT ret0;
            var succ = ReflectBindValueOp.js_get_tvar<RT>(ctx, rval, out ret0);
            JSApi.JS_FreeValue(ctx, rval);
            JSApi.JS_FreeValue(ctx, argv[0]);
            JSApi.JS_FreeValue(ctx, argv[1]);
            JSApi.JS_FreeValue(ctx, argv[2]);
            if (succ)
            {
                return ret0;
            }
            else
            {
                throw new Exception("js exception caught");
            }
        }
    }
}