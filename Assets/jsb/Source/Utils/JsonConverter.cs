using System;

namespace QuickJS.Utils
{
    public interface JsonConverter
    {
        object Deserialize(string json, Type type);
    }
}
