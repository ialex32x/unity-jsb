#if !JSB_UNITYLESS
using System;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using Native;
    using UnityEngine;
    using UnityEngine.Serialization;

    // 实际 ScriptableObject.OnAfterDeserialize/OnEnable 可能早于 Runtime 初始化
    // 脚本回调与 C# 版本不完全一致

    [CreateAssetMenu(fileName = "js_data", menuName = "JSScriptableObject Asset", order = 100)]
    public class JSScriptableObject : ScriptableObject, ISerializationCallbackReceiver, IScriptEditorSupport
    {
        // 在编辑器运行时下与 js 脚本建立链接关系
        [SerializeField]
        [FormerlySerializedAs("scriptRef")]
        private JSScriptRef _scriptRef;

        [SerializeField]
        private JSScriptProperties _properties;

        // internal use only
        public JSScriptProperties properties => _properties;

        private bool _enabled;

        public bool enabled => _enabled;

        private bool _isScriptInstanced = false;

        private bool _isWaitingRuntime = false;

        public bool isScriptInstanced => _isScriptInstanced;

        // self controlled script instance lifetime 
        public bool isStandaloneScript => true;

        JSScriptRef IScriptEditorSupport.scriptRef { get { return _scriptRef; } set { _scriptRef = value; } }

        public JSContext ctx => _ctx;

        private JSContext _ctx = JSContext.Null;
        private JSValue _this_obj = JSApi.JS_UNDEFINED;

        private bool _resetValid;
        private JSValue _resetFunc = JSApi.JS_UNDEFINED;

        private bool _onBeforeSerializeValid;
        private JSValue _onBeforeSerializeFunc = JSApi.JS_UNDEFINED;

        private bool _onAfterDeserializeValid;
        private JSValue _onAfterDeserializeFunc = JSApi.JS_UNDEFINED;

        public JSValue ToValue()
        {
            return _this_obj;
        }

        public bool IsValid()
        {
            return _ctx.IsValid() && !_this_obj.IsNullish();
        }

        public void ReleaseJSValues()
        {
            if (!_this_obj.IsNullish())
            {
                JSApi.JS_FreeValue(_ctx, _onBeforeSerializeFunc);
                _onBeforeSerializeFunc = JSApi.JS_UNDEFINED;
                _onBeforeSerializeValid = false;

                JSApi.JS_FreeValue(_ctx, _resetFunc);
                _resetFunc = JSApi.JS_UNDEFINED;
                _resetValid = false;

                JSApi.JS_FreeValue(_ctx, _onAfterDeserializeFunc);
                _onAfterDeserializeFunc = JSApi.JS_UNDEFINED;
                _onAfterDeserializeValid = false;

                JSApi.JS_FreeValue(_ctx, _this_obj);
                _this_obj = JSApi.JS_UNDEFINED;
            }

            var context = ScriptEngine.GetContext(_ctx);
            _isScriptInstanced = false;
            if (_isWaitingRuntime)
            {
                _isWaitingRuntime = false;
                ScriptEngine.RuntimeCreated -= OnRuntimeCreated;
            }
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

#if UNITY_EDITOR
        private void OnScriptReloading(ScriptContext context, string resolved_id)
        {
            if (context.CheckModuleId(_scriptRef, resolved_id))
            {
                OnBeforeSerialize();
            }
        }

        private void OnScriptReloaded(ScriptContext context, string resolved_id)
        {
            if (context.CheckModuleId(_scriptRef, resolved_id))
            {
                ReleaseJSValues();
                CreateScriptInstance();
            }
        }
#endif

        private void OnContextDestroy(ScriptContext context)
        {
            ReleaseJSValues();
        }

        // 通过 scriptRef 还原脚本绑定关系
        public bool CreateScriptInstance()
        {
            if (!_isScriptInstanced)
            {
                if (!string.IsNullOrEmpty(_scriptRef.modulePath) && !string.IsNullOrEmpty(_scriptRef.className))
                {
                    var runtime = ScriptEngine.GetRuntime();
                    if (runtime != null)
                    {
                        var context = runtime.isInitialized ? runtime.GetMainContext() : null;
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
                        if (!_isWaitingRuntime)
                        {
                            _isWaitingRuntime = true;
                            ScriptEngine.RuntimeCreated += OnRuntimeCreated;
                        }
                        // Debug.LogError("script runtime not ready");
                    }
                }
                else
                {
                    SetUnresolvedScriptInstance();
                }
            }

            return _isScriptInstanced;
        }

        private void OnRuntimeCreated(ScriptRuntime runtime)
        {
            if (_isWaitingRuntime)
            {
                runtime.OnInitialized += OnRuntimeInitialized;
            }
        }

        private void OnRuntimeInitialized(ScriptRuntime runtime)
        {
            if (_isWaitingRuntime)
            {
                _isWaitingRuntime = false;
                ScriptEngine.RuntimeCreated -= OnRuntimeCreated;
                runtime.OnInitialized -= OnRuntimeInitialized;
                CreateScriptInstance();
            }
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

                _resetFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("Reset"));
                _resetValid = JSApi.JS_IsFunction(ctx, _resetFunc) == 1;

                _onAfterDeserializeFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnAfterDeserialize"));
                _onAfterDeserializeValid = JSApi.JS_IsFunction(ctx, _onAfterDeserializeFunc) == 1;

                this._OnScriptingAfterDeserialize();
            }
        }

        private void SetUnresolvedScriptInstance()
        {
            _isScriptInstanced = true;
        }

        public void ReleaseScriptInstance()
        {
            _isScriptInstanced = false;
            ReleaseJSValues();
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

        void OnEnable()
        {
            _enabled = true;
            CreateScriptInstance();
        }

        void OnDisable()
        {
            _enabled = false;
            ReleaseJSValues();
        }
    }
}
#endif
