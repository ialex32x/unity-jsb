using System;
using System.Reflection;

namespace QuickJS.Unity
{
    using Native;
    using UnityEngine;
    using UnityEditor;

    /// <summary>
    /// 脚本实现编辑器窗口的基础类. 
    /// 此实现刻意放在非 Editor 目录下, 以便在非编辑器脚本中也可以使用, 实际仍然只在编辑器环境下可用.
    /// </summary>
    public class JSEditorWindow : EditorWindow, IHasCustomMenu
    {
        private string _scriptTypeName;

        public string scriptTypeName
        {
            get { return _scriptTypeName; }
        }

        private bool _released;
        private bool _destroyed;
        private JSContext _ctx;
        private JSValue _this_obj;

        private bool _updateValid;
        private JSValue _updateFunc;

        private bool _onEnableValid;
        private JSValue _onEnableFunc;

        private bool _onDisableValid;
        private JSValue _onDisableFunc;

        private bool _onDestroyValid;
        private JSValue _onDestroyFunc;

        private bool _onGUIValid;
        private JSValue _onGUIFunc;

        private bool _addItemsToMenuValid;
        private JSValue _addItemsToMenuFunc;

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

            _onEnableFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnEnable"));
            _onEnableValid = JSApi.JS_IsFunction(ctx, _onEnableFunc) == 1;

            _onDisableFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnDisable"));
            _onDisableValid = JSApi.JS_IsFunction(ctx, _onDisableFunc) == 1;

            _onDestroyFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnDestroy"));
            _onDestroyValid = JSApi.JS_IsFunction(ctx, _onDestroyFunc) == 1;

            _onGUIFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnGUI"));
            _onGUIValid = JSApi.JS_IsFunction(ctx, _onGUIFunc) == 1;

            _addItemsToMenuFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("AddItemsToMenu"));
            _addItemsToMenuValid = JSApi.JS_IsFunction(ctx, _addItemsToMenuFunc) == 1;

            var awake_obj = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("Awake"));

            Call(awake_obj);
            JSApi.JS_FreeValue(_ctx, awake_obj);
            if (_onEnableValid)
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
            JSApi.JS_FreeValue(_ctx, _onEnableFunc);
            _onEnableValid = false;
            JSApi.JS_FreeValue(_ctx, _onDisableFunc);
            _onDisableValid = false;
            JSApi.JS_FreeValue(_ctx, _onDestroyFunc);
            _onDestroyValid = false;
            JSApi.JS_FreeValue(_ctx, _onGUIFunc);
            _onGUIValid = false;
            JSApi.JS_FreeValue(_ctx, _addItemsToMenuFunc);
            _addItemsToMenuValid = false;
            JSApi.JS_FreeValue(_ctx, _this_obj);

            var context = ScriptEngine.GetContext(_ctx);
            if (context != null)
            {
                context.OnDestroy -= OnContextDestroy;
            }

            if (!_destroyed)
            {
                try
                {
                    Close();
                }
                catch (Exception) { }
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
            if (UnityEditor.EditorApplication.isCompiling)
            {
                Release();
            }
        }

        void OnDestroy()
        {
            if (_destroyed)
            {
                return;
            }

            if (_onDestroyValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onDestroyFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
            _destroyed = true;
            Release();
        }

        void OnGUI()
        {
            if (_onGUIValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onGUIFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }

        public unsafe void AddItemsToMenu(GenericMenu menu)
        {
            if (_addItemsToMenuValid)
            {
                var argv = stackalloc JSValue[] { Binding.Values.js_push_classvalue(_ctx, menu) };
                var rval = JSApi.JS_Call(_ctx, _addItemsToMenuFunc, _this_obj, 1, argv);

                JSApi.JS_FreeValue(_ctx, argv[0]);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }
    }
}