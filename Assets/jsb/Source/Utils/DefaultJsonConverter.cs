using System;

namespace QuickJS.Utils
{
    public class DefaultJsonConverter : IJsonConverter
    {
        public object Deserialize(string json, Type type)
        {
            return FromJson(json, type);
        }

        public static object FromJson(string json, Type type)
        {
#if JSB_UNITYLESS && !JSB_UNITY_UTILITY
            //TODO: dotnet core: System.Text.Json.JsonSerializer.Deserialize
            throw new NotImplementedException();
#else
            return UnityEngine.JsonUtility.FromJson(json, type);
#endif
        }

        public static string ToJson(object obj, bool prettyPrint)
        {
#if JSB_UNITYLESS && !JSB_UNITY_UTILITY
            throw new NotImplementedException();
#else
            return UnityEngine.JsonUtility.ToJson(obj, true);
#endif
        }

    }
}
