using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    using QuickJS.Utils;
    using QuickJS.IO;
    using QuickJS;
    using QuickJS.Binding;
    using QuickJS.Native;

    public class EditorRuntimeConsole : BaseEditorWindow
    {
        private string _text;
        private bool _isEditorRuntime = true;

        void Awake()
        {
        }

        void OnDestroy()
        {
        }

        protected override void OnEnable()
        {
            titleContent = new GUIContent("JS Console");
        }

        private void onEvalReturn(JSContext ctx, JSValue jsValue)
        {
            var logger = ScriptEngine.GetLogger(ctx);
            if (logger != null)
            {
                var ret = JSApi.GetString(ctx, jsValue);
                logger.Write(LogLevel.Info, ret);
            }
        }

        protected override void OnPaint()
        {
            _isEditorRuntime = EditorGUILayout.Toggle("EditorRuntime", _isEditorRuntime);
            var runtime = ScriptEngine.GetRuntime(_isEditorRuntime);
            var available = runtime != null;
            
            using (new EditorGUI.DisabledGroupScope(!available))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    _text = EditorGUILayout.TextField(">", _text);
                    if (GUILayout.Button("Run", GUILayout.Width(36f)))
                    {
                        runtime.GetMainContext().EvalSourceFree(_text, "eval", onEvalReturn);
                    }
                }
            }
            
            if (!available)
            {
                EditorGUILayout.HelpBox("Runtime Not Available", MessageType.Warning);
            }
        }
    }
}

