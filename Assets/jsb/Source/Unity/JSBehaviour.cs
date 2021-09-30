#if !JSB_UNITYLESS
using System;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using Native;
    using UnityEngine;
    using UnityEngine.Serialization;

    public class JSBehaviour : MonoBehaviour, ISerializationCallbackReceiver, IScriptEditorSupport, IScriptInstancedObject
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

        public static bool IsUpdatable(ScriptContext context, JSValue prototype)
        {
            return prototype.CheckFuncProperty(context, "Update")
                || prototype.CheckFuncProperty(context, "LateUpdate")
                || prototype.CheckFuncProperty(context, "FixedUpdate");
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
                    var context = ScriptEngine.GetContext(ctx);
                    if (!prototype.IsException())
                    {
                        if (context != null)
                        {
                            fullCap = IsUpdatable(context, prototype);
                        }
                    }
                    JSApi.JS_FreeValue(ctx, prototype);
                    var bridge = fullCap ? gameObject.AddComponent<JSBehaviourFull>() : gameObject.AddComponent<JSBehaviour>();
#if UNITY_EDITOR
                    context.TrySetScriptRef(ref bridge._scriptRef, ctor);
#endif
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
                    var objectCache = ScriptEngine.GetObjectCache(ctx);

                    // 旧的绑定值释放？
                    if (!_this_obj.IsNullish())
                    {
                        var payload = JSApi.JSB_FreePayload(ctx, _this_obj);
                        if (payload.type_id == BridgeObjectType.ObjectRef)
                        {
                            try
                            {
                                objectCache.RemoveObject(payload.value);
                            }
                            catch (Exception exception)
                            {
                                ScriptEngine.GetLogger(ctx)?.WriteException(exception);
                            }
                        }
                    }

                    var object_id = objectCache.AddObject(this, false);
                    var val = JSApi.jsb_construct_bridge_object(ctx, ctor, object_id);
                    if (val.IsException())
                    {
                        objectCache.RemoveObject(object_id);
                        SetUnresolvedScriptInstance();
                    }
                    else
                    {
                        objectCache.AddJSValue(this, val);
                        this._SetScriptInstance(ctx, val, execAwake);
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
                    JSScriptableObject._CallJSFunc(context, _this_obj, _awakeFunc);
                    if (enabled && _onEnableValid)
                    {
                        JSScriptableObject._CallJSFunc(context, _this_obj, _onEnableFunc);
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

#if UNITY_EDITOR
            _onDrawGizmosFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnDrawGizmos"));
            _onDrawGizmosValid = JSApi.JS_IsFunction(ctx, _onDrawGizmosFunc) == 1;
#endif

            _onDestroyFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnDestroy"));
            _onDestroyValid = JSApi.JS_IsFunction(ctx, _onDestroyFunc) == 1;

            _awakeFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("Awake"));
            _awakeValid = JSApi.JS_IsFunction(ctx, _awakeFunc) == 1;

            if (Application.isPlaying)
            {
                if (JSApi.JS_HasProperty(ctx, _this_obj, context.GetAtom("OnBecameVisible")) == 1
                || JSApi.JS_HasProperty(ctx, _this_obj, context.GetAtom("OnBecameInvisible")) == 1)
                {
                    SetupMonoBehaviourCallback(typeof(JSBecameVisibleCallback));
                }

                if (JSApi.JS_HasProperty(ctx, _this_obj, context.GetAtom("OnCollisionEnter")) == 1
                || JSApi.JS_HasProperty(ctx, _this_obj, context.GetAtom("OnCollisionExit")) == 1
                || JSApi.JS_HasProperty(ctx, _this_obj, context.GetAtom("OnCollisionStay")) == 1)
                {
                    SetupMonoBehaviourCallback(typeof(JSCollisionCallback));
                }

                if (JSApi.JS_HasProperty(ctx, _this_obj, context.GetAtom("OnApplicationFocus")) == 1
                || JSApi.JS_HasProperty(ctx, _this_obj, context.GetAtom("OnApplicationPause")) == 1
                || JSApi.JS_HasProperty(ctx, _this_obj, context.GetAtom("OnApplicationQuit")) == 1)
                {
                    SetupMonoBehaviourCallback(typeof(JSApplicationCallback));
                }
            }
        }

        public void SetupMonoBehaviourCallback(Type type)
        {
            if (type != null && type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                var go = gameObject;
                if (go && !go.GetComponent(type))
                {
                    go.AddComponent(type);
                }
            }
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

#if UNITY_EDITOR

            JSApi.JS_FreeValue(_ctx, _onDrawGizmosFunc);
            _onDrawGizmosFunc = JSApi.JS_UNDEFINED;
            _onDrawGizmosValid = false;
#endif

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

        public static void Dispatch(GameObject gameObject, string funcName)
        {
            if (gameObject)
            {
                foreach (var comp in gameObject.GetComponents<JSBehaviour>())
                {
                    comp._Dispatch(funcName);
                }
            }
        }

        public static void Dispatch(GameObject gameObject, string funcName, bool p1)
        {
            if (gameObject)
            {
                foreach (var comp in gameObject.GetComponents<JSBehaviour>())
                {
                    comp._Dispatch(funcName, p1);
                }
            }
        }

        public static void Dispatch(GameObject gameObject, string funcName, object p1)
        {
            if (gameObject)
            {
                foreach (var comp in gameObject.GetComponents<JSBehaviour>())
                {
                    comp._Dispatch(funcName, p1);
                }
            }
        }

        public void _Dispatch(string funcName)
        {
            if (!_this_obj.IsNullish())
            {
                var context = ScriptEngine.GetContext(_ctx);
                var atom = context.GetAtom(funcName);
                if (JSApi.JS_HasProperty(_ctx, _this_obj, atom) == 1)
                {
                    var rval = JSApi.JS_Invoke(_ctx, _this_obj, atom);
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

        public unsafe void _Dispatch(string funcName, bool p1)
        {
            if (!_this_obj.IsNullish())
            {
                var context = ScriptEngine.GetContext(_ctx);
                var atom = context.GetAtom(funcName);
                if (JSApi.JS_HasProperty(_ctx, _this_obj, atom) == 1)
                {
                    var argv = stackalloc[] { Binding.Values.js_push_primitive(_ctx, p1) };
                    var rval = JSApi.JS_Invoke(_ctx, _this_obj, atom, 1, argv);
                    if (rval.IsException())
                    {
                        _ctx.print_exception();
                    }
                    else
                    {
                        JSApi.JS_FreeValue(_ctx, rval);
                    }
                    JSApi.JS_FreeValue(_ctx, argv[0]);
                }
            }
        }

        public unsafe void _Dispatch(string funcName, object p1)
        {
            if (!_this_obj.IsNullish())
            {
                var context = ScriptEngine.GetContext(_ctx);
                var atom = context.GetAtom(funcName);
                if (JSApi.JS_HasProperty(_ctx, _this_obj, atom) == 1)
                {
                    var argv = stackalloc[] { Binding.Values.js_push_var(_ctx, p1) };
                    var rval = JSApi.JS_Invoke(_ctx, _this_obj, atom, 1, argv);
                    if (rval.IsException())
                    {
                        _ctx.print_exception();
                    }
                    else
                    {
                        JSApi.JS_FreeValue(_ctx, rval);
                    }
                    JSApi.JS_FreeValue(_ctx, argv[0]);
                }
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
                    else
                    {
                        JSApi.JS_FreeValue(_ctx, rval);
                    }
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
                                JSScriptableObject._CallJSFunc(context, _this_obj, _onAfterScriptReloadFunc);
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
                else
                {
                    JSApi.JS_FreeValue(_ctx, rval);
                }
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
                else
                {
                    JSApi.JS_FreeValue(_ctx, rval);
                }
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
                else
                {
                    JSApi.JS_FreeValue(_ctx, rval);
                }
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
                else
                {
                    JSApi.JS_FreeValue(_ctx, rval);
                }
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
                else
                {
                    JSApi.JS_FreeValue(_ctx, rval);
                }
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
                else
                {
                    JSApi.JS_FreeValue(_ctx, rval);
                }
            }
        }
#endif

        void OnDestroy()
        {
            if (_onDestroyValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onDestroyFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                else
                {
                    JSApi.JS_FreeValue(_ctx, rval);
                }
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