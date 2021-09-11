#if !JSB_UNITYLESS
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
    using QuickJS.Binding;

    public class ScriptEditorWindowLauncher : BaseEditorWindow
    {
        private GUIContent _scriptIcon;
        private Vector2 _sv;
        private List<JSScriptClassPathHint> _classPaths;

        void Awake()
        {
            var image = (Texture)AssetDatabase.LoadAssetAtPath("Assets/jsb/Editor/Icons/JsScript.png", typeof(Texture));
            _scriptIcon = new GUIContent(image);
            Reset();
        }

        void OnDestroy()
        {
            JSScriptFinder.GetInstance().ScriptClassPathsUpdated -= OnScriptClassPathsUpdated;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent("Launchpad", _scriptIcon.image);
        }

        private void OnScriptClassPathsUpdated()
        {
            _classPaths.Clear();
            JSScriptFinder.GetInstance().Search(JSScriptClassType.EditorWindow, _classPaths);
        }

        private void DrawScriptItem(Rect rect, JSScriptClassPathHint classPath)
        {
            var labelHeight = Math.Min(EditorStyles.label.lineHeight, rect.height);
            var padding = 4f;
            var buttonSize = rect.height - labelHeight - padding;
            var name = classPath.className;

            if (buttonSize > 8f)
            {
                var buttonRect = new Rect(rect.x + (rect.width - buttonSize) * .5f, rect.y, buttonSize, buttonSize);
                
                if (GUI.Button(buttonRect, _scriptIcon))
                {
                    EditorRuntime.ShowWindow(classPath.modulePath, classPath.className);
                }

                var labelRect = new Rect(rect.x, rect.yMax - labelHeight, rect.width, labelHeight);
                EditorGUI.LabelField(labelRect, name, EditorStyles.centeredGreyMiniLabel);
            }
            else 
            {
                if (GUI.Button(rect, name))
                {
                    EditorRuntime.ShowWindow(classPath.modulePath, classPath.className);
                }
            }
        }

        private void Reset()
        {
            JSScriptFinder.GetInstance().ScriptClassPathsUpdated -= OnScriptClassPathsUpdated;
            JSScriptFinder.GetInstance().ScriptClassPathsUpdated += OnScriptClassPathsUpdated;
            _classPaths = new List<JSScriptClassPathHint>();
            JSScriptFinder.GetInstance().Search(JSScriptClassType.EditorWindow, _classPaths);
        }

        protected override void OnPaint()
        {
            if (_classPaths == null)
            {
                Reset();
            }

            var size = _classPaths.Count;
            EditorGUILayout.HelpBox("ScriptEditorWindowLauncher is an experimental unfinished feature. it could be used to open editor windows implemented in typescript, we need this because there is no open api in Unity to dynamically create menu item at the moment.", MessageType.Warning);
            EditorGUILayout.HelpBox(string.Format("{0} EditorWindow Scripts", size), MessageType.Info);

            _sv = EditorGUILayout.BeginScrollView(_sv);
            var itemSize = new Vector2(120f, 80f);
            var rowRect = EditorGUILayout.GetControlRect(GUILayout.Height(itemSize.y));
            var itemRect = new Rect(rowRect.x, rowRect.y, itemSize.x, itemSize.y);
            for (var i = 0; i < size; i++)
            {
                var item = _classPaths[i];

                DrawScriptItem(itemRect, item);
                itemRect.x += itemSize.x;
                if (itemRect.xMax > rowRect.xMax)
                {
                    itemRect.x = rowRect.x;
                    itemRect.y += itemSize.y;
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
#endif
