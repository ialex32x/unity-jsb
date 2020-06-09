using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using QuickJS.Native;
using QuickJS.Utils;
using UnityEngine;

namespace QuickJS
{
    public class ScriptContext
    {
        public event Action<ScriptContext> OnDestroy;

        private ScriptRuntime _runtime;
        private JSContext _ctx;
        private AtomCache _atoms;

        public ScriptContext(ScriptRuntime runtime)
        {
            _runtime = runtime;
            _ctx = JSApi.JS_NewContext(_runtime);
            _atoms = new AtomCache(_ctx);
        }

        public TimerManager GetTimerManager()
        {
            return _runtime.GetTimerManager();
        }

        public IScriptLogger GetLogger()
        {
            return _runtime.GetLogger();
        }

        public ScriptRuntime GetRuntime()
        {
            return _runtime;
        }

        public bool IsContext(JSContext ctx)
        {
            return ctx.IsContext(_ctx);
        }

        public JSAtom GetAtom(string name)
        {
            return _atoms.GetAtom(name);
        }

        public void Destroy()
        {
            try
            {
                OnDestroy?.Invoke(this);
            }
            catch (Exception e)
            {
                _runtime.GetLogger().Error(e);
            }
            _atoms.Clear();
            JSApi.JS_FreeContext(_ctx);
            _ctx = JSContext.Null;
        }

        public void AddIntrinsicOperators()
        {
            JSApi.JS_AddIntrinsicOperators(_ctx);
        }

        public void FreeValue(JSValue value)
        {
            _runtime.FreeValue(value);
        }

        public void FreeValues(JSValue[] values)
        {
            _runtime.FreeValues(values);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JSValue GetGlobalObject()
        {
            return JSApi.JS_GetGlobalObject(_ctx);
        }

        #region Builtins

        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        private static JSValue _print(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv, int magic)
        {
            int i;
            var sb = new StringBuilder();
            size_t len;

            for (i = 0; i < argc; i++)
            {
                if (i != 0)
                {
                    sb.Append(' ');
                }

                var pstr = JSApi.JS_ToCStringLen(ctx, out len, argv[i]);
                if (pstr == IntPtr.Zero)
                {
                    return JSApi.JS_EXCEPTION;
                }

                var str = JSApi.GetString(pstr, len);
                if (str != null)
                {
                    sb.Append(str);
                }

                JSApi.JS_FreeCString(ctx, pstr);
            }

            sb.AppendLine();
            var logger = ScriptEngine.GetLogger(ctx);
            logger.ScriptWrite((LogLevel)magic, sb.ToString());
            return JSApi.JS_UNDEFINED;
        }

        #endregion

        public void RegisterBuiltins()
        {
            var ctx = (JSContext)this;
            var global_object = JSApi.JS_GetGlobalObject(ctx);
            {
                var require_func_obj = JSApi.JSB_NewCFunctionMagic(ctx, ScriptRuntime.module_require, GetAtom("require"), 1, JSCFunctionEnum.JS_CFUNC_generic_magic, -1);
                JSApi.JS_SetPropertyStr(ctx, global_object, "require", require_func_obj);

                JSApi.JS_SetPropertyStr(ctx, global_object, "print", JSApi.JS_NewCFunctionMagic(ctx, _print, "print", 1, JSCFunctionEnum.JS_CFUNC_generic_magic, 0));
                var console = JSApi.JS_NewObject(ctx);
                {
                    JSApi.JS_SetPropertyStr(ctx, console, "log", JSApi.JS_NewCFunctionMagic(ctx, _print, "log", 1, JSCFunctionEnum.JS_CFUNC_generic_magic, 0));
                    JSApi.JS_SetPropertyStr(ctx, console, "info", JSApi.JS_NewCFunctionMagic(ctx, _print, "info", 1, JSCFunctionEnum.JS_CFUNC_generic_magic, 0));
                    JSApi.JS_SetPropertyStr(ctx, console, "debug", JSApi.JS_NewCFunctionMagic(ctx, _print, "debug", 1, JSCFunctionEnum.JS_CFUNC_generic_magic, 0));
                    JSApi.JS_SetPropertyStr(ctx, console, "warn", JSApi.JS_NewCFunctionMagic(ctx, _print, "warn", 1, JSCFunctionEnum.JS_CFUNC_generic_magic, 1));
                    JSApi.JS_SetPropertyStr(ctx, console, "error", JSApi.JS_NewCFunctionMagic(ctx, _print, "error", 1, JSCFunctionEnum.JS_CFUNC_generic_magic, 2));
                    JSApi.JS_SetPropertyStr(ctx, console, "assert", JSApi.JS_NewCFunctionMagic(ctx, _print, "assert", 1, JSCFunctionEnum.JS_CFUNC_generic_magic, 3));
                }
                JSApi.JS_SetPropertyStr(ctx, global_object, "console", console);
            }
            JSApi.JS_FreeValue(ctx, global_object);
        }

        public static implicit operator JSContext(ScriptContext sc)
        {
            return sc._ctx;
        }
    }
}