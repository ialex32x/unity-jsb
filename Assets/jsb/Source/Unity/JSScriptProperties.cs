#if !JSB_UNITYLESS
using System;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using Native;
    using UnityEngine;

    [Serializable]
    public class JSScriptProperties
    {
        [Serializable]
        public class ObjectKeyValuePair
        {
            public string key;
            public Object value;
        }

        [SerializeField]
        private List<ObjectKeyValuePair> _objects;

        [SerializeField]
        public byte[] genericValueData;

        [SerializeField]
        public int dataFormat;

        public bool IsEmpty
        {
            get { return ObjectCount + GenericCount == 0; }
        }

        public int ObjectCount => _objects != null ? _objects.Count : 0;

        public int GenericCount => genericValueData != null ? genericValueData.Length : 0;

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

        public void SetGenericValue(IO.ByteBuffer buffer)
        {
            if (buffer != null)
            {
                if (genericValueData == null)
                {
                    genericValueData = new byte[buffer.readableBytes];
                    buffer.ReadBytes(genericValueData, 0, buffer.readableBytes);
                }
                else
                {
                    if (genericValueData.Length != buffer.readableBytes)
                    {
                        Array.Resize(ref genericValueData, buffer.readableBytes);
                    }
                    buffer.ReadBytes(genericValueData, 0, buffer.readableBytes);
                }
            }
        }

        public void Clear()
        {
            _objects?.Clear();
        }
    }
}
#endif
