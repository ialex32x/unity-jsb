#if !JSB_UNITYLESS
using System;
using System.IO;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using UnityEditor;
    using UnityEngine;
    using Native;

    public abstract class JSInspectorBase<T> : Editor
    where T : Object, IScriptEditorSupport
    {
        protected T _target;
        protected JSScriptClassType _classType;

        private string[] _tabViews = new string[] { "Editor", "Source", "Primitive" };
        private int _selectedTabViewIndex = 0;

        private bool _enabled;
        private bool _enabledPending;
        private JSContext _ctx = JSContext.Null;
        private JSValue _this_obj = JSApi.JS_UNDEFINED;

        private bool _onDestroyValid;
        private JSValue _onDestroyFunc = JSApi.JS_UNDEFINED;

        private bool _onEnableValid;
        private JSValue _onEnableFunc = JSApi.JS_UNDEFINED;

        private bool _onDisableValid;
        private JSValue _onDisableFunc = JSApi.JS_UNDEFINED;

        private bool _onInspectorGUIValid;
        private JSValue _onInspectorGUIFunc = JSApi.JS_UNDEFINED;

        void Awake()
        {
            _target = target as T;
            _classType = GetScriptClassType();
        }

        protected abstract JSScriptClassType GetScriptClassType();

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

        private void OnScriptReloaded(ScriptContext context, string resolved_id)
        {
            if (context.CheckModuleId(_target.scriptRef, resolved_id))
            {
                if (_enabled)
                {
                    OnEnable();
                }
            }
        }

        private void OnContextDestroy(ScriptContext context)
        {
            Release();
        }

        private void ReleaseJSValues()
        {
            if (!_this_obj.IsNullish())
            {
                OnUnbindingJSMembers();

                JSApi.JS_FreeValue(_ctx, _this_obj);
                _this_obj = JSApi.JS_UNDEFINED;
            }

            var context = ScriptEngine.GetContext(_ctx);
            _ctx = JSContext.Null;

            if (context != null)
            {
                context.OnDestroy -= OnContextDestroy;
                context.OnScriptReloaded -= OnScriptReloaded;
            }
        }

        void Release()
        {
            if (!_target.isStandaloneScript)
            {
                _target.ReleaseScriptInstance();
            }
            ReleaseJSValues();
        }

        private void CreateScriptInstance(JSContext ctx, JSValue this_obj, JSValue ctor)
        {
            var context = ScriptEngine.GetContext(ctx);
            if (context == null || !context.IsValid())
            {
                return;
            }

            context.OnDestroy += OnContextDestroy;
            context.OnScriptReloaded += OnScriptReloaded;
            _ctx = ctx;
            _this_obj = JSApi.JS_DupValue(ctx, this_obj);

            if (!_this_obj.IsNullish())
            {
                OnBindindJSMembers(context);

                var awakeFunc = JSApi.JS_GetProperty(ctx, this_obj, context.GetAtom("Awake"));

                CallJSFunc(awakeFunc);
                JSApi.JS_FreeValue(_ctx, awakeFunc);
            }
        }

        private void OnBindindJSMembers(ScriptContext context)
        {
            var ctx = (JSContext)context;

            _onDestroyFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnDestroy"));
            _onDestroyValid = JSApi.JS_IsFunction(ctx, _onDestroyFunc) == 1;

            _onEnableFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnEnable"));
            _onEnableValid = JSApi.JS_IsFunction(ctx, _onEnableFunc) == 1;

            _onDisableFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnDisable"));
            _onDisableValid = JSApi.JS_IsFunction(ctx, _onDisableFunc) == 1;

            _onInspectorGUIFunc = JSApi.JS_GetProperty(ctx, _this_obj, context.GetAtom("OnInspectorGUI"));
            _onInspectorGUIValid = JSApi.JS_IsFunction(ctx, _onInspectorGUIFunc) == 1;
        }

        private void OnUnbindingJSMembers()
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
        }

        private void CreateScriptInstance()
        {
            ReleaseJSValues();

            var ctx = _target.ctx;
            if (!ctx.IsValid())
            {
                return;
            }

            var runtime = ScriptEngine.GetRuntime(ctx);
            if (runtime == null || !runtime.isRunning)
            {
                return;
            }

            var editorClass = JSApi.JS_UNDEFINED;
            var scriptFunc = runtime.GetMainContext().EvalSource<ScriptFunction>("require('plover/editor/editor_decorators').EditorUtil.getCustomEditor", "eval");
            if (scriptFunc != null)
            {
                unsafe
                {
                    JSValue targetValue = _target.ToValue();
                    if (targetValue.IsObject())
                    {
                        var args = stackalloc JSValue[] { targetValue };
                        editorClass = scriptFunc._Invoke(1, args);
                    }
                }
                scriptFunc.Dispose();
            }

            // var editorClass = _target.GetProperty("__editor__");
            if (JSApi.JS_IsConstructor(ctx, editorClass) == 1)
            {
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

        void OnEnable()
        {
            _enabled = true;
            _enabledPending = true;
            ReleaseJSValues();
            // EnableScriptInstance();
        }

        private void EnableScriptInstance()
        {
            if (!_enabledPending || !_enabled)
            {
                return;
            }

            // 当前编辑器为附加模式执行时, 等待目标脚本建立连接
            // 否则由编辑器管理目标生命周期

            if (_target.isStandaloneScript)
            {
                if (_target.isScriptInstanced)
                {
                    this.OnEnableScriptInstance();
                }
            }
            else
            {
                if (_target.CreateScriptInstance())
                {
                    this.OnEnableScriptInstance();
                }
            }
        }

        private void OnEnableScriptInstance()
        {
            _enabledPending = false;
            this.CreateScriptInstance();

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
            _enabledPending = false;
            _enabled = false;

            if (_onDisableValid)
            {
                var rval = JSApi.JS_Call(_ctx, _onDisableFunc, _this_obj);
                if (rval.IsException())
                {
                    _ctx.print_exception();
                }

                JSApi.JS_FreeValue(_ctx, rval);
            }

            Release();
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

            _enabledPending = false;
            Release();
        }

        private void OnSelectedScript(JSScriptClassPathHint classPath)
        {
            if (_enabled && _target != null && !classPath.IsReferenced(_target.scriptRef))
            {
                var scriptRef = _target.scriptRef;

                scriptRef.sourceFile = classPath.sourceFile;
                if (scriptRef.modulePath != classPath.modulePath || scriptRef.className != classPath.className)
                {
                    scriptRef.modulePath = classPath.modulePath;
                    scriptRef.className = classPath.className;

                    _target.scriptRef = scriptRef;
                    this.ReleaseJSValues();
                    _target.ReleaseScriptInstance();
                    _target.CreateScriptInstance();

                    // 重新绑定当前编辑器脚本实例
                    this.CreateScriptInstance();
                    EditorUtility.SetDirty(_target);
                }
                else
                {
                    _target.scriptRef = scriptRef;
                }

                EditorUtility.SetDirty(_target);
            }
        }

        private void DrawSourceView()
        {
            // EditorGUI.BeginDisabledGroup(_target.isStandaloneScript);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Source File", _target.scriptRef.sourceFile);
            var sourceFileRect = GUILayoutUtility.GetLastRect();

            if (GUILayout.Button("F", GUILayout.Width(20f)))
            {
                sourceFileRect.y += 10f;
                if (JSScriptSearchWindow.Show(sourceFileRect, string.Empty, _classType, OnSelectedScript))
                {
                    GUIUtility.ExitGUI();
                }
            }

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_target.scriptRef.sourceFile))
            {
                var sourceFileExists = File.Exists(_target.scriptRef.sourceFile);

                if (!sourceFileExists)
                {
                    EditorGUILayout.HelpBox("Source file is missing", MessageType.Warning);
                }
            }
            else
            {
                if (EditorApplication.isPlaying)
                {
                    EditorGUILayout.HelpBox("Script instance without relevant script reference info?", MessageType.Warning);
                }
            }

            EditorGUILayout.LabelField("Module Path", _target.scriptRef.modulePath);
            EditorGUILayout.LabelField("Class Name", _target.scriptRef.className);
            // EditorGUI.EndDisabledGroup();
        }

        private void DrawPrimitiveView()
        {
            var ps = _target.properties;
            if (ps == null || ps.IsEmpty)
            {
                EditorGUILayout.HelpBox("Empty Properties View", MessageType.Info);
                return;
            }

            ps.ForEach((string key, Object value) =>
            {
                //
                EditorGUILayout.ObjectField(key, value, value != null ? value.GetType() : typeof(Object), true);
            });

            ps.ForEach((string key, string value) =>
            {
                //
                EditorGUILayout.LabelField(key);
                EditorGUILayout.TextArea(value);
            });

            ps.ForEach((string key, int value) =>
            {
                // unsafe
                EditorGUILayout.IntField(key, value);
            });

            ps.ForEach((string key, float value) =>
            {
                // unsafe
                EditorGUILayout.FloatField(key, value);
            });
        }

        private void DrawScriptingView()
        {
            if (_target.isScriptInstanced)
            {
                if (_target.IsValid())
                {
                    if (_onInspectorGUIValid)
                    {
                        var rval = JSApi.JS_Call(_ctx, _onInspectorGUIFunc, _this_obj);
                        if (rval.IsException())
                        {
                            _ctx.print_exception();
                        }

                        JSApi.JS_FreeValue(_ctx, rval);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("No inspector available for this script type", MessageType.Info);
                    }
                }
                else
                {
                    if (!_target.scriptRef.IsEmpty())
                    {
                        EditorGUILayout.HelpBox("Invalid script reference", MessageType.Warning);
                    }
                    DrawSourceView();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Waiting for script instancing...", MessageType.Warning);
                OnWaitingForScriptInstancing();
            }
        }

        protected virtual void OnWaitingForScriptInstancing()
        {
        }

        public override void OnInspectorGUI()
        {
            if (EditorApplication.isCompiling || (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode))
            {
                Release();
                EditorGUILayout.HelpBox("Temporarily unavailable in the script compilation process", MessageType.Warning);
                return;
            }

            if (_enabledPending)
            {
                EnableScriptInstance();
            }

            _selectedTabViewIndex = GUILayout.Toolbar(_selectedTabViewIndex, _tabViews);
            switch (_selectedTabViewIndex)
            {
                case 0: DrawScriptingView(); break;
                case 1: DrawSourceView(); break;
                default: DrawPrimitiveView(); break;
            }
        }
    }
}
#endif
