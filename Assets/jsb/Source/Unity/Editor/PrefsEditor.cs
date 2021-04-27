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

    // 配置编辑器
    public class PrefsEditor : BaseEditorWindow
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
        private string _filePath;

        private bool _dirty;

        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent("JS Bridge Prefs");

            _prefs = UnityHelper.LoadPrefs(out _filePath);
            _assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

        public void MarkAsDirty()
        {
            if (!_dirty)
            {
                _dirty = true;
                EditorApplication.delayCall += Save;
            }
        }

        public void Save()
        {
            if (_dirty)
            {
                _dirty = false;
                try
                {
                    var json = JsonUtility.ToJson(this, true);
                    System.IO.File.WriteAllText(_filePath, json);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(exception);
                }
            }
        }

        protected override void OnPaint()
        {
            EditorGUILayout.HelpBox("(experimental) Editor for " + _filePath, MessageType.Warning);
            if (GUILayout.Button("Save"))
            {
                MarkAsDirty();
            }

            // ShowAssemblies();
        }

        private void ShowAssemblies()
        {
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
                                    MarkAsDirty();
                                }
                                else if (pMethod == BindingGenMethod.Explicit)
                                {
                                    _prefs.explicitAssemblies.Remove(name);
                                    MarkAsDirty();
                                }
                                break;
                            case BindingGenMethod.Implicit:
                                if (pMethod == BindingGenMethod.Explicit)
                                {
                                    _prefs.explicitAssemblies.Remove(name);
                                }
                                _prefs.implicitAssemblies.Add(name);
                                MarkAsDirty();
                                break;
                            case BindingGenMethod.Explicit:
                                if (pMethod == BindingGenMethod.Implicit)
                                {
                                    _prefs.implicitAssemblies.Remove(name);
                                }
                                _prefs.explicitAssemblies.Add(name);
                                MarkAsDirty();
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
                        Defer(() => _prefs = UnityHelper.LoadPrefs(out _filePath));
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
                        Defer(() => UnityHelper.GenerateBindingsAndTypeDefinition());
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
            return Attribute.GetCustomAttribute(type, typeof(JSTypeAttribute)) != null;
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
#endif
