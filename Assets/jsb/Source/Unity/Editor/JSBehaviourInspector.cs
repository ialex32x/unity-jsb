#if !JSB_UNITYLESS
using System;
using System.Collections.Generic;
using System.IO;

namespace QuickJS.Unity
{
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;
    using Native;

    [CustomEditor(typeof(JSBehaviour))]
    public class JSBehaviourInspector : Editor
    {
        private JSBehaviour _target;

        private bool _psView;
        private bool _foldoutObjects;
        private bool _foldoutStrings;
        private bool _foldoutNumbers;
        private bool _foldoutSourceRef;

        private bool _enabled;
        private bool _enabledPending;
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

        void Awake()
        {
            _target = target as JSBehaviour;
        }

        public void CreateScriptInstance(JSContext ctx, JSValue this_obj, JSValue ctor)
        {
            var context = ScriptEngine.GetContext(ctx);
            if (context == null)
            {
                return;
            }

            context.OnDestroy += OnContextDestroy;
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
                
                var context = ScriptEngine.GetContext(_ctx);
                if (context != null)
                {
                    context.OnDestroy -= OnContextDestroy;
                }
            }
        }

        void Release()
        {
            _target.ReleaseScriptInstance();
            ReleaseJSValues();
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

        void OnEnable()
        {
            _enabled = true;
            _enabledPending = true;
            ReleaseJSValues();
            // EnableScriptInstance();
        }

        private void EnableScriptInstance()
        {
            if (!_enabledPending)
            {
                return;
            }

            if (_target.SetScriptInstance())
            {
                _enabledPending = false;
                CreateScriptInstance();

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
        }

        void OnDisable()
        {
            _enabledPending = false;
            if (_target.isScriptInstanced)
            {
                _target.OnBeforeSerialize();
            }

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
            _enabled = false;
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

        private void OnSelectedScript(JSScriptClassPathHint classPath)
        {
            if (_enabled && _target != null && !classPath.IsReferenced(_target.scriptRef))
            {
                _target.scriptRef.sourceFile = classPath.sourceFile;
                if (_target.scriptRef.modulePath != classPath.modulePath || _target.scriptRef.className != classPath.className)
                {
                    _target.scriptRef.modulePath = classPath.modulePath;
                    _target.scriptRef.className = classPath.className;

                    _target.ReleaseScriptInstance();
                    _target.SetScriptInstance();

                    // 重新绑定当前编辑器脚本实例
                    this.CreateScriptInstance();
                    EditorUtility.SetDirty(_target);
                }

                EditorUtility.SetDirty(_target);
            }
        }

        private void DrawSourceRef()
        {
            var showSourceRefEdit = !_target.IsValid();

            if (!showSourceRefEdit)
            {
                _foldoutSourceRef = EditorGUILayout.Foldout(_foldoutSourceRef, "Script Ref");
                showSourceRefEdit = _foldoutSourceRef;
            }

            if (showSourceRefEdit)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField("Source File", _target.scriptRef.sourceFile);
                var sourceFileRect = GUILayoutUtility.GetLastRect();

                if (GUILayout.Button("F", GUILayout.Width(20f)))
                {
                    sourceFileRect.y += 10f;
                    if (JSScriptSearchWindow.Show(sourceFileRect, _target.scriptRef.ToClassPath(), OnSelectedScript))
                    {
                        GUIUtility.ExitGUI();
                    }
                }

                EditorGUILayout.EndHorizontal();

                var sourceFileExists = File.Exists(_target.scriptRef.sourceFile);

                if (!sourceFileExists)
                {
                    EditorGUILayout.HelpBox("Source file is missing", MessageType.Warning);
                }

                EditorGUILayout.LabelField("Module Path", _target.scriptRef.modulePath);
                EditorGUILayout.LabelField("Class Name", _target.scriptRef.className);
            }
        }

        private void DrawPrimitiveView()
        {
            _psView = EditorGUILayout.Toggle("Primitive View", _psView);
            if (_psView)
            {
                var ps = _target.properties;
                if (ps == null || ps.Count == 0)
                {
                    EditorGUILayout.HelpBox("Empty Properties View", MessageType.Info);
                    return;
                }

                if (_foldoutObjects = EditorGUILayout.Foldout(_foldoutObjects, "Objects"))
                {
                    ps.ForEach((string key, Object value) =>
                    {
                        //
                        EditorGUILayout.ObjectField(key, value, value != null ? value.GetType() : typeof(Object), true);
                    });
                }

                if (_foldoutStrings = EditorGUILayout.Foldout(_foldoutStrings, "Strings"))
                {
                    ps.ForEach((string key, string value) =>
                    {
                        //
                        EditorGUILayout.LabelField(key);
                        EditorGUILayout.TextArea(value);
                    });
                }

                if (_foldoutNumbers = EditorGUILayout.Foldout(_foldoutNumbers, "Numbers"))
                {
                    ps.ForEach((string key, double value) =>
                    {
                        // unsafe
                        EditorGUILayout.FloatField(key, (float) value);
                    });
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (UnityEditor.EditorApplication.isCompiling)
            {
                Release();
                EditorGUILayout.HelpBox("Temporarily unavailable in the script compilation process", MessageType.Warning);
                return;
            }

            if (_enabledPending)
            {
                EnableScriptInstance();
            }

            DrawPrimitiveView();
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
#endif