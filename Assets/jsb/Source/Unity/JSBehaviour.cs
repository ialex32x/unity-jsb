using System;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using Native;
    using UnityEngine;

    public class JSBehaviour : MonoBehaviour
    {
        private string _scriptTypeName;

        public string scriptTypeName
        {
            get { return _scriptTypeName; }
        }

        private bool _released;
        private JSContext _ctx;
        private JSValue _this_obj;

        private bool _updateValid;
        private JSValue _updateFunc;

        private bool _lateUpdateValid;
        private JSValue _lateUpdateFunc;

        private bool _fixedUpdateValid;
        private JSValue _fixedUpdateFunc;

        private bool _startValid;
        private JSValue _startFunc;

        private bool _onEnableValid;
        private JSValue _onEnableFunc;

        private bool _onDisableValid;
        private JSValue _onDisableFunc;

        private bool _onApplicationFocusValid;
        private JSValue _onApplicationFocusFunc;

        private bool _onApplicationPauseValid;
        private JSValue _onApplicationPauseFunc;

        private bool _onApplicationQuitValid;
        private JSValue _onApplicationQuitFunc;

        private bool _onDestroyValid;
        private JSValue _onDestroyFunc;

#if UNITY_EDITOR
        private bool _onDrawGizmosValid;
        private JSValue _onDrawGizmosFunc;
#endif

        public int IsInstanceOf(JSValue ctor)
        {
            if (_released)
            {
                return 0;
            }
            return JSApi.JS_IsInstanceOf(_ctx, _this_obj, ctor);
        }

        public JSValue CloneValue()
        {
            if (_released)
            {
                return JSApi.JS_UNDEFINED;
            }
            return JSApi.JS_DupValue(_ctx, _this_obj);
        }

        public unsafe void ForEachProperty(Action<JSContext, JSAtom, JSValue> callback)
        {
            if (_released)
            {
                return;
            }
            JSPropertyEnum* ptab;
            uint plen;
            if (JSApi.JS_GetOwnPropertyNames(_ctx, out ptab, out plen, _this_obj, JSGPNFlags.JS_GPN_STRING_MASK) < 0)
            {
                // failed
                return;
            }

            for (var i = 0; i < plen; i++)
            {
                var prop = JSApi.JS_GetProperty(_ctx, _this_obj, ptab[i].atom);
                try
                {
                    callback(_ctx, ptab[i].atom, prop);
                }
                catch (Exception)
                {
                }
                JSApi.JS_FreeValue(_ctx, prop);
            }

            for (var i = 0; i < plen; i++)
            {
                JSApi.JS_FreeAtom(_ctx, ptab[i].atom);
            }
        }

        public void SetBridge(JSContext ctx, JSValue this_obj, JSValue ctor)
        {
            var context = ScriptEngine.GetContext(ctx);
            if (context == null)
            {
                return;
            }

            context.OnDestroy += OnContextDestroy;
            _released = false;
            _ctx = ctx;
            _this_obj = JSApi.JS_DupValue(ctx, this_obj);
            var nameProp = JSApi.JS_GetProperty(ctx, ctor, JSApi.JS_ATOM_name);
            if (nameProp.IsException())
            {
                _scriptTypeName = "Unknown";
            }
            else
            {
                _scriptTypeName = JSApi.GetString(ctx, nameProp);
                JSApi.JS_FreeValue(ctx, nameProp);
            }

            _updateFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("Update"));
            _updateValid = JSApi.JS_IsFunction(ctx, _updateFunc) == 1;

            _lateUpdateFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("LateUpdate"));
            _lateUpdateValid = JSApi.JS_IsFunction(ctx, _lateUpdateFunc) == 1;

            _fixedUpdateFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("FixedUpdate"));
            _fixedUpdateValid = JSApi.JS_IsFunction(ctx, _fixedUpdateFunc) == 1;

            _startFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("Start"));
            _startValid = JSApi.JS_IsFunction(ctx, _startFunc) == 1;

            _onEnableFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnEnable"));
            _onEnableValid = JSApi.JS_IsFunction(ctx, _onEnableFunc) == 1;

            _onDisableFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnDisable"));
            _onDisableValid = JSApi.JS_IsFunction(ctx, _onDisableFunc) == 1;

            _onApplicationFocusFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnApplicationFocus"));
            _onApplicationFocusValid = JSApi.JS_IsFunction(ctx, _onApplicationFocusFunc) == 1;

#if UNITY_EDITOR
            _onDrawGizmosFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnDrawGizmos"));
            _onDrawGizmosValid = JSApi.JS_IsFunction(ctx, _onDrawGizmosFunc) == 1;
#endif

            _onApplicationPauseFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnApplicationPause"));
            _onApplicationPauseValid = JSApi.JS_IsFunction(ctx, _onApplicationPauseFunc) == 1;

            _onApplicationQuitFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnApplicationQuit"));
            _onApplicationQuitValid = JSApi.JS_IsFunction(ctx, _onApplicationQuitFunc) == 1;

            _onDestroyFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnDestroy"));
            _onDestroyValid = JSApi.JS_IsFunction(ctx, _onDestroyFunc) == 1;

            var awake_obj = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("Awake"));

            Call(awake_obj);
            JSApi.JS_FreeValue(_ctx, awake_obj);
            if (enabled && _onEnableValid)
            {
                Call(_onEnableFunc);
            }
        }

        private void Call(JSValue func_obj)
        {
            if (JSApi.JS_IsFunction(_ctx, func_obj) == 1)
            {
                var rval = JSApi.JS_Call(_ctx, func_obj, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }

        private void OnContextDestroy(ScriptContext context)
        {
            Release();
        }

        void Release()
        {
            if (_released)
            {
                return;
            }
            _released = true;
            JSApi.JS_FreeValue(_ctx, _updateFunc);
            _updateValid = false;
            JSApi.JS_FreeValue(_ctx, _lateUpdateFunc);
            _lateUpdateValid = false;
            JSApi.JS_FreeValue(_ctx, _fixedUpdateFunc);
            _fixedUpdateValid = false;
            JSApi.JS_FreeValue(_ctx, _startFunc);
            _startValid = false;
            JSApi.JS_FreeValue(_ctx, _onEnableFunc);
            _onEnableValid = false;
            JSApi.JS_FreeValue(_ctx, _onDisableFunc);
            _onDisableValid = false;
            JSApi.JS_FreeValue(_ctx, _onApplicationFocusFunc);
            _onApplicationFocusValid = false;
#if UNITY_EDITOR
            JSApi.JS_FreeValue(_ctx, _onDrawGizmosFunc);
            _onDrawGizmosValid = false;
#endif
            JSApi.JS_FreeValue(_ctx, _onApplicationPauseFunc);
            _onApplicationPauseValid = false;
            JSApi.JS_FreeValue(_ctx, _onApplicationQuitFunc);
            _onApplicationQuitValid = false;
            JSApi.JS_FreeValue(_ctx, _onDestroyFunc);
            _onDestroyValid = false;
            JSApi.JS_FreeValue(_ctx, _this_obj);

            var context = ScriptEngine.GetContext(_ctx);
            if (context != null)
            {
                context.OnDestroy -= OnContextDestroy;
            }
        }

        void Update()
        {
            if (_updateValid)
            {
                var rval = JSApi.JS_Call(_ctx, _updateFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }

        void LateUpdate()
        {
            if (_lateUpdateValid)
            {
                var rval = JSApi.JS_Call(_ctx, _lateUpdateFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }

        void FixedUpdate()
        {
            if (_fixedUpdateValid)
            {
                var rval = JSApi.JS_Call(_ctx, _fixedUpdateFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }

        void Start()
        {
            if (_startValid)
            {
                var rval = JSApi.JS_Call(_ctx, _startFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }

        void OnEnable()
        {
            if (_onEnableValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onEnableFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }

        void OnDisable()
        {
            if (_onDisableValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onDisableFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isCompiling)
            {
                Release();
            }
#endif
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (_onDrawGizmosValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onDrawGizmosFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }
#endif

        void OnApplicationFocus()
        {
            if (_onApplicationFocusValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onApplicationFocusFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }

        void OnApplicationPause()
        {
            if (_onApplicationPauseValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onApplicationPauseFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }

        void OnApplicationQuit()
        {
            if (_onApplicationQuitValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onApplicationQuitFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }

        void OnDestroy()
        {
            if (_onDestroyValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onDestroyFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
            Release();
        }
    }
}