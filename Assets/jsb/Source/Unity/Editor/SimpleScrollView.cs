#if !JSB_UNITYLESS
using System;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public class SimpleScrollView<T>
    {
        private Vector2 _scollPosition;
        private Rect _viewRect;
        private Rect _itemRect;
        private List<T> _items;
        private HashSet<T> _selected = new HashSet<T>();
        private float _itemHeight;
        private Color _rowColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);
        private Color _selectColor = new Color(44f / 255f, 93f / 255f, 135f / 255f);

        public Action<Rect, int, T> OnDrawItem;
        public Action<Rect, int, T, HashSet<T>> OnSelectItem;

        public SimpleScrollView()
        {
            _itemHeight = EditorGUIUtility.singleLineHeight;
            _items = new List<T>();
        }

        public int Count => _items.Count;

        public void Clear()
        {
            _items.Clear();
            _viewRect = new Rect(0f, 0f, 0f, 0f);
        }

        public void AddRange(System.Collections.Generic.IEnumerable<T> items)
        {
            _items.AddRange(items);
            _viewRect.height = _itemHeight * _items.Count;
        }

        public void Add(T item)
        {
            _items.Add(item);
            _viewRect.height = _itemHeight * _items.Count;
        }

        public void Draw(Rect rect)
        {
            _viewRect.width = rect.width - 16f;
            _scollPosition = GUI.BeginScrollView(rect, _scollPosition, _viewRect);
            var fromIndex = Mathf.Max(Mathf.FloorToInt(_scollPosition.y / _itemHeight), 0);
            var toIndex = Mathf.Min(fromIndex + Mathf.CeilToInt(rect.height / _itemHeight), _items.Count - 1);
            for (var i = fromIndex; i <= toIndex; ++i)
            {
                _itemRect.Set(0f, i * _itemHeight, rect.width, _itemHeight);

                var currentItem = _items[i];
                var isSelected = _selected.Contains(currentItem);

                if (isSelected)
                {
                    EditorGUI.DrawRect(_itemRect, _selectColor);
                }
                else
                {
                    if (i % 2 == 0)
                    {
                        EditorGUI.DrawRect(_itemRect, _rowColor);
                    }
                }
                OnDrawItem?.Invoke(_itemRect, i, currentItem);
                if (Event.current.type == EventType.MouseUp && !isSelected)
                {
                    if (_itemRect.Contains(Event.current.mousePosition))
                    {
                        _selected.Clear();
                        _selected.Add(currentItem);
                        OnSelectItem?.Invoke(_itemRect, i, currentItem, _selected);
                    }
                }
            }
            GUI.EndScrollView();
        }
    }
}
#endif
