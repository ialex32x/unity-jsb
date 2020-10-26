using System;

namespace QuickJS.Utils
{
    public interface IJsonConverter
    {
        object Deserialize(string json, Type type);
    }
}
