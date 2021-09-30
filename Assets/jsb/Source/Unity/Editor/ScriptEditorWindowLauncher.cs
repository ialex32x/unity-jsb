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
        private int _selectedTabViewIndex;
        private string[] _tabViews = new string[] { "EditorWindow", "Editor", };

        private GUIContent _scriptIcon;
        private Vector2 _editorViewScrollPosition;
        private Vector2 _editorWindowViewScrollPosition;
        private List<JSScriptClassPathHint> _editorWindowClassPaths;
        private List<JSScriptClassPathHint> _editorClassPaths;

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
            _editorWindowClassPaths.Clear();
            JSScriptFinder.GetInstance().Search(JSScriptClassType.EditorWindow, _editorWindowClassPaths);
        }

        private void DrawEditorWindowScriptItem(Rect rect, JSScriptClassPathHint classPath)
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
            _editorClassPaths = new List<JSScriptClassPathHint>();
            _editorWindowClassPaths = new List<JSScriptClassPathHint>();
            JSScriptFinder.GetInstance().Search(JSScriptClassType.CustomEditor, _editorClassPaths);
            JSScriptFinder.GetInstance().Search(JSScriptClassType.EditorWindow, _editorWindowClassPaths);
        }

        private void DrawEditorWindowScripts()
        {
            _editorWindowViewScrollPosition = EditorGUILayout.BeginScrollView(_editorWindowViewScrollPosition);
            var size = _editorWindowClassPaths.Count;
            EditorGUILayout.HelpBox(string.Format("{0} EditorWindow Scripts", size), MessageType.Info);
            GUILayout.Space(12f);
            var itemSize = new Vector2(120f, 80f);
            var rowRect = EditorGUILayout.GetControlRect(GUILayout.Height(itemSize.y));
            var itemRect = new Rect(rowRect.x, rowRect.y, itemSize.x, itemSize.y);
            for (var i = 0; i < size; i++)
            {
                var item = _editorWindowClassPaths[i];

                DrawEditorWindowScriptItem(itemRect, item);
                itemRect.x += itemSize.x;
                if (itemRect.xMax > rowRect.xMax)
                {
                    itemRect.x = rowRect.x;
                    itemRect.y += itemSize.y;
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.HelpBox("ScriptEditorWindowLauncher is an experimental unfinished feature. it could be used to open editor windows implemented in typescript, we need this because there is no open api in Unity to dynamically create menu item at the moment.", MessageType.Warning);
        }

        private void DrawEditorScripts()
        {
            var size = _editorClassPaths.Count;
            
            _editorViewScrollPosition = EditorGUILayout.BeginScrollView(_editorViewScrollPosition);
            EditorGUILayout.HelpBox(string.Format("{0} Editor Scripts", size), MessageType.Info);
            GUILayout.Space(12f);
            for (var i = 0; i < size; i++)
            {
                var item = _editorClassPaths[i];

                EditorGUILayout.LabelField(item.ToClassPath());
            }
            EditorGUILayout.EndScrollView();
        }

        protected override void OnPaint()
        {
            if (_editorWindowClassPaths == null || _editorClassPaths == null)
            {
                Reset();
            }

            _selectedTabViewIndex = GUILayout.Toolbar(_selectedTabViewIndex, _tabViews);
            GUILayout.Space(12f);
            switch (_selectedTabViewIndex)
            {
                case 0: DrawEditorWindowScripts(); break;
                case 1: DrawEditorScripts(); break;
            }

        }
    }
}
#endif
