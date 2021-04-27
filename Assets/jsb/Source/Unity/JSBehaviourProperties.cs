#if !JSB_UNITYLESS
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
        public class ObjectKeyValuePair
        {
            public string key;
            public Object value;
        }

        [Serializable]
        public class NumberKeyValuePair
        {
            public string key;
            public double value;
        }

        [Serializable]
        public class StringKeyValuePair
        {
            public string key;
            public string value;
        }

        [SerializeField]
        private List<ObjectKeyValuePair> _objects;

        [SerializeField]
        private List<StringKeyValuePair> _strings;

        [SerializeField]
        private List<NumberKeyValuePair> _numbers;

        public int Count
        {
            get { return ObjectCount + StringCount + NumberCount; }
        }

        private int ObjectCount => _objects != null ? _objects.Count : 0;
        private int StringCount => _strings != null ? _strings.Count : 0;
        private int NumberCount => _numbers != null ? _numbers.Count : 0;

        public void ForEach(Action<string, Object> cb)
        {
            if (_objects != null)
            {
                for (int i = 0, count = _objects.Count; i < count; i++)
                {
                    var pair = _objects[i];
                    cb(pair.key, pair.value);
                }
            }
        }

        public void SetObject(string key, Object value)
        {
            if (_objects == null)
            {
                _objects = new List<ObjectKeyValuePair>();
            }

            var found = _objects.Find(pair => pair.key == key);
            if (found == null)
            {
                _objects.Add(new ObjectKeyValuePair { key = key, value = value });
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

        public void ForEach(Action<string, string> cb)
        {
            if (_strings != null)
            {
                for (int i = 0, count = _strings.Count; i < count; i++)
                {
                    var pair = _strings[i];
                    cb(pair.key, pair.value);
                }
            }
        }

        public void SetString(string key, string value)
        {
            if (_strings == null)
            {
                _strings = new List<StringKeyValuePair>();
            }

            var found = _strings.Find(pair => pair.key == key);
            if (found == null)
            {
                _strings.Add(new StringKeyValuePair { key = key, value = value });
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

        public void ForEach(Action<string, double> cb)
        {
            if (_numbers != null)
            {
                for (int i = 0, count = _numbers.Count; i < count; i++)
                {
                    var pair = _numbers[i];
                    cb(pair.key, pair.value);
                }
            }
        }

        public void SetNumber(string key, double value)
        {
            if (_numbers == null)
            {
                _numbers = new List<NumberKeyValuePair>();
            }

            var found = _numbers.Find(pair => pair.key == key);
            if (found == null)
            {
                _numbers.Add(new NumberKeyValuePair { key = key, value = value });
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
#endif
