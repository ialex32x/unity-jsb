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

    // 配置编辑器
    public class PrefsEditor : EditorWindow
    {
        public enum BindingGenMethod
        {
            None,
            Implicit,
            Explicit,
        }

        private Assembly[] _assemblies;
        private int _selectedIndex;
        private List<Type> _filteredTypes = new List<Type>();
        private Vector2 _sv;
        private Prefs _prefs;
        protected static GUIStyle _blockStyle = new GUIStyle();
        protected List<Action> _defers = new List<Action>();

        void OnEnable()
        {
            titleContent = new GUIContent("duktape.json");
            _prefs = Prefs.Load();
            _blockStyle.normal.background = MakeTex(100, 100, new Color32(56, 56, 56, 0));
            _assemblies = AppDomain.CurrentDomain.GetAssemblies();
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

        void OnGUI()
        {
            EditorGUILayout.HelpBox("(experimental) Editor for duktape.json", MessageType.Warning);
            EditorGUILayout.BeginHorizontal();
            Block("Assemblies", () =>
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Preview", GUILayout.Width(60f));
                EditorGUILayout.LabelField("Binding", GUILayout.Width(100f));
                EditorGUILayout.LabelField("Assembly Name");
                EditorGUILayout.EndHorizontal();
                _sv = EditorGUILayout.BeginScrollView(_sv);
                for (var i = 0; i < _assemblies.Length; i++)
                {
                    var a = _assemblies[i];
                    var name = a.GetName().Name;
                    var pMethod = BindingGenMethod.None;
                    if (_prefs.implicitAssemblies.Contains(name))
                    {
                        pMethod = BindingGenMethod.Implicit;
                    }
                    else if (_prefs.explicitAssemblies.Contains(name))
                    {
                        pMethod = BindingGenMethod.Explicit;
                    }
                    EditorGUI.BeginDisabledGroup(a.IsDynamic);
                    EditorGUILayout.BeginHorizontal();
                    var tRect = EditorGUILayout.GetControlRect(GUILayout.Width(60f));
                    tRect.xMin = tRect.xMax - 20f;
                    var selected = EditorGUI.Toggle(tRect, _selectedIndex == i);
                    var rMethod = (BindingGenMethod)EditorGUILayout.EnumPopup(pMethod, GUILayout.Width(100f));
                    if (selected)
                    {
                        if (_selectedIndex != i)
                        {
                            _selectedIndex = i;
                            FilterTypes();
                        }
                    }
                    EditorGUILayout.TextField(name);
                    if (rMethod != pMethod)
                    {
                        switch (rMethod)
                        {
                            case BindingGenMethod.None:
                                if (pMethod == BindingGenMethod.Implicit)
                                {
                                    _prefs.implicitAssemblies.Remove(name);
                                    _prefs.MarkAsDirty();
                                }
                                else if (pMethod == BindingGenMethod.Explicit)
                                {
                                    _prefs.explicitAssemblies.Remove(name);
                                    _prefs.MarkAsDirty();
                                }
                                break;
                            case BindingGenMethod.Implicit:
                                if (pMethod == BindingGenMethod.Explicit)
                                {
                                    _prefs.explicitAssemblies.Remove(name);
                                }
                                _prefs.implicitAssemblies.Add(name);
                                _prefs.MarkAsDirty();
                                break;
                            case BindingGenMethod.Explicit:
                                if (pMethod == BindingGenMethod.Implicit)
                                {
                                    _prefs.implicitAssemblies.Remove(name);
                                }
                                _prefs.explicitAssemblies.Add(name);
                                _prefs.MarkAsDirty();
                                break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUILayout.EndScrollView();
            }, () =>
            {
                var color = GUI.color;
                GUI.color = Color.green;
                if (GUILayout.Button("R", GUILayout.Width(20f)))
                {
                    if (EditorUtility.DisplayDialog("Reload", "Reload duktape.json?", "ok", "cancel"))
                    {
                        Defer(() => _prefs = Prefs.Load());
                    }
                }
                GUI.color = color;
            }, () =>
            {
                var color = GUI.color;
                GUI.color = Color.yellow;
                if (GUILayout.Button("G", GUILayout.Width(20f)))
                {
                    if (EditorUtility.DisplayDialog("Generate", "Generate all binding code?", "ok", "cancel"))
                    {
                        Defer(() => UnityHelper.GenerateBindings());
                    }
                }
                GUI.color = color;
            }, () =>
            {
                var color = GUI.color;
                GUI.color = Color.red;
                if (GUILayout.Button("C", GUILayout.Width(20f)))
                {
                    if (EditorUtility.DisplayDialog("Cleanup", "Cleanup generated binding code?", "ok", "cancel"))
                    {
                        Defer(() => UnityHelper.ClearBindings());
                    }
                }
                GUI.color = color;
            });
            EditorGUILayout.BeginVertical();
            Block("Assembly Info", () =>
            {
                if (_selectedIndex >= 0 && _selectedIndex < _assemblies.Length)
                {
                    var assembly = _assemblies[_selectedIndex];
                    EditorGUILayout.TextField("Full Name", assembly.FullName);
                    EditorGUILayout.TextField("Location", assembly.Location, GUILayout.MinWidth(500f));
                }
            });
            Block("Types", () =>
            {
                var count = _filteredTypes.Count;
                for (var i = 0; i < count; i++)
                {
                    var type = _filteredTypes[i];
                    EditorGUILayout.TextField(type.FullName);
                }
                if (count == 0)
                {
                    EditorGUILayout.HelpBox("No type to bindgen.", MessageType.Info);
                }
            });
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            ExecuteDefers();
        }

        private void FilterTypes()
        {
            _filteredTypes.Clear();
            if (_selectedIndex >= 0 && _selectedIndex < _assemblies.Length)
            {
                var a = _assemblies[_selectedIndex];
                var types = a.GetExportedTypes();
                if (_prefs.implicitAssemblies.Contains(a.GetName().Name))
                {
                    for (var i = 0; i < types.Length; i++)
                    {
                        var type = types[i];
                        if (!InBlacklist(type))
                        {
                            _filteredTypes.Add(type);
                        }
                    }
                }
                else if (_prefs.explicitAssemblies.Contains(a.GetName().Name))
                {
                    for (var i = 0; i < types.Length; i++)
                    {
                        var type = types[i];
                        if (IsMarked(type))
                        {
                            _filteredTypes.Add(type);
                        }
                    }
                }

            }
        }

        private bool IsMarked(Type type)
        {
            return type.GetCustomAttribute(typeof(JSTypeAttribute)) != null;
        }

        private bool InBlacklist(Type type)
        {
            var list = _prefs.typePrefixBlacklist;
            for (int i = 0, size = list.Count; i < size; i++)
            {
                var item = list[i];
                if (type.FullName.StartsWith(item))
                {
                    return true;
                }
            }
            return false;
        }
    }
}