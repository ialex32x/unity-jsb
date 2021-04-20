using System;
using System.Collections.Generic;
using UnityEditor;

namespace QuickJS.Unity
{
    using Native;
    using UnityEngine;

    public class JSBehaviour : MonoBehaviour, ISerializationCallbackReceiver
    {
        // 在编辑器运行时下与 js 脚本建立链接关系
        public JSBehaviourScriptRef scriptRef;

        [SerializeField]
        private JSBehaviourProperties<Object> _objects;
        [SerializeField]
        private JSBehaviourProperties<double> _numbers;

        // unsafe
        public JSContext ctx { get { return _ctx; } }

        public bool isScriptInstanced => _isScriptInstanced;

        private bool _isScriptInstanced;

        private bool _released;
        private JSContext _ctx;
        private JSValue _this_obj = JSApi.JS_UNDEFINED;

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

        private bool _onBeforeSerializeValid;
        private JSValue _onBeforeSerializeFunc;

        private bool _onAfterDeserializeValid;
        private JSValue _onAfterDeserializeFunc;

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

        public JSValue GetProperty(string key)
        {
            return JSApi.JS_GetPropertyStr(_ctx, _this_obj, key);
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

        // 在 gameObject 上创建一个新的脚本组件实例
        // ctor: js class
        public static JSValue CreateScriptInstance(GameObject gameObject, JSContext ctx, JSValue ctor, bool bSetupCallbacks)
        {
            if (JSApi.JS_IsConstructor(ctx, ctor) == 1)
            {
                var header = JSApi.jsb_get_payload_header(ctor);
                if (header.type_id == BridgeObjectType.None) // it's a plain js value
                {
                    var bridge = gameObject.AddComponent<JSBehaviour>();
                    return bridge.CreateScriptInstance(ctx, ctor, bSetupCallbacks);
                }
            }

            return JSApi.JS_UNDEFINED;
        }

        public void CreateUnresolvedScriptInstance()
        {
            _isScriptInstanced = true;
        }

        // 在当前 JSBehaviour 实例上创建一个脚本实例并与之绑定
        public JSValue CreateScriptInstance(JSContext ctx, JSValue ctor, bool bSetupCallbacks)
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
                        CreateUnresolvedScriptInstance();
                    }
                    else
                    {
                        cache.AddJSValue(this, val);
                        this.SetScriptInstance(ctx, val, bSetupCallbacks);
                        // JSApi.JSB_SetBridgeType(ctx, val, type_id);
                    }

                    return val;
                }
            }

            CreateUnresolvedScriptInstance();
            return JSApi.JS_UNDEFINED;
        }

        public void SetScriptInstance(JSContext ctx, JSValue this_obj, bool bSetupCallbacks)
        {
            if (_released)
            {
                return;
            }

            var context = ScriptEngine.GetContext(ctx);
            if (context == null)
            {
                return;
            }

            ReleaseJSValues();
            if (_ctx != (JSContext)ctx)
            {
                var oldContext = ScriptEngine.GetContext(_ctx);
                if (oldContext != null)
                {
                    oldContext.OnDestroy -= OnContextDestroy;
                }
                context.OnDestroy += OnContextDestroy;
                _ctx = ctx;
            }

            _isScriptInstanced = true;
            _this_obj = JSApi.JS_DupValue(ctx, this_obj);

            _onBeforeSerializeFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnBeforeSerialize"));
            _onBeforeSerializeValid = JSApi.JS_IsFunction(ctx, _onBeforeSerializeFunc) == 1;
            _onAfterDeserializeFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnAfterDeserialize"));
            _onAfterDeserializeValid = JSApi.JS_IsFunction(ctx, _onAfterDeserializeFunc) == 1;

            if (bSetupCallbacks)
            {
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

#if UNITY_EDITOR
                if (EditorApplication.isPlaying)
                {
#endif
                    var awakeFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("Awake"));

                    CallJSFunc(awakeFunc);
                    JSApi.JS_FreeValue(_ctx, awakeFunc);
                    if (enabled && _onEnableValid)
                    {
                        CallJSFunc(_onEnableFunc);
                    }
#if UNITY_EDITOR
                }
#endif
            }
        }

        private void CallJSFunc(JSValue func_obj)
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

        public void ReleaseJSValues()
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

            JSApi.JS_FreeValue(_ctx, _onBeforeSerializeFunc);
            _onBeforeSerializeFunc = JSApi.JS_UNDEFINED;
            _onBeforeSerializeValid = false;

            JSApi.JS_FreeValue(_ctx, _onAfterDeserializeFunc);
            _onAfterDeserializeFunc = JSApi.JS_UNDEFINED;
            _onAfterDeserializeValid = false;

            if (!_this_obj.IsNullish())
            {
                JSApi.JS_FreeValue(_ctx, _this_obj);
                _this_obj = JSApi.JS_UNDEFINED;
            }
        }

        void Release()
        {
            if (_released)
            {
                return;
            }
            _released = true;
            ReleaseJSValues();

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

        public void OnBeforeSerialize()
        {
            if (_onBeforeSerializeValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onBeforeSerializeFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }

        public void OnAfterDeserialize()
        {
            if (_onAfterDeserializeValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onAfterDeserializeFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }
    }
}