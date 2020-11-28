using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    using QuickJS.Utils;
    using QuickJS.IO;
    using QuickJS;
    using QuickJS.Binding;
    using QuickJS.Native;

    //
    public class EditorRuntimeConsole : BaseEditorWindow
    {
        private string _text;
        private string _rvalToString;
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

        protected override void OnPaint()
        {
            _isEditorRuntime = EditorGUILayout.Toggle("EditorRuntime", _isEditorRuntime);
            if (_isEditorRuntime)
            {
                EditorRuntime.GetInstance();
            }
            var runtime = ScriptEngine.GetRuntime(_isEditorRuntime);
            var available = runtime != null;

            using (new EditorGUI.DisabledGroupScope(!available))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    _text = EditorGUILayout.TextField(">", _text);
                    if (GUILayout.Button("Run", GUILayout.Width(36f)))
                    {
                        using (var ret = runtime.GetMainContext().EvalSource<ScriptValue>(_text, "eval"))
                        {
                            _rvalToString += ret.JSONStringify();
                        }
                    }
                }
            }

            EditorGUILayout.TextArea(_rvalToString);

            if (!available)
            {
                EditorGUILayout.HelpBox("Runtime Not Available", MessageType.Warning);
            }
        }
    }
}

