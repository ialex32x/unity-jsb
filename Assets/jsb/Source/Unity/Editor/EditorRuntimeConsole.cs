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

    public class EditorRuntimeConsole : BaseEditorWindow
    {
        private string _text;

        protected override void OnEnable()
        {
            titleContent = new GUIContent("JS Console");
        }

        protected override void OnPaint()
        {
            EditorGUILayout.BeginHorizontal();
            _text = EditorGUILayout.TextField(">", _text);
            if (GUILayout.Button("Run", GUILayout.Width(36f)))
            {
                EditorRuntime.Eval(_text);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}

