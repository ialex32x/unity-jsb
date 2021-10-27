using System;

namespace QuickJS.Utils
{
    public class DefaultJsonConverter : IJsonConverter
    {
        public object Deserialize(string json, Type type)
        {
#if JSB_UNITYLESS
#if JSB_COMPATIBLE
            throw new NotImplementedException();
#else
            return System.Text.Json.JsonSerializer.Deserialize(json, type);
#endif
#else
            return UnityEngine.JsonUtility.FromJson(json, type);
#endif
        }

        public string Serialize(object obj, bool prettyPrint)
        {
#if JSB_UNITYLESS
#if JSB_COMPATIBLE
            throw new NotImplementedException();
#else
            return System.Text.Json.JsonSerializer.Serialize(obj);
#endif
#else
            return UnityEngine.JsonUtility.ToJson(obj, true);
#endif
        }
    }
}
