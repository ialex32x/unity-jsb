using System;

namespace QuickJS.Unity
{
    public class DefaultJsonConverter : Utils.JsonConverter
    {
        public object Deserialize(string json, Type type)
        {
            return UnityEngine.JsonUtility.FromJson(json, type);
        }
    }
}
