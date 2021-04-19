namespace QuickJS.Unity
{
    using UnityEditor;
    using UnityEngine;
    using Native;

    [CustomEditor(typeof(JSBehaviour))]
    public class JSBehaviourInspector : Editor
    {
        private JSBehaviour _target;

        private bool _released;
        private JSContext _ctx;
        private JSValue _this_obj;

        private bool _onDestroyValid;
        private JSValue _onDestroyFunc;

        private bool _onEnableValid;
        private JSValue _onEnableFunc;

        private bool _onDisableValid;
        private JSValue _onDisableFunc;

        private bool _onInspectorGUIValid;
        private JSValue _onInspectorGUIFunc;

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

            _onDestroyFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnDestroy"));
            _onDestroyValid = JSApi.JS_IsFunction(ctx, _onDestroyFunc) == 1;

            _onEnableFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnEnable"));
            _onEnableValid = JSApi.JS_IsFunction(ctx, _onEnableFunc) == 1;

            _onDisableFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnDisable"));
            _onDisableValid = JSApi.JS_IsFunction(ctx, _onDisableFunc) == 1;

            _onInspectorGUIFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("OnInspectorGUI"));
            _onInspectorGUIValid = JSApi.JS_IsFunction(ctx, _onInspectorGUIFunc) == 1;

            var awake_obj = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("Awake"));

            Call(awake_obj);
            JSApi.JS_FreeValue(_ctx, awake_obj);
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

            JSApi.JS_FreeValue(_ctx, _onDestroyFunc);
            _onDestroyValid = false;

            JSApi.JS_FreeValue(_ctx, _onEnableFunc);
            _onEnableValid = false;

            JSApi.JS_FreeValue(_ctx, _onDisableFunc);
            _onDisableValid = false;

            JSApi.JS_FreeValue(_ctx, _onInspectorGUIFunc);
            _onInspectorGUIValid = false;

            JSApi.JS_FreeValue(_ctx, _this_obj);

            var context = ScriptEngine.GetContext(_ctx);
            if (context != null)
            {
                context.OnDestroy -= OnContextDestroy;
            }
        }

        private void SetBridge()
        {
            //TODO: 旧值释放

            var ctx = _target.ctx;
            if (ctx.IsValid())
            {
                var editorClass = _target.GetProperty("__editor__");
                if (JSApi.JS_IsConstructor(ctx, editorClass) == 1)
                {
                    var cache = ScriptEngine.GetObjectCache(ctx);
                    var object_id = cache.AddObject(this, false);
                    var val = JSApi.jsb_construct_bridge_object(ctx, editorClass, object_id);
                    if (val.IsException())
                    {
                        ctx.print_exception();
                        cache.RemoveObject(object_id);
                    }
                    else
                    {
                        cache.AddJSValue(this, val);
                        this.SetBridge(ctx, val, editorClass);
                        JSApi.JS_FreeValue(ctx, val);
                    }
                }
                JSApi.JS_FreeValue(ctx, editorClass);
            }
        }

        void Awake()
        {
            _target = target as JSBehaviour;
            SetBridge();
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
                        EditorUtility.SetDirty(_target);
                    }

                    // ScriptInstance 释放的问题
                    // 
                    // var context = ScriptEngine.GetContext();
                    // if (context != null)
                    // {
                    //     var ctx = (JSContext)context;
                    //     var snippet = $"require('{_target.scriptRef.modulePath}')['{_target.scriptRef.className}']";
                    //     var bytes = System.Text.Encoding.UTF8.GetBytes(snippet);
                    //     var typeValue = ScriptRuntime.EvalSource(ctx, bytes, _target.scriptRef.sourceFile, false);
                    //     if (JSApi.JS_IsException(typeValue))
                    //     {
                    //         var ex = ctx.GetExceptionString();
                    //         Debug.LogError(ex);
                    //     }
                    //     else
                    //     {
                    //         var instValue = _target.RebindBridge(ctx, typeValue);
                    //         JSApi.JS_FreeValue(ctx, instValue);
                    //         JSApi.JS_FreeValue(ctx, typeValue);
                    //         // SetBridge();
                    //     }
                    // }
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
