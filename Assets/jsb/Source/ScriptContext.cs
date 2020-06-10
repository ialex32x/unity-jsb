using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using QuickJS.Binding;
using QuickJS.Native;
using QuickJS.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QuickJS
{
    public class ScriptContext
    {
        public event Action<ScriptContext> OnDestroy;

        private ScriptRuntime _runtime;
        private JSContext _ctx;
        private AtomCache _atoms;
        private JSValue _moduleCache; // commonjs module cache
        private JSValue _require; // require function object 
        private CoroutineManager _coroutines;
        private bool _isValid;

        public ScriptContext(ScriptRuntime runtime)
        {
            _isValid = true;
            _runtime = runtime;
            _ctx = JSApi.JS_NewContext(_runtime);
            _atoms = new AtomCache(_ctx);
            _moduleCache = JSApi.JS_NewObject(_ctx);
        }

        public bool IsValid()
        {
            return _isValid;
        }

        public JSValue Yield(YieldInstruction yieldInstruction)
        {
            if (_isValid)
            {
                if (_coroutines == null)
                {
                    var go = _runtime.GetContainer();
                    if (go != null)
                    {
                        _coroutines = go.AddComponent<CoroutineManager>();
                    }
                }
            }

            if (_coroutines != null)
            {
                return _coroutines.Yield(this, yieldInstruction);
            }

            return JSApi.JS_UNDEFINED;
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

        //NOTE: 返回值不需要释放, context 销毁时会自动释放所管理的 Atom
        public JSAtom GetAtom(string name)
        {
            return _atoms.GetAtom(name);
        }

        public void Destroy()
        {
            _isValid = false;
            
            try
            {
                OnDestroy?.Invoke(this);
            }
            catch (Exception e)
            {
                _runtime.GetLogger().Error(e);
            }
            _atoms.Clear();
            JSApi.JS_FreeValue(_ctx, _moduleCache);
            JSApi.JS_FreeValue(_ctx, _require);
            JSApi.JS_FreeContext(_ctx);
            
            if (_coroutines != null)
            {
                Object.DestroyImmediate(_coroutines);
                _coroutines = null;
            }
            
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

        //NOTE: 返回值需要调用者 free 
        public JSValue _get_commonjs_module(string module_id)
        {
            var prop = GetAtom(module_id);
            return JSApi.JS_GetProperty(_ctx, _moduleCache, prop);
        }

        public void _new_commonjs_module(string module_id, JSValue module_obj)
        {
            var prop = GetAtom(module_id);
            JSApi.JS_SetProperty(_ctx, _moduleCache, prop, module_obj);
            JSApi.JS_SetProperty(_ctx, module_obj, GetAtom("cache"), JSApi.JS_DupValue(_ctx, _moduleCache));
        }

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

        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue yield_func(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc < 1)
            {
                return JSApi.JS_ThrowInternalError(ctx, "invalid arg0: YieldInstruction");
            }
            YieldInstruction yieldInstruction;
            if (!Values.js_get_classvalue(ctx, argv[0], out yieldInstruction))
            {
                return JSApi.JS_ThrowInternalError(ctx, "invalid arg0: YieldInstruction");
            }

            var context = ScriptEngine.GetContext(ctx);
            return context.Yield(yieldInstruction);
        }

        public static void Bind(TypeRegister register)
        {
            var ns = register.CreateNamespace("jsb");
            ns.AddFunction("Yield", yield_func, 1);
            ns.Close();
        }

        public void EvalMain(string source, string fileName)
        {
            JSApi.JS_SetProperty(_ctx, _require, GetAtom("moduleId"), JSApi.JS_NewString(_ctx, fileName));
            var jsValue = JSApi.JS_Eval(_ctx, source, fileName);
            if (JSApi.JS_IsException(jsValue))
            {
                _ctx.print_exception();
            }

            JSApi.JS_FreeValue(_ctx, jsValue);
        }

        public void EvalSource(string source, string fileName)
        {
            var jsValue = JSApi.JS_Eval(_ctx, source, fileName);
            if (JSApi.JS_IsException(jsValue))
            {
                _ctx.print_exception();
            }

            JSApi.JS_FreeValue(_ctx, jsValue);
        }

        public void RegisterBuiltins()
        {
            var ctx = (JSContext)this;
            var global_object = JSApi.JS_GetGlobalObject(ctx);
            {
                _require = JSApi.JSB_NewCFunction(ctx, ScriptRuntime.module_require, GetAtom("require"), 1, JSCFunctionEnum.JS_CFUNC_generic, 0);
                JSApi.JS_SetProperty(ctx, _require, GetAtom("moduleId"), JSApi.JS_NewString(ctx, ""));
                JSApi.JS_SetProperty(ctx, _require, GetAtom("cache"), JSApi.JS_DupValue(ctx, _moduleCache));
                JSApi.JS_SetProperty(ctx, global_object, GetAtom("require"), JSApi.JS_DupValue(ctx, _require));

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