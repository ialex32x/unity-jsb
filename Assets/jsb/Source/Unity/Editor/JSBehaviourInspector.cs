using System;

namespace QuickJS.Unity
{
    using UnityEditor;
    using UnityEngine;
    using Native;

    [CustomEditor(typeof(JSBehaviour))]
    public class JSBehaviourInspector : Editor
    {
        private bool _replaceScriptInstance;
        private JSBehaviour _target;

        private bool _released;
        private JSContext _ctx;
        private JSValue _this_obj = JSApi.JS_UNDEFINED;

        private bool _onDestroyValid;
        private JSValue _onDestroyFunc = JSApi.JS_UNDEFINED;

        private bool _onEnableValid;
        private JSValue _onEnableFunc = JSApi.JS_UNDEFINED;

        private bool _onDisableValid;
        private JSValue _onDisableFunc = JSApi.JS_UNDEFINED;

        private bool _onInspectorGUIValid;
        private JSValue _onInspectorGUIFunc = JSApi.JS_UNDEFINED;

        public void CreateScriptInstance(JSContext ctx, JSValue this_obj, JSValue ctor)
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
            
            _ctx = ctx;
            _this_obj = JSApi.JS_DupValue(ctx, this_obj);

            _onDestroyFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnDestroy"));
            _onDestroyValid = JSApi.JS_IsFunction(ctx, _onDestroyFunc) == 1;

            _onEnableFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnEnable"));
            _onEnableValid = JSApi.JS_IsFunction(ctx, _onEnableFunc) == 1;

            _onDisableFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnDisable"));
            _onDisableValid = JSApi.JS_IsFunction(ctx, _onDisableFunc) == 1;

            _onInspectorGUIFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnInspectorGUI"));
            _onInspectorGUIValid = JSApi.JS_IsFunction(ctx, _onInspectorGUIFunc) == 1;

            var awakeFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("Awake"));

            CallJSFunc(awakeFunc);
            JSApi.JS_FreeValue(_ctx, awakeFunc);
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

        private void ReleaseJSValues()
        {
            if (_ctx.IsValid())
            {
                JSApi.JS_FreeValue(_ctx, _onDestroyFunc);
                _onDestroyFunc = JSApi.JS_UNDEFINED;
                _onDestroyValid = false;

                JSApi.JS_FreeValue(_ctx, _onEnableFunc);
                _onEnableFunc = JSApi.JS_UNDEFINED;
                _onEnableValid = false;

                JSApi.JS_FreeValue(_ctx, _onDisableFunc);
                _onDisableFunc = JSApi.JS_UNDEFINED;
                _onDisableValid = false;

                JSApi.JS_FreeValue(_ctx, _onInspectorGUIFunc);
                _onInspectorGUIFunc = JSApi.JS_UNDEFINED;
                _onInspectorGUIValid = false;

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

            _target.ReleaseScriptInstance();
            ReleaseJSValues();

            var context = ScriptEngine.GetContext(_ctx);
            if (context != null)
            {
                context.OnDestroy -= OnContextDestroy;
            }
        }

        private void CreateScriptInstance()
        {
            //TODO: 旧值释放
            ReleaseJSValues();

            var ctx = _target.ctx;
            if (ctx.IsValid())
            {
                var editorClass = _target.GetProperty("__editor__");
                if (JSApi.JS_IsConstructor(ctx, editorClass) == 1)
                {
                    var runtime = ScriptEngine.GetRuntime(ctx);
                    var objectCache = runtime.GetObjectCache();
                    
                    // 旧的绑定值释放？
                    if (!_this_obj.IsNullish())
                    {
                        var payload = JSApi.jsb_get_payload_header(_this_obj);
                        if (payload.type_id == BridgeObjectType.ObjectRef)
                        {

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
                    
                    var object_id = objectCache.AddObject(this, false);
                    var editorInstance = JSApi.jsb_construct_bridge_object(ctx, editorClass, object_id);
                    if (editorInstance.IsException())
                    {
                        ctx.print_exception();
                        objectCache.RemoveObject(object_id);
                    }
                    else
                    {
                        objectCache.AddJSValue(this, editorInstance);
                        CreateScriptInstance(ctx, editorInstance, editorClass);
                        JSApi.JS_FreeValue(ctx, editorInstance);
                    }
                }
                JSApi.JS_FreeValue(ctx, editorClass);
            }
        }

        void Awake()
        {
            _target = target as JSBehaviour;
            CreateScriptInstance();
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

        private void DrawSourceRef()
        {
            EditorGUILayout.BeginHorizontal();
            //TODO: 使用 SearchField 代替, 对脚本源代码做预处理以提供搜索
            var sourceFile = EditorGUILayout.TextField("SourceFile", _target.scriptRef.sourceFile);
            if (GUILayout.Button("R", GUILayout.Width(20f)))
            {
                string modulePath;
                string[] classNames;
                if (UnityHelper.ResolveScriptRef(_target.scriptRef.sourceFile, out modulePath, out classNames))
                {
                    if (_target.scriptRef.modulePath != modulePath || _target.scriptRef.className != classNames[0])
                    {
                        _target.scriptRef.modulePath = modulePath;
                        _target.scriptRef.className = classNames[0];
                        _replaceScriptInstance = true;
                        EditorUtility.SetDirty(_target);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Module", _target.scriptRef.modulePath);
            EditorGUILayout.LabelField("Class", _target.scriptRef.className);

            if (sourceFile != _target.scriptRef.sourceFile)
            {
                _target.scriptRef.sourceFile = sourceFile;
                EditorUtility.SetDirty(_target);
            }
        }

        public override void OnInspectorGUI()
        {
            if (UnityEditor.EditorApplication.isCompiling)
            {
                Release();
                EditorGUILayout.HelpBox("Temporarily unavailable", MessageType.Warning);
                return;
            }

            DrawSourceRef();
            if (_replaceScriptInstance || !_target.isScriptInstanced)
            {
                var context = ScriptEngine.GetContext();
                if (context != null && !string.IsNullOrEmpty(_target.scriptRef.modulePath) && !string.IsNullOrEmpty(_target.scriptRef.className))
                {
                    var ctx = (JSContext)context;
                    var snippet = $"require('{_target.scriptRef.modulePath}')['{_target.scriptRef.className}']";
                    var bytes = System.Text.Encoding.UTF8.GetBytes(snippet);
                    var typeValue = ScriptRuntime.EvalSource(ctx, bytes, _target.scriptRef.sourceFile, false);
                    if (JSApi.JS_IsException(typeValue))
                    {
                        var ex = ctx.GetExceptionString();
                        Debug.LogError(ex);
                        _target.CreateUnresolvedScriptInstance();
                    }
                    else
                    {
                        var instValue = _target.CreateScriptInstance(ctx, typeValue, false, false);
                        JSApi.JS_FreeValue(ctx, instValue);
                        JSApi.JS_FreeValue(ctx, typeValue);
                        CreateScriptInstance();

                        if (!instValue.IsObject())
                        {
                            Debug.LogError("script instance error");
                        }
                    }
                    _replaceScriptInstance = false;
                }
            }

            if (_onInspectorGUIValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onInspectorGUIFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }
                JSApi.JS_FreeValue(_ctx, rval);
            }
        }
    }
}
