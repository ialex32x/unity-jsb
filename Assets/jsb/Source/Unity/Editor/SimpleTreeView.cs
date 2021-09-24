#if !JSB_UNITYLESS
using System;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    internal class SimpleTreeView
    {
        public interface INode
        {
            GUIContent content { get; }
            bool isExpanded { get; set; }
            int childCount { get; }

            void AddChild(INode node);
            void Prepass(State state);
            void Render(State state);
            Vector2 CalcSize(GUIStyle style);

            bool CollapseAll();
            bool ExpandAll();

            void ShowContextMenu(State state);
        }

        public class State
        {
            private SimpleTreeView _treeView;
            private float _indentWidth = 16f;
            private float _itemHeight = EditorGUIUtility.singleLineHeight;

            private int _index;
            private int _indent;
            private float _leftPadding = 4f;
            private Vector2 _position;
            private Vector2 _scrollPosition;
            private float _maxWidth;
            private Rect _drawRect;
            private Rect _viewRect;
            private Rect _itemRect;
            private Rect _itemFullRect;
            private GUIStyle _itemStyle;

            private float _lastClickTime;

            private Color _rowColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);
            private Color _selectColor = new Color(44f / 255f, 93f / 255f, 135f / 255f);

            private int _fromIndex;
            private int _toIndex;
            private int _itemCount;
            private bool _repaint;

            private HashSet<INode> _selection = new HashSet<INode>();

            public bool repaint => _repaint;

            public State(SimpleTreeView treeView)
            {
                _treeView = treeView;
                _itemStyle = EditorStyles.label;
            }

            public void BeginPrepass()
            {
                _indent = 0;
                _itemCount = 0;
                _viewRect.Set(0f, 0f, 0f, 0f);
            }

            public void AddSpace(INode node)
            {
                var itemWidth = node.CalcSize(_itemStyle).x;
                _viewRect.height += _itemHeight;
                var width = _indent * _indentWidth + itemWidth;
                _viewRect.width = Mathf.Max(width, _viewRect.width);
                _itemCount++;
            }

            public void EndPrepass()
            {
                var yMax = _viewRect.height - _drawRect.height;
                if (_scrollPosition.y > yMax)
                {
                    _scrollPosition.y = yMax;
                }
            }

            public void BeginView(Rect rect)
            {
                _repaint = false;
                _drawRect = rect;
                _maxWidth = Mathf.Max(_drawRect.width, _viewRect.width);
                _scrollPosition = GUI.BeginScrollView(_drawRect, _scrollPosition, _viewRect);
                _fromIndex = Mathf.Max(Mathf.FloorToInt(_scrollPosition.y / _itemHeight) - 1, 0);
                _toIndex = Mathf.Min(_fromIndex + Mathf.CeilToInt(_drawRect.height / _itemHeight) + 1, _itemCount - 1);
                _index = 0;
                _indent = 0;
            }

            public void EndView()
            {
                GUI.EndScrollView();
            }

            public void PushGroup()
            {
                ++_indent;
            }

            public void PopGroup()
            {
                --_indent;
            }

            private bool CheckDoubleClick()
            {
                var rt = Time.realtimeSinceStartup;
                var dt = rt - _lastClickTime;
                if (dt < 0.2f)
                {
                    _lastClickTime = 0f;
                    return true;
                }
                _lastClickTime = rt;
                return false;
            }

            public void Render(INode node)
            {
                var index = _index++;
                var visible = index >= _fromIndex && index <= _toIndex;

                if (visible)
                {
                    var isSelected = _selection.Contains(node);
                    var isExpandable = node.childCount > 0;

                    if (isSelected)
                    {
                        _itemRect.Set(0, index * _itemHeight, _maxWidth, _itemHeight);
                        EditorGUI.DrawRect(_itemRect, _selectColor);
                    }
                    else
                    {
                        if (index % 2 == 0)
                        {
                            _itemRect.Set(0, index * _itemHeight, _maxWidth, _itemHeight);
                            EditorGUI.DrawRect(_itemRect, _rowColor);
                        }
                    }

                    var x = _indent * _indentWidth + _leftPadding;
                    var eventUsed = false;

                    if (isExpandable)
                    {
                        _itemRect.Set(x, index * _itemHeight, _itemHeight, _itemHeight);

                        var isExpanded = node.isExpanded;
                        GUI.DrawTexture(_itemRect, isExpanded ? UnityHelper.GetIcon("ExpandedItemIcon") : UnityHelper.GetIcon("CollapsedItemIcon"), ScaleMode.ScaleToFit);
                        if (!eventUsed && Event.current.type == EventType.MouseUp)
                        {
                            if (_itemRect.Contains(Event.current.mousePosition))
                            {
                                if (Event.current.button == 1)
                                {
                                    node.ShowContextMenu(this);
                                    eventUsed = true;
                                }
                                else
                                {
                                    node.isExpanded = !isExpanded;
                                    _treeView.Invalidate();
                                    _repaint = true;
                                    eventUsed = true;
                                }
                            }
                        }
                    }

                    _itemRect.Set(x + _itemHeight, index * _itemHeight, _maxWidth - x, _itemHeight);
                    EditorGUI.LabelField(_itemRect, node.content, _itemStyle);

                    if (!eventUsed && Event.current.type == EventType.MouseUp)
                    {
                        _itemRect.x = 0f;
                        if (_itemRect.Contains(Event.current.mousePosition))
                        {
                            if (Event.current.button == 1)
                            {
                                node.ShowContextMenu(this);
                                eventUsed = true;
                            }
                            else
                            {
                                if (CheckDoubleClick())
                                {
                                    node.isExpanded = !node.isExpanded;
                                    _treeView.Invalidate();
                                    _repaint = true;
                                    eventUsed = true;
                                }
                                else
                                {
                                    if (!isSelected)
                                    {
                                        _selection.Clear();
                                        _selection.Add(node);
                                        _treeView.OnSelectItem?.Invoke(_itemRect, index, node, _selection);
                                        _repaint = true;
                                        eventUsed = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public void CollapseAll()
            {
                var change = false;
                foreach (var child in _treeView._children)
                {
                    if (child.CollapseAll())
                    {
                        change = true;
                    }
                }

                if (change)
                {
                    _treeView.Invalidate();
                }
            }

            public void ExpandAll()
            {
                var change = false;
                foreach (var child in _treeView._children)
                {
                    if (child.ExpandAll())
                    {
                        change = true;
                    }
                }

                if (change)
                {
                    _treeView.Invalidate();
                }
            }
        }

        public Action<Rect, int, INode, HashSet<INode>> OnSelectItem;

        private State _state;
        private List<INode> _children = new List<INode>();

        public void Add(INode node)
        {
            if (node != null)
            {
                _children.Add(node);
            }
        }

        public void Invalidate()
        {
            if (_state == null)
            {
                _state = new State(this);
            }
            _state.BeginPrepass();
            for (int i = 0, count = _children.Count; i < count; ++i)
            {
                var child = _children[i];
                child.Prepass(_state);
            }
            _state.EndPrepass();
        }

        public bool Draw(Rect rect)
        {
            if (_state == null)
            {
                _state = new State(this);
            }
            _state.BeginView(rect);
            for (int i = 0, count = _children.Count; i < count; ++i)
            {
                var child = _children[i];
                child.Render(_state);
            }
            _state.EndView();

            return _state.repaint;
        }
    }
}
#endif
