using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;
    using QuickJS.Native;

    public unsafe class DelegateReflectBindMethods
    {
        public static void ActionCall(ScriptDelegate fn)
        {
            var ctx = fn.ctx;
            var rval = fn.Invoke(ctx);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            JSApi.JS_FreeValue(ctx, rval);
        }

        public static void ActionCall<T1>(ScriptDelegate fn, T1 a1)
        {
            var ctx = fn.ctx;
            var argv = stackalloc JSValue[1];
            argv[0] = ReflectBindValueOp.js_push_tvar<T1>(ctx, a1);
            if (argv[0].IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            var rval = fn.Invoke(ctx, 1, argv);
            JSApi.JS_FreeValue(ctx, argv[0]);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            JSApi.JS_FreeValue(ctx, rval);
        }

        public static void ActionCall<T1, T2>(ScriptDelegate fn, T1 a1, T2 a2)
        {
            var ctx = fn.ctx;
            var argv = stackalloc JSValue[2];
            argv[0] = ReflectBindValueOp.js_push_tvar<T1>(ctx, a1);
            if (argv[0].IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            argv[1] = ReflectBindValueOp.js_push_tvar<T2>(ctx, a2);
            if (argv[1].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                throw new Exception(ctx.GetExceptionString());
            }
            var rval = fn.Invoke(ctx, 2, argv);
            JSApi.JS_FreeValue(ctx, argv[0]);
            JSApi.JS_FreeValue(ctx, argv[1]);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            JSApi.JS_FreeValue(ctx, rval);
        }

        public static void ActionCall<T1, T2, T3>(ScriptDelegate fn, T1 a1, T2 a2, T3 a3)
        {
            var ctx = fn.ctx;
            var argv = stackalloc JSValue[3];
            argv[0] = ReflectBindValueOp.js_push_tvar<T1>(ctx, a1);
            if (argv[0].IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            argv[1] = ReflectBindValueOp.js_push_tvar<T2>(ctx, a2);
            if (argv[1].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                throw new Exception(ctx.GetExceptionString());
            }
            argv[2] = ReflectBindValueOp.js_push_tvar<T2>(ctx, a2);
            if (argv[2].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                JSApi.JS_FreeValue(ctx, argv[1]);
                throw new Exception(ctx.GetExceptionString());
            }
            var rval = fn.Invoke(ctx, 3, argv);
            JSApi.JS_FreeValue(ctx, argv[0]);
            JSApi.JS_FreeValue(ctx, argv[1]);
            JSApi.JS_FreeValue(ctx, argv[2]);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            JSApi.JS_FreeValue(ctx, rval);
        }

        public static RT FuncCall<RT>(ScriptDelegate fn)
        {
            var ctx = fn.ctx;
            var rval = fn.Invoke(ctx);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            RT ret0;
            var succ = ReflectBindValueOp.js_get_tvar<RT>(ctx, rval, out ret0);
            JSApi.JS_FreeValue(ctx, rval);
            if (succ)
            {
                return ret0;
            }
            else
            {
                throw new Exception("js exception caught");
            }
        }

        public static RT FuncCall<T1, RT>(ScriptDelegate fn, T1 a1)
        {
            var ctx = fn.ctx;
            var argv = stackalloc JSValue[1];
            argv[0] = ReflectBindValueOp.js_push_tvar<T1>(ctx, a1);
            if (argv[0].IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            var rval = fn.Invoke(ctx, 1, argv);
            if (rval.IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                throw new Exception(ctx.GetExceptionString());
            }
            RT ret0;
            var succ = ReflectBindValueOp.js_get_tvar<RT>(ctx, rval, out ret0);
            JSApi.JS_FreeValue(ctx, rval);
            JSApi.JS_FreeValue(ctx, argv[0]);
            if (succ)
            {
                return ret0;
            }
            else
            {
                throw new Exception("js exception caught");
            }
        }

        public static RT FuncCall<T1, T2, RT>(ScriptDelegate fn, T1 a1, T2 a2)
        {
            var ctx = fn.ctx;
            var argv = stackalloc JSValue[2];
            argv[0] = ReflectBindValueOp.js_push_tvar<T1>(ctx, a1);
            if (argv[0].IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            argv[1] = ReflectBindValueOp.js_push_tvar<T2>(ctx, a2);
            if (argv[1].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                throw new Exception(ctx.GetExceptionString());
            }
            var rval = fn.Invoke(ctx, 2, argv);
            JSApi.JS_FreeValue(ctx, argv[0]);
            JSApi.JS_FreeValue(ctx, argv[1]);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            RT ret0;
            var succ = ReflectBindValueOp.js_get_tvar<RT>(ctx, rval, out ret0);
            JSApi.JS_FreeValue(ctx, rval);
            if (succ)
            {
                return ret0;
            }
            else
            {
                throw new Exception("js exception caught");
            }
        }

        public static RT FuncCall<T1, T2, T3, RT>(ScriptDelegate fn, T1 a1, T2 a2, T3 a3)
        {
            var ctx = fn.ctx;
            var argv = stackalloc JSValue[3];
            argv[0] = ReflectBindValueOp.js_push_tvar<T1>(ctx, a1);
            if (argv[0].IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            argv[1] = ReflectBindValueOp.js_push_tvar<T2>(ctx, a2);
            if (argv[1].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                throw new Exception(ctx.GetExceptionString());
            }
            argv[2] = ReflectBindValueOp.js_push_tvar<T3>(ctx, a3);
            if (argv[2].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                JSApi.JS_FreeValue(ctx, argv[1]);
                throw new Exception(ctx.GetExceptionString());
            }
            var rval = fn.Invoke(ctx, 3, argv);
            JSApi.JS_FreeValue(ctx, argv[0]);
            JSApi.JS_FreeValue(ctx, argv[1]);
            JSApi.JS_FreeValue(ctx, argv[2]);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            RT ret0;
            var succ = ReflectBindValueOp.js_get_tvar<RT>(ctx, rval, out ret0);
            JSApi.JS_FreeValue(ctx, rval);
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
