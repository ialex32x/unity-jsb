using System;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using Native;
    using UnityEngine;

    [Serializable]
    public class JSBehaviourProperties
    {
        [Serializable]
        public class KeyValuePair<T>
        {
            public string key;
            public T value;
        }

        [SerializeField]
        private List<KeyValuePair<Object>> _objects;

        [SerializeField]
        private List<KeyValuePair<string>> _strings;

        [SerializeField]
        private List<KeyValuePair<double>> _numbers;

        public void SetObject(string key, Object value)
        {
            if (_objects == null)
            {
                _objects = new List<KeyValuePair<Object>>();
            }

            var found = _objects.Find(pair => pair.key == key);
            if (found == null)
            {
                _objects.Add(new KeyValuePair<Object> { key = key, value = value });
            }
            else
            {
                found.value = value;
            }
        }

        public Object GetObject(string key)
        {
            return _objects?.Find(pair => pair.key == key)?.value;
        }

        public void SetString(string key, string value)
        {
            if (_strings == null)
            {
                _strings = new List<KeyValuePair<string>>();
            }

            var found = _strings.Find(pair => pair.key == key);
            if (found == null)
            {
                _strings.Add(new KeyValuePair<string> { key = key, value = value });
            }
            else
            {
                found.value = value;
            }
        }

        public string GetString(string key)
        {
            return _strings?.Find(pair => pair.key == key)?.value;
        }

        public void SetNumber(string key, double value)
        {
            if (_numbers == null)
            {
                _numbers = new List<KeyValuePair<double>>();
            }

            var found = _numbers.Find(pair => pair.key == key);
            if (found == null)
            {
                _numbers.Add(new KeyValuePair<double> { key = key, value = value });
            }
            else
            {
                found.value = value;
            }
        }

        public double GetNumber(string key)
        {
            return _numbers?.Find(pair => pair.key == key)?.value ?? 0.0;
        }

        public void Clear()
        {
            _objects?.Clear();
            _strings?.Clear();
            _numbers?.Clear();
        }
    }
}
