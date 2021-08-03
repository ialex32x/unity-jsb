#if !JSB_UNITYLESS
using System;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using Native;
    using UnityEngine;
    using UnityEngine.Serialization;

    public class JSBehaviour : MonoBehaviour, ISerializationCallbackReceiver, IScriptEditorSupport
    {
        // 在编辑器运行时下与 js 脚本建立链接关系
        [SerializeField]
        [FormerlySerializedAs("scriptRef")]
        private JSScriptRef _scriptRef;

        public JSScriptRef scriptRef { get { return _scriptRef; } set { _scriptRef = value; } }

        [SerializeField]
        private JSScriptProperties _properties;

        // internal use only
        public JSScriptProperties properties => _properties;

        // unsafe, internal use only
        public JSContext ctx { get { return _ctx; } }

        private bool _isScriptInstanced = false;

        public bool isScriptInstanced => _isScriptInstanced;

#if UNITY_EDITOR
        // self controlled script instance lifetime 
        private bool _isStandaloneScript = false;
        public bool isStandaloneScript => _isStandaloneScript;
#else
        public bool isStandaloneScript => true;
#endif

        protected JSContext _ctx = JSContext.Null;

        protected JSValue _this_obj = JSApi.JS_UNDEFINED;

        private bool _startValid;
        private JSValue _startFunc = JSApi.JS_UNDEFINED;

        private bool _resetValid;
        private JSValue _resetFunc = JSApi.JS_UNDEFINED;

        private bool _onEnableValid;
        private JSValue _onEnableFunc = JSApi.JS_UNDEFINED;

        private bool _onDisableValid;
        private JSValue _onDisableFunc = JSApi.JS_UNDEFINED;

        private bool _onApplicationFocusValid;
        private JSValue _onApplicationFocusFunc = JSApi.JS_UNDEFINED;

        private bool _onApplicationPauseValid;
        private JSValue _onApplicationPauseFunc = JSApi.JS_UNDEFINED;

        private bool _onApplicationQuitValid;
        private JSValue _onApplicationQuitFunc = JSApi.JS_UNDEFINED;

        private bool _onDestroyValid;
        private JSValue _onDestroyFunc = JSApi.JS_UNDEFINED;

        private bool _awakeValid;
        private JSValue _awakeFunc = JSApi.JS_UNDEFINED;

        private bool _onBeforeSerializeValid;
        private JSValue _onBeforeSerializeFunc = JSApi.JS_UNDEFINED;

        private bool _onAfterDeserializeValid;
        private JSValue _onAfterDeserializeFunc = JSApi.JS_UNDEFINED;

        private bool _onBeforeScriptReloadValid;
        private JSValue _onBeforeScriptReloadFunc = JSApi.JS_UNDEFINED;

        private bool _onAfterScriptReloadValid;
        private JSValue _onAfterScriptReloadFunc = JSApi.JS_UNDEFINED;

#if UNITY_EDITOR
        private bool _onDrawGizmosValid;
        private JSValue _onDrawGizmosFunc = JSApi.JS_UNDEFINED;
#endif

        public JSValue ToValue()
        {
            return _this_obj;
        }

        public bool IsValid()
        {
            return _ctx.IsValid() && !_this_obj.IsNullish();
        }

        public int IsInstanceOf(JSValue ctor)
        {
            if (!IsValid())
            {
                return 0;
            }
            return JSApi.JS_IsInstanceOf(_ctx, _this_obj, ctor);
        }

        public JSValue CloneValue()
        {
            if (!IsValid())
            {
                return JSApi.JS_UNDEFINED;
            }
            return JSApi.JS_DupValue(_ctx, _this_obj);
        }

        public JSValue GetProperty(string key)
        {
            if (!IsValid())
            {
                return JSApi.JS_UNDEFINED;
            }

            return JSApi.JS_GetPropertyStr(_ctx, _this_obj, key);
        }

        // 在 gameObject 上创建一个新的脚本组件实例
        // ctor: js class
        public static JSValue SetScriptInstance(GameObject gameObject, JSContext ctx, JSValue ctor, bool execAwake)
        {
            if (JSApi.JS_IsConstructor(ctx, ctor) == 1)
            {
                var header = JSApi.jsb_get_payload_header(ctor);
                if (header.type_id == BridgeObjectType.None) // it's a plain js value
                {
                    var fullCap = false;
                    var prototype = JSApi.JS_GetProperty(ctx, ctor, JSApi.JS_ATOM_prototype);
                    if (!prototype.IsException())
                    {
                        var context = ScriptEngine.GetContext(ctx);
                        if (context != null)
                        {
                            fullCap = prototype.CheckFuncProperty(context, "Update")
                                || prototype.CheckFuncProperty(context, "LateUpdate")
                                || prototype.CheckFuncProperty(context, "FixedUpdate");
                        }
                    }
                    JSApi.JS_FreeValue(ctx, prototype);
                    var bridge = fullCap ? gameObject.AddComponent<JSBehaviourFull>() : gameObject.AddComponent<JSBehaviour>();
                    return bridge.SetScriptInstance(ctx, ctor, execAwake);
                }
            }

            return JSApi.JS_UNDEFINED;
        }

        public void SetUnresolvedScriptInstance()
        {
            _isScriptInstanced = true;
        }

        public void ReleaseScriptInstance()
        {
            _isScriptInstanced = false;
            ReleaseJSValues();
        }

        // 在当前 JSBehaviour 实例上创建一个脚本实例并与之绑定
        public JSValue SetScriptInstance(JSContext ctx, JSValue ctor, bool execAwake)
        {
            if (JSApi.JS_IsConstructor(ctx, ctor) == 1)
            {
                var header = JSApi.jsb_get_payload_header(ctor);
                if (header.type_id == BridgeObjectType.None) // it's a plain js value
                {
                    var cache = ScriptEngine.GetObjectCache(ctx);

                    // 旧的绑定值释放？
                    if (!_this_obj.IsNullish())
                    {
                        var payload = JSApi.jsb_get_payload_header(_this_obj);
                        if (payload.type_id == BridgeObjectType.ObjectRef)
                        {
                            var runtime = ScriptEngine.GetRuntime(ctx);
                            var objectCache = runtime.GetObjectCache();

                            if (objectCache != null)
                            {
                                object obj;
                                try
                                {
                                    objectCache.RemoveObject(payload.value, out obj);
                                }
                                catch (Exception exception)
                                {
                                    runtime.GetLogger()?.WriteException(exception);
                                }
                            }
                        }
                    }

                    var object_id = cache.AddObject(this, false);
                    var val = JSApi.jsb_construct_bridge_object(ctx, ctor, object_id);
                    if (val.IsException())
                    {
                        cache.RemoveObject(object_id);
                        SetUnresolvedScriptInstance();
                    }
                    else
                    {
                        cache.AddJSValue(this, val);
                        this._SetScriptInstance(ctx, val, execAwake);
                        // JSApi.JSB_SetBridgeType(ctx, val, type_id);
                    }

                    return val;
                }
            }

            SetUnresolvedScriptInstance();
            return JSApi.JS_UNDEFINED;
        }

        private void _SetScriptInstance(JSContext ctx, JSValue this_obj, bool execAwake)
        {
            var context = ScriptEngine.GetContext(ctx);
            if (context == null)
            {
                return;
            }

            ReleaseJSValues();
            _ctx = ctx;
            context.OnDestroy += OnContextDestroy;
#if UNITY_EDITOR
            context.OnScriptReloading += OnScriptReloading;
            context.OnScriptReloaded += OnScriptReloaded;
#endif

            _isScriptInstanced = true;
            _this_obj = JSApi.JS_DupValue(ctx, this_obj);

            if (!_this_obj.IsNullish())
            {
                this.OnBindingJSFuncs(context);
                this._OnScriptingAfterDeserialize();
                if (execAwake)
                {
                    CallJSFunc(_awakeFunc);
                    if (enabled && _onEnableValid)
                    {
                        CallJSFunc(_onEnableFunc);
                    }
                }
            }
        }

        protected virtual void OnBindingJSFuncs(ScriptContext context)
        {
            _onBeforeSerializeFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnBeforeSerialize"));
            _onBeforeSerializeValid = JSApi.JS_IsFunction(ctx, _onBeforeSerializeFunc) == 1;

            _onAfterDeserializeFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnAfterDeserialize"));
            _onAfterDeserializeValid = JSApi.JS_IsFunction(ctx, _onAfterDeserializeFunc) == 1;

            _onBeforeScriptReloadFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnBeforeScriptReload"));
            _onBeforeScriptReloadValid = JSApi.JS_IsFunction(ctx, _onBeforeScriptReloadFunc) == 1;

            _onAfterScriptReloadFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnAfterScriptReload"));
            _onAfterScriptReloadValid = JSApi.JS_IsFunction(ctx, _onAfterScriptReloadFunc) == 1;

            _startFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("Start"));
            _startValid = JSApi.JS_IsFunction(ctx, _startFunc) == 1;

            _resetFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("Reset"));
            _resetValid = JSApi.JS_IsFunction(ctx, _resetFunc) == 1;

            _onEnableFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnEnable"));
            _onEnableValid = JSApi.JS_IsFunction(ctx, _onEnableFunc) == 1;

            _onDisableFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnDisable"));
            _onDisableValid = JSApi.JS_IsFunction(ctx, _onDisableFunc) == 1;

            _onApplicationFocusFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnApplicationFocus"));
            _onApplicationFocusValid = JSApi.JS_IsFunction(ctx, _onApplicationFocusFunc) == 1;

#if UNITY_EDITOR
            _onDrawGizmosFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnDrawGizmos"));
            _onDrawGizmosValid = JSApi.JS_IsFunction(ctx, _onDrawGizmosFunc) == 1;
#endif

            _onApplicationPauseFunc =
                JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnApplicationPause"));
            _onApplicationPauseValid = JSApi.JS_IsFunction(ctx, _onApplicationPauseFunc) == 1;

            _onApplicationQuitFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnApplicationQuit"));
            _onApplicationQuitValid = JSApi.JS_IsFunction(ctx, _onApplicationQuitFunc) == 1;

            _onDestroyFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnDestroy"));
            _onDestroyValid = JSApi.JS_IsFunction(ctx, _onDestroyFunc) == 1;

            _awakeFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("Awake"));
            _awakeValid = JSApi.JS_IsFunction(ctx, _awakeFunc) == 1;
        }

        protected virtual void OnUnbindingJSFuncs()
        {
            JSApi.JS_FreeValue(_ctx, _startFunc);
            _startFunc = JSApi.JS_UNDEFINED;
            _startValid = false;

            JSApi.JS_FreeValue(_ctx, _resetFunc);
            _resetFunc = JSApi.JS_UNDEFINED;
            _resetValid = false;

            JSApi.JS_FreeValue(_ctx, _onEnableFunc);
            _onEnableFunc = JSApi.JS_UNDEFINED;
            _onEnableValid = false;

            JSApi.JS_FreeValue(_ctx, _onDisableFunc);
            _onDisableFunc = JSApi.JS_UNDEFINED;
            _onDisableValid = false;

            JSApi.JS_FreeValue(_ctx, _onApplicationFocusFunc);
            _onApplicationFocusFunc = JSApi.JS_UNDEFINED;
            _onApplicationFocusValid = false;
#if UNITY_EDITOR

            JSApi.JS_FreeValue(_ctx, _onDrawGizmosFunc);
            _onDrawGizmosFunc = JSApi.JS_UNDEFINED;
            _onDrawGizmosValid = false;
#endif

            JSApi.JS_FreeValue(_ctx, _onApplicationPauseFunc);
            _onApplicationPauseFunc = JSApi.JS_UNDEFINED;
            _onApplicationPauseValid = false;

            JSApi.JS_FreeValue(_ctx, _onApplicationQuitFunc);
            _onApplicationQuitFunc = JSApi.JS_UNDEFINED;
            _onApplicationQuitValid = false;

            JSApi.JS_FreeValue(_ctx, _onDestroyFunc);
            _onDestroyFunc = JSApi.JS_UNDEFINED;
            _onDestroyValid = false;

            JSApi.JS_FreeValue(_ctx, _awakeFunc);
            _awakeFunc = JSApi.JS_UNDEFINED;
            _awakeValid = false;

            JSApi.JS_FreeValue(_ctx, _onBeforeSerializeFunc);
            _onBeforeSerializeFunc = JSApi.JS_UNDEFINED;
            _onBeforeSerializeValid = false;

            JSApi.JS_FreeValue(_ctx, _onAfterDeserializeFunc);
            _onAfterDeserializeFunc = JSApi.JS_UNDEFINED;
            _onAfterDeserializeValid = false;

            JSApi.JS_FreeValue(_ctx, _onBeforeScriptReloadFunc);
            _onBeforeScriptReloadFunc = JSApi.JS_UNDEFINED;
            _onBeforeScriptReloadValid = false;

            JSApi.JS_FreeValue(_ctx, _onAfterScriptReloadFunc);
            _onAfterScriptReloadFunc = JSApi.JS_UNDEFINED;
            _onAfterScriptReloadValid = false;
        }

        private void CallJSFunc(JSValue func_obj)
        {
            if (!_this_obj.IsNullish() && JSApi.JS_IsFunction(_ctx, func_obj) == 1)
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
            // it's dangerous, more protection are required during the process of context-destroying

            if (enabled)
            {
                OnDisable();
            }
            OnDestroy();
        }

#if UNITY_EDITOR
        private void OnScriptReloading(ScriptContext context, string resolved_id)
        {
            if (context.CheckModuleId(_scriptRef, resolved_id))
            {
                if (_onBeforeScriptReloadValid)
                {
                    var rval = JSApi.JS_Call(_ctx, _onBeforeScriptReloadFunc, _this_obj);
                    if (rval.IsException())
                    {
                        _ctx.print_exception();
                    }
                    JSApi.JS_FreeValue(_ctx, rval);
                }
            }
        }

        private void OnScriptReloaded(ScriptContext context, string resolved_id)
        {
            if (context.CheckModuleId(_scriptRef, resolved_id))
            {
                if (!_this_obj.IsNullish())
                {
                    JSValue newClass;
                    if (context.LoadModuleCacheExports(resolved_id, _scriptRef.className, out newClass))
                    {
                        var prototype = JSApi.JS_GetProperty(context, newClass, context.GetAtom("prototype"));

                        if (prototype.IsObject())
                        {
                            OnUnbindingJSFuncs();
                            JSApi.JS_SetPrototype(context, _this_obj, prototype);
                            OnBindingJSFuncs(context);

                            if (_onAfterScriptReloadValid)
                            {
                                var rval = JSApi.JS_Call(_ctx, _onAfterScriptReloadFunc, _this_obj);
                                if (rval.IsException())
                                {
                                    _ctx.print_exception();
                                }
                                JSApi.JS_FreeValue(_ctx, rval);
                            }
                        }

                        JSApi.JS_FreeValue(context, prototype);
                        JSApi.JS_FreeValue(context, newClass);
                    }
                }
            }
        }
#endif

        public void ReleaseJSValues()
        {
            if (!_this_obj.IsNullish())
            {
                this.OnUnbindingJSFuncs();
                JSApi.JS_FreeValue(_ctx, _this_obj);
                _this_obj = JSApi.JS_UNDEFINED;
            }

            _isScriptInstanced = false;
            var context = ScriptEngine.GetContext(_ctx);
            _ctx = JSContext.Null;

            if (context != null)
            {
                context.OnDestroy -= OnContextDestroy;
#if UNITY_EDITOR
                context.OnScriptReloading -= OnScriptReloading;
                context.OnScriptReloaded -= OnScriptReloaded;
#endif
            }
        }

        void Awake()
        {
#if UNITY_EDITOR
            _isStandaloneScript = true;
#endif
            CreateScriptInstance();
            // _OnScriptingAfterDeserialize();

            if (_awakeValid)
            {
                var rval = JSApi.JS_Call(_ctx, _awakeFunc, _this_obj);
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

        public void Reset()
        {
            if (_resetValid)
            {
                var rval = JSApi.JS_Call(_ctx, _resetFunc, _this_obj);
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
                ReleaseJSValues();
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
            ReleaseJSValues();
        }

        public void OnBeforeSerialize()
        {
            if (_onBeforeSerializeValid)
            {
                if (_properties == null)
                {
                    _properties = new JSScriptProperties();
                }
                else
                {
                    _properties.Clear();
                }

                var buffer = ScriptEngine.AllocByteBuffer(_ctx, 512);

                unsafe
                {
                    var argv = stackalloc[] { Binding.Values.js_push_classvalue(_ctx, _properties), Binding.Values.js_push_classvalue(_ctx, buffer) };
                    var rval = JSApi.JS_Call(_ctx, _onBeforeSerializeFunc, _this_obj, 2, argv);
                    JSApi.JS_FreeValue(_ctx, argv[0]);
                    JSApi.JS_FreeValue(_ctx, argv[1]);
                    if (rval.IsException())
                    {
                        _ctx.print_exception();
                    }
                    else
                    {
                        JSApi.JS_FreeValue(_ctx, rval);
                    }
                }
                _properties.SetGenericValue(buffer);
            }
        }

        public void OnAfterDeserialize()
        {
            // intentionally skipped
        }

        public void _OnScriptingAfterDeserialize()
        {
            if (_onAfterDeserializeValid)
            {
                if (_properties == null)
                {
                    _properties = new JSScriptProperties();
                }

                var buffer = new IO.ByteBuffer(_properties.genericValueData);

                unsafe
                {
                    var argv = stackalloc[] { Binding.Values.js_push_classvalue(_ctx, _properties), Binding.Values.js_push_classvalue(_ctx, buffer) };
                    var rval = JSApi.JS_Call(_ctx, _onAfterDeserializeFunc, _this_obj, 2, argv);
                    JSApi.JS_FreeValue(_ctx, argv[0]);
                    JSApi.JS_FreeValue(_ctx, argv[1]);
                    if (rval.IsException())
                    {
                        _ctx.print_exception();
                    }
                    else
                    {
                        JSApi.JS_FreeValue(_ctx, rval);
                    }
                }
            }
        }

        // 通过 scriptRef 还原脚本绑定关系
        public bool CreateScriptInstance()
        {
            if (!_isScriptInstanced)
            {
                if (!string.IsNullOrEmpty(_scriptRef.modulePath) && !string.IsNullOrEmpty(_scriptRef.className))
                {
                    var runtime = ScriptEngine.GetRuntime();
                    if (runtime != null && runtime.isInitialized)
                    {
                        var context = runtime.GetMainContext();
                        if (context != null)
                        {
                            var ctx = (JSContext)context;
                            var snippet = $"require('{_scriptRef.modulePath}')['{_scriptRef.className}']";
                            var bytes = System.Text.Encoding.UTF8.GetBytes(snippet);
                            var typeValue = ScriptRuntime.EvalSource(ctx, bytes, _scriptRef.sourceFile, false);
                            if (JSApi.JS_IsException(typeValue))
                            {
                                var ex = ctx.GetExceptionString();
                                Debug.LogError(ex);
                                SetUnresolvedScriptInstance();
                            }
                            else
                            {
                                var instValue = SetScriptInstance(ctx, typeValue, false);
                                JSApi.JS_FreeValue(ctx, instValue);
                                JSApi.JS_FreeValue(ctx, typeValue);

                                // if (!instValue.IsObject())
                                // {
                                //     Debug.LogError("script instance error");
                                // }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("script runtime not ready");
                    }
                }
                else
                {
                    SetUnresolvedScriptInstance();
                }
            }

            return _isScriptInstanced;
        }
    }
}
#endif