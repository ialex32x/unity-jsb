using System;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    using Native;
    using UnityEngine;

    [Serializable]
    public class JSBehaviourProperties<T>
    {
        [Serializable]
        public class KeyValuePair
        {
            public string key;
            public T value;
        }

        public List<KeyValuePair> values;
    }
}
