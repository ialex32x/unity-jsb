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

            public T value => _value;

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
        private BindingManager _bindingManager;

        private int _selectedTabViewIndex;
        private GUIContent[] _tabViewNames = new GUIContent[] { };
        private Action[] _tabViewDrawers = new Action[] { };
        private string[] _newlineValues = new string[] { "cr", "lf", "crlf", "" };
        private string[] _newlineNames = new string[] { "UNIX", "MacOS", "Windows", "Auto" };

        private GUIStyle _footStyle;
        private SearchField _searchField;
        private string _lastSearchString = string.Empty;

        private List<Type_TreeViewNode> _typeNodes = new List<Type_TreeViewNode>();

        private SimpleTreeView _treeView = new SimpleTreeView();
        private SimpleScrollView<Type_TreeViewNode> _listView = new SimpleScrollView<Type_TreeViewNode>();
        private SimpleSplitView _splitView = new SimpleSplitView();

        public void AddTabView(string name, Action action)
        {
            ArrayUtility.Add(ref _tabViewNames, new GUIContent(name));
            ArrayUtility.Add(ref _tabViewDrawers, action);
        }

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

            _prefs = UnityHelper.LoadPrefs(out _filePath);
            _bindingManager = new BindingManager(_prefs, new BindingManager.Args());
            _bindingManager.Collect();
            _bindingManager.Generate(TypeBindingFlags.None);
            _bindingManager.Report();

            AddTabView("Codegen", DrawView_Codegen);
            AddTabView("Scripting", DrawView_Scripting);
            AddTabView("Types", DrawView_Types);
            AddTabView("Process", DrawView_Process);
            AddTabView("Options", DrawView_Options);
            OnDirtyStateChanged();
        }

        public void MarkAsDirty()
        {
            if (!_dirty)
            {
                _dirty = true;
                OnDirtyStateChanged();
                // EditorApplication.delayCall += Save;
            }
        }

        public void Save()
        {
            if (_dirty)
            {
                _dirty = false;
                OnDirtyStateChanged();
                try
                {
                    var json = JsonUtility.ToJson(_prefs, true);
                    System.IO.File.WriteAllText(_filePath, json);
                    Debug.LogFormat("save {0}", _filePath);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(exception);
                }
            }
        }

        private void OnDirtyStateChanged()
        {
            titleContent = new GUIContent("JS Bridge Prefs" + (_dirty ? " *" : ""));
        }

        private void OnDrawItem(Rect rect, Type type)
        {
            GUI.Label(rect, type.FullName);
        }

        private void DrawView_Scripting()
        {
            EditorGUI.BeginChangeCheck();
            _prefs.editorScripting = EditorGUILayout.Toggle("Editor Scripting", _prefs.editorScripting);
            _prefs.reflectBinding = EditorGUILayout.Toggle("Reflect Binding", _prefs.reflectBinding);
            _prefs.typescriptExt = EditorGUILayout.TextField("Typescript Ext", _prefs.typescriptExt);
            _prefs.sourceDir = EditorGUILayout.TextField("Source Dir", _prefs.sourceDir);
            _prefs.editorEntryPoint = EditorGUILayout.TextField("Editor Entry Script", _prefs.editorEntryPoint);
            if (EditorGUI.EndChangeCheck())
            {
                MarkAsDirty();
            }
        }

        private void DrawView_Process()
        {
            var list = _bindingManager.GetBindingProcessTypes();

            for (int i = 0, count = list.Count; i < count; ++i)
            {
                var process = list[i];
                var name = process.FullName;
                var enabled = !_prefs.skipBinding.Contains(name);
                var state = EditorGUILayout.ToggleLeft(name, enabled);
                if (state != enabled)
                {
                    if (state)
                    {
                        _prefs.skipBinding.Remove(name);
                    }
                    else
                    {
                        _prefs.skipBinding.Add(name);
                    }
                    MarkAsDirty();
                }
            }
        }

        private List<string> _repeatStringCache = new List<string>(new string[] { "" });
        private string RepeatString(string v, int repeat)
        {
            while (_repeatStringCache.Count < repeat + 1)
            {
                _repeatStringCache.Add(_repeatStringCache[_repeatStringCache.Count - 1] + v);
            }
            return _repeatStringCache[repeat];
        }

        private void DrawView_Codegen()
        {
            EditorGUI.BeginChangeCheck();
            _prefs.debugCodegen = EditorGUILayout.Toggle("Debug Codegen", _prefs.debugCodegen);
            _prefs.verboseLog = EditorGUILayout.Toggle("Verbose Log", _prefs.verboseLog);
            _prefs.optToString = EditorGUILayout.Toggle("Auto ToString", _prefs.optToString);
            _prefs.enableOperatorOverloading = EditorGUILayout.Toggle("Operator Overloading", _prefs.enableOperatorOverloading);
            _prefs.skipDelegateWithByRefParams = EditorGUILayout.Toggle("Omit ref param Delegates", _prefs.skipDelegateWithByRefParams);
            _prefs.alwaysCheckArgType = EditorGUILayout.Toggle("Always check arg type", _prefs.alwaysCheckArgType);
            _prefs.alwaysCheckArgc = EditorGUILayout.Toggle("Always check argc", _prefs.alwaysCheckArgc);
            _prefs.randomizedBindingCode = EditorGUILayout.Toggle("Obfuscate", _prefs.randomizedBindingCode);
            _prefs.genTypescriptDoc = EditorGUILayout.Toggle("Gen d.ts", _prefs.genTypescriptDoc);
            _prefs.xmlDocDir = EditorGUILayout.TextField("XmlDoc Dir", _prefs.xmlDocDir);
            _prefs.typescriptDir = EditorGUILayout.TextField("d.ts Output Dir", _prefs.typescriptDir);
            _prefs.outDir = EditorGUILayout.TextField("Output Dir", _prefs.outDir);
            _prefs.logPath = EditorGUILayout.TextField("Log", _prefs.logPath);
            _prefs.jsModulePackInfoPath = EditorGUILayout.TextField("JS Module List", _prefs.jsModulePackInfoPath);
            _prefs.typeBindingPrefix = EditorGUILayout.TextField("C# Binding Prefix", _prefs.typeBindingPrefix);
            _prefs.ns = EditorGUILayout.TextField("C# Binding Namespace", _prefs.ns);
            _prefs.defaultJSModule = EditorGUILayout.TextField("Default Module", _prefs.defaultJSModule);
            _prefs.tab = RepeatString(" ", EditorGUILayout.IntSlider("Tab Size", _prefs.tab.Length, 0, 8));
            var newlineIndex = Array.IndexOf(_newlineValues, _prefs.newLineStyle);
            var newlineIndex_t = EditorGUILayout.Popup("Newline Style", newlineIndex, _newlineNames);
            if (newlineIndex_t != newlineIndex && newlineIndex_t >= 0)
            {
                _prefs.newLineStyle = _newlineValues[newlineIndex_t];
            }

            if (EditorGUI.EndChangeCheck())
            {
                MarkAsDirty();
            }
        }

        private void DrawView_Options()
        {
            EditorGUI.BeginChangeCheck();
            if (EditorGUI.EndChangeCheck())
            {
                MarkAsDirty();
            }
        }

        private void DrawView_Types()
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(1f));
            var y = 90f;
            rect.height = position.height - rect.height;
            var repaint = _splitView.Draw(rect);

            GUILayout.BeginArea(new Rect(rect.x, y, _splitView.cursorChangeRect.x, rect.height));
            DrawView_Types_Left();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(_splitView.cursorChangeRect.x + rect.x, y, rect.width - _splitView.cursorChangeRect.xMax, rect.height));
            DrawView_Types_Right();
            GUILayout.EndArea();

            if (repaint)
            {
                Repaint();
            }
        }

        private void DrawView_Types_Right()
        {
            EditorGUILayout.LabelField("HI", "OK");
        }

        private void DrawView_Types_Left()
        {
            if (_searchField == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    _treeView.Add(CreateAssemblyNode(assembly));
                }
                _treeView.Invalidate();
                _listView.OnDrawItem = (rect, index, node) => GUI.Label(rect, node.FullName, EditorStyles.label);
                _listView.OnSelectItem = OnSelectListViewItem;
                _searchField = new SearchField();
                _searchField.autoSetFocusOnFindCommand = true;
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

        private void OnSelectListViewItem(Rect rect, int index, Type_TreeViewNode item, HashSet<Type_TreeViewNode> selection)
        {
            Repaint();
        }

        protected override void OnPaint()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!_dirty);
            if (GUILayout.Button("Save", EditorStyles.miniButton, GUILayout.Width(46f)))
            {
                Save();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("(experimental) Editor for " + _filePath, MessageType.Warning);

            _selectedTabViewIndex = GUILayout.Toolbar(_selectedTabViewIndex, _tabViewNames);
            _tabViewDrawers[_selectedTabViewIndex]();
        }
    }
}
#endif
