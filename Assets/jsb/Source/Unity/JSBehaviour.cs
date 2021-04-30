#if !JSB_UNITYLESS
using System;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using Native;
    using UnityEngine;

    public class JSBehaviour : MonoBehaviour, ISerializationCallbackReceiver
    {
        // 在编辑器运行时下与 js 脚本建立链接关系
        public JSBehaviourScriptRef scriptRef;

        // internal use only
        public JSBehaviourProperties properties => _properties;

        // unsafe, internal use only
        public JSContext ctx { get { return _ctx; } }

        public bool isScriptInstanced => _isScriptInstanced;

#if UNITY_EDITOR
        // self controlled script instance lifetime 
        private bool _isStandaloneScript = false;
        public bool isStandaloneScript => _isStandaloneScript;
#endif

        [SerializeField]
        private JSBehaviourProperties _properties;

        private bool _isScriptInstanced = false;

        private JSContext _ctx;
        private JSValue _this_obj = JSApi.JS_UNDEFINED;

        private bool _updateValid;
        private JSValue _updateFunc = JSApi.JS_UNDEFINED;

        private bool _lateUpdateValid;
        private JSValue _lateUpdateFunc = JSApi.JS_UNDEFINED;

        private bool _fixedUpdateValid;
        private JSValue _fixedUpdateFunc = JSApi.JS_UNDEFINED;

        private bool _startValid;
        private JSValue _startFunc = JSApi.JS_UNDEFINED;

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

#if UNITY_EDITOR
        private bool _onDrawGizmosValid;
        private JSValue _onDrawGizmosFunc = JSApi.JS_UNDEFINED;
#endif

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

        public unsafe void ForEachProperty(Action<JSContext, JSAtom, JSValue> callback)
        {
            if (!IsValid())
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

        // 在 gameObject 上创建一个新的脚本组件实例
        // ctor: js class
        public static JSValue SetScriptInstance(GameObject gameObject, JSContext ctx, JSValue ctor, bool execAwake)
        {
            if (JSApi.JS_IsConstructor(ctx, ctor) == 1)
            {
                var header = JSApi.jsb_get_payload_header(ctor);
                if (header.type_id == BridgeObjectType.None) // it's a plain js value
                {
                    var bridge = gameObject.AddComponent<JSBehaviour>();
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
                _onBeforeSerializeFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnBeforeSerialize"));
                _onBeforeSerializeValid = JSApi.JS_IsFunction(ctx, _onBeforeSerializeFunc) == 1;

                _onAfterDeserializeFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnAfterDeserialize"));
                _onAfterDeserializeValid = JSApi.JS_IsFunction(ctx, _onAfterDeserializeFunc) == 1;

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

                _onApplicationPauseFunc =
                    JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnApplicationPause"));
                _onApplicationPauseValid = JSApi.JS_IsFunction(ctx, _onApplicationPauseFunc) == 1;

                _onApplicationQuitFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnApplicationQuit"));
                _onApplicationQuitValid = JSApi.JS_IsFunction(ctx, _onApplicationQuitFunc) == 1;

                _onDestroyFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnDestroy"));
                _onDestroyValid = JSApi.JS_IsFunction(ctx, _onDestroyFunc) == 1;

                _awakeFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("Awake"));
                _awakeValid = JSApi.JS_IsFunction(ctx, _awakeFunc) == 1;

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
            ReleaseJSValues();
        }

#if UNITY_EDITOR
        private void OnScriptReloading(ScriptContext context, string resolved_id)
        {
            if (context.CheckModuleId(scriptRef, resolved_id))
            {
                OnBeforeSerialize();
            }
        }
        
        private void OnScriptReloaded(ScriptContext context, string resolved_id)
        {
            if (context.CheckModuleId(scriptRef, resolved_id))
            {
                ReleaseJSValues();
                CreateScriptInstance();
            }
        }
#endif

        public void ReleaseJSValues()
        {
            if (!_this_obj.IsNullish())
            {
                JSApi.JS_FreeValue(_ctx, _updateFunc);
                _updateFunc = JSApi.JS_UNDEFINED;
                _updateValid = false;

                JSApi.JS_FreeValue(_ctx, _lateUpdateFunc);
                _lateUpdateFunc = JSApi.JS_UNDEFINED;
                _lateUpdateValid = false;

                JSApi.JS_FreeValue(_ctx, _fixedUpdateFunc);
                _fixedUpdateFunc = JSApi.JS_UNDEFINED;
                _fixedUpdateValid = false;

                JSApi.JS_FreeValue(_ctx, _startFunc);
                _startFunc = JSApi.JS_UNDEFINED;
                _startValid = false;

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
                    _properties = new JSBehaviourProperties();
                }
                else
                {
                    _properties.Clear();
                }

                unsafe
                {
                    var argv = stackalloc[] { Binding.Values.js_push_var(_ctx, _properties) };
                    var rval = JSApi.JS_Call(_ctx, _onBeforeSerializeFunc, _this_obj, 1, argv);
                    JSApi.JS_FreeValue(_ctx, argv[0]);
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

        public void OnAfterDeserialize()
        {
            // intentionally skipped
        }

        // 通过 scriptRef 还原脚本绑定关系
        public bool CreateScriptInstance()
        {
            if (!_isScriptInstanced)
            {
                if (!string.IsNullOrEmpty(scriptRef.modulePath) && !string.IsNullOrEmpty(scriptRef.className))
                {
                    var runtime = ScriptEngine.GetRuntime();
                    if (runtime != null && runtime.isInitialized)
                    {
                        var context = runtime.GetMainContext();
                        if (context != null)
                        {
                            var ctx = (JSContext)context;
                            var snippet = $"require('{scriptRef.modulePath}')['{scriptRef.className}']";
                            var bytes = System.Text.Encoding.UTF8.GetBytes(snippet);
                            var typeValue = ScriptRuntime.EvalSource(ctx, bytes, scriptRef.sourceFile, false);
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

        public void _OnScriptingAfterDeserialize()
        {
            if (_onAfterDeserializeValid)
            {
                if (_properties == null)
                {
                    _properties = new JSBehaviourProperties();
                }

                unsafe
                {
                    var argv = stackalloc[] { Binding.Values.js_push_var(_ctx, _properties) };
                    var rval = JSApi.JS_Call(_ctx, _onAfterDeserializeFunc, _this_obj, 1, argv);
                    JSApi.JS_FreeValue(_ctx, argv[0]);
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
    }
}
#endif