#if !JSB_UNITYLESS
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    internal class SimpleTreeView
    {
        public interface INode
        {
            void AddChild(INode node);
            void Prepass(State state);
            void Draw(State state);
        }

        public class State
        {
            private float _indentWidth = 32f;
            private float _itemHeight = EditorGUIUtility.singleLineHeight;

            private int _index;
            private int _indent;
            private Vector2 _position;
            private Vector2 _scrollPosition;
            private float _maxWidth;
            private Rect _drawRect;
            private Rect _viewRect;
            private Rect _itemRect;
            private GUIStyle _itemStyle;

            private Color _rowColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);

            private int _fromIndex;
            private int _toIndex;
            private int _itemCount;

            public State()
            {
                _itemStyle = EditorStyles.label;
            }

            public void BeginPrepass()
            {
                _indent = 0;
                _itemCount = 0;
                _viewRect.Set(0f, 0f, 0f, 0f);
            }

            public void AddSpace(GUIContent content)
            {
                var itemWidth = _itemStyle.CalcSize(content).x;
                _viewRect.height += _itemHeight;
                var width = _indent * _indentWidth + itemWidth;
                _viewRect.width = Mathf.Max(width, _viewRect.width);
                _itemCount++;
            }

            public void EndPrepass()
            {

            }

            public void BeginView(Rect rect)
            {
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

            public void Draw(GUIContent content)
            {
                var index = _index++;
                var visible = index >= _fromIndex && index <= _toIndex;

                if (visible)
                {
                    if (index % 2 == 0)
                    {
                        _itemRect.Set(0, index * _itemHeight, _drawRect.width, _itemHeight);
                        EditorGUI.DrawRect(_itemRect, _rowColor);
                    }
                    var x = _indent * _indentWidth;
                    _itemRect.Set(x, index * _itemHeight, _maxWidth - x, _itemHeight);
                    GUI.Label(_itemRect, content, _itemStyle);
                }
            }
        }

        private State _state;
        private List<INode> _children = new List<INode>();

        public void Add(INode node)
        {
            _children.Add(node);
        }

        public void Invalidate()
        {
            if (_state == null)
            {
                _state = new State();
            }
            _state.BeginPrepass();
            for (int i = 0, count = _children.Count; i < count; ++i)
            {
                var child = _children[i];
                child.Prepass(_state);
            }
            _state.EndPrepass();
        }

        public void Draw(Rect rect)
        {
            if (_state == null)
            {
                _state = new State();
            }
            _state.BeginView(rect);
            for (int i = 0, count = _children.Count; i < count; ++i)
            {
                var child = _children[i];
                child.Draw(_state);
            }
            _state.EndView();
        }
    }
}
#endif
