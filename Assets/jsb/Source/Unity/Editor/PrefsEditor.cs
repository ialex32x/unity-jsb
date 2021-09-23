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
    using UnityEditor.IMGUI.Controls;
    using QuickJS.Binding;

    // 配置编辑器
    public class PrefsEditor : BaseEditorWindow
    {
        internal abstract class TreeViewNode<T> : SimpleTreeView.INode
        {
            protected bool _expanded = true;

            protected T _value;
            protected GUIContent _content;
            protected List<SimpleTreeView.INode> _children = new List<SimpleTreeView.INode>();

            public void AddChild(SimpleTreeView.INode node)
            {
                _children.Add(node);
            }

            public void Prepass(SimpleTreeView.State state)
            {
                state.AddSpace(_content);
                if (_expanded)
                {
                    state.PushGroup();
                    for (int i = 0, count = _children.Count; i < count; ++i)
                    {
                        _children[i].Prepass(state);
                    }
                    state.PopGroup();
                }
            }

            public void Draw(SimpleTreeView.State state)
            {
                state.Draw(_content);
                if (_expanded)
                {
                    state.PushGroup();
                    for (int i = 0, count = _children.Count; i < count; ++i)
                    {
                        _children[i].Draw(state);
                    }
                    state.PopGroup();
                }
            }
        }

        internal class Type_TreeViewNode : TreeViewNode<Type>
        {
            public GUIContent content => _content;

            public string FullName => _value.FullName;

            public Type_TreeViewNode(Type type)
            {
                _value = type;
                _content = new GUIContent(type.Name);
            }

            public bool MatchString(string pattern)
            {
                return _value.FullName.Contains(pattern);
            }

            public override string ToString()
            {
                return _value.Name;
            }
        }

        internal class Namespace_TreeViewNode : TreeViewNode<string>
        {
            public Namespace_TreeViewNode(string ns)
            {
                _value = ns;
                _content = new GUIContent(ns);
            }

            public override string ToString()
            {
                return _value;
            }
        }

        internal class Assembly_TreeViewNode : TreeViewNode<Assembly>
        {
            private Dictionary<string, Namespace_TreeViewNode> _allNamespaces = new Dictionary<string, Namespace_TreeViewNode>();

            public Namespace_TreeViewNode GetNamespace_TreeViewNode(string ns)
            {
                var name = string.IsNullOrEmpty(ns) ? "-" : ns;
                Namespace_TreeViewNode node;
                if (!_allNamespaces.TryGetValue(name, out node))
                {
                    _allNamespaces[name] = node = new Namespace_TreeViewNode(name);
                    AddChild(node);
                }
                return node;
            }

            public Assembly_TreeViewNode(Assembly assembly)
            {
                _value = assembly;
                _content = new GUIContent(assembly.GetName().Name);
            }

            public override string ToString()
            {
                return _value.GetName().Name;
            }
        }

        private Prefs _prefs;
        private string _filePath;
        private bool _dirty;

        private GUIStyle _footStyle;
        private SearchField _searchField;
        private string _lastSearchString = string.Empty;

        private List<Type_TreeViewNode> _typeNodes = new List<Type_TreeViewNode>();

        private SimpleTreeView _treeView = new SimpleTreeView();
        private SimpleScrollView<Type_TreeViewNode> _listView = new SimpleScrollView<Type_TreeViewNode>();

        private Type_TreeViewNode CreateTypeNode(Type type)
        {
            var self = new Type_TreeViewNode(type);
            _typeNodes.Add(self);
            if (type.DeclaringType != null)
            {
                var enclosing = CreateTypeNode(type.DeclaringType);
                enclosing.AddChild(self);
                return enclosing;
            }

            return self;
        }

        private Assembly_TreeViewNode CreateAssemblyNode(Assembly assembly)
        {
            var types = assembly.GetExportedTypes();
            var node = new Assembly_TreeViewNode(assembly);

            foreach (var type in types)
            {
                if (type.IsGenericTypeDefinition)
                {
                    continue;
                }

                node.GetNamespace_TreeViewNode(type.Namespace).AddChild(CreateTypeNode(type));
            }
            return node;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent("JS Bridge Prefs");

            _prefs = UnityHelper.LoadPrefs(out _filePath);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                _treeView.Add(CreateAssemblyNode(assembly));
            }
            _treeView.Invalidate();
            _listView.OnDrawItem = (rect, index, node) => GUI.Label(rect, node.FullName, EditorStyles.label);
            _searchField = new SearchField();
            _searchField.autoSetFocusOnFindCommand = true;
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
                    var json = JsonUtility.ToJson(_prefs, true);
                    System.IO.File.WriteAllText(_filePath, json);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(exception);
                }
            }
        }

        private void OnDrawItem(Rect rect, Type type)
        {
            GUI.Label(rect, type.FullName);
        }

        protected override void OnPaint()
        {
            EditorGUILayout.HelpBox("(experimental) Editor for " + _filePath, MessageType.Warning);
            if (GUILayout.Button("Save"))
            {
                MarkAsDirty();
            }

            var searchFieldRect = EditorGUILayout.GetControlRect();
            var searchString = _searchField.OnGUI(searchFieldRect, _lastSearchString);
            if (_lastSearchString != searchString)
            {
                _lastSearchString = searchString;
                _listView.Clear();
                if (!string.IsNullOrEmpty(_lastSearchString))
                {
                    foreach (var t in _typeNodes)
                    {
                        if (t.MatchString(_lastSearchString))
                        {
                            _listView.Add(t);
                        }
                    }
                }
            }

            if (_footStyle == null)
            {
                _footStyle = new GUIStyle(EditorStyles.miniLabel);
                _footStyle.alignment = TextAnchor.MiddleRight;
            }

            var typesViewRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
            if (string.IsNullOrEmpty(_lastSearchString))
            {
                _treeView.Draw(typesViewRect);
                GUILayout.Label($"{_typeNodes.Count} Types", _footStyle);
            }
            else
            {
                _listView.Draw(typesViewRect);
                GUILayout.Label($"{_listView.Count} Types", _footStyle);
            }
        }
    }
}
#endif
