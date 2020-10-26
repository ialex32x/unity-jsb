using System;

namespace QuickJS.Unity
{
    public class DefaultJsonConverter : Utils.IJsonConverter
    {
        public object Deserialize(string json, Type type)
        {
#if JSB_UNITYLESS
            //TODO: dotnet core: System.Text.Json.JsonSerializer.Deserialize
#else
            return UnityEngine.JsonUtility.FromJson(json, type);
#endif
        }
    }
}
