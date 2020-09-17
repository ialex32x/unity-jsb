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

    public class BaseEditorWindow : EditorWindow
    {
        protected static GUIStyle _blockStyle = new GUIStyle();
        protected List<Action> _defers = new List<Action>();

        protected virtual void OnEnable()
        {
            _blockStyle.normal.background = MakeTex(100, 100, new Color32(56, 56, 56, 0));
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnPaint()
        {
        }

        private void OnGUI()
        {
            OnPaint();
            ExecuteDefers();
        }

        protected Texture2D MakeTex(int width, int height, Color fillColor)
        {
            var pixels = new Color[width * height];
            for (var x = 0; x < width; ++x)
            {
                for (var y = 0; y < height; ++y)
                {
                    var point = x + y * width;
                    pixels[point] = fillColor;
                }
            }
            var result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        protected void BorderLine(Rect rect)
        {
            Handles.color = Color.black;
            Handles.DrawLine(new Vector3(rect.xMin, rect.yMin + rect.height * 0.5f), new Vector3(rect.xMax, rect.yMin + rect.height * 0.5f));
            Handles.color = Color.gray;
            Handles.DrawLine(new Vector3(rect.xMin + 1f, rect.yMin + rect.height * 0.5f + 1f), new Vector3(rect.xMax, rect.yMin + rect.height * 0.5f + 1f));
        }

        protected void BorderLine(float x1, float y1, float x2, float y2)
        {
            Handles.color = Color.black;
            Handles.DrawLine(new Vector3(x1, y1), new Vector3(x2, y2));
            Handles.color = Color.gray;
            Handles.DrawLine(new Vector3(x1 + 1f, y1 + 1f), new Vector3(x2, y2));
        }

        protected void Block(string title, Action contentDrawer, Action[] utilities, Action tailUtility)
        {
            var li = new Action[utilities.Length + 1];
            for (var i = 0; i < utilities.Length; i++)
            {
                li[i] = utilities[i];
            }
            li[utilities.Length] = tailUtility;
            Block(title, Color.clear, contentDrawer, li);
        }

        protected void Block(string title, Action contentDrawer, params Action[] utilities)
        {
            Block(title, Color.clear, contentDrawer, utilities);
        }

        protected void Block(string title, Color titleColor, Action contentDrawer, params Action[] utilities)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10f);
            EditorGUILayout.BeginVertical(_blockStyle);
            EditorGUILayout.BeginHorizontal();
            var guiColor = GUI.color;
            if (titleColor != Color.clear)
            {
                GUI.color = titleColor;
            }
            GUILayout.Label(title, GUILayout.ExpandWidth(false));
            GUI.color = guiColor;
            var rectBegin = EditorGUILayout.GetControlRect(true, GUILayout.ExpandWidth(true));
            var handlesColor = Handles.color;
            BorderLine(rectBegin);
            Handles.color = handlesColor;
            for (var i = 0; i < utilities.Length; i++)
            {
                utilities[i]();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10f);
            EditorGUILayout.BeginVertical();
            contentDrawer();
            EditorGUILayout.EndVertical();
            GUILayout.Space(4f);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10f);
            var rectEnd = EditorGUILayout.GetControlRect(true, GUILayout.Height(1f));
            BorderLine(rectEnd);
            BorderLine(rectEnd.xMin, rectBegin.yMax, rectEnd.xMin, rectEnd.yMax);
            BorderLine(rectEnd.xMax, (rectBegin.yMin + rectBegin.yMax) * 0.5f, rectEnd.xMax, rectEnd.yMax);
            Handles.color = handlesColor;
            GUILayout.Space(2f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        protected void Defer(Action action)
        {
            _defers.Add(action);
        }

        protected void ExecuteDefers()
        {
            var size = _defers.Count;
            if (size > 0)
            {
                var list = new Action[size];
                _defers.CopyTo(list, 0);
                _defers.Clear();
                for (var i = 0; i < size; i++)
                {
                    list[i]();
                }
            }
        }
    }
}
