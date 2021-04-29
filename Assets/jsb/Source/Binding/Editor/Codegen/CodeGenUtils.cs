using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace QuickJS.Binding
{
    public static class CodeGenUtils
    {
        public static void RemoveAt<T>(ref T[] array, int index)
        {
#if JSB_UNITYLESS
            for (var i = index; i < array.Length - 1; i++)
            {
                array[i] = array[i + 1];
            }
            Array.Resize(ref array, array.Length - 1);
#else
            UnityEditor.ArrayUtility.RemoveAt(ref array, index);
#endif
        }

        public static string ToLiteral(bool v)
        {
            return v ? "true" : "false";
        }

        public static string Normalize(string name)
        {
            var gArgIndex = name.IndexOf("<");
            return gArgIndex < 0 ? name : name.Substring(0, gArgIndex);
        }

        public static string[] NormalizeEx(string[] values, string additional)
        {
            var list = new List<string>(values.Length + 1);
            list.AddRange(values);
            list.Add(additional);
            return Normalize(list.ToArray());
        }

        public static string[] Normalize(params string[] values)
        {
            return (from value in values where !string.IsNullOrEmpty(value) select value).ToArray();
        }

        public static string Concat(string sp, params string[] values)
        {
            return string.Join(sp, Normalize(values));
        }

        public static string ConcatAsLiteral(string sp, params string[] values)
        {
            return string.Join(sp, from value in Normalize(values) select $"\"{value}\"");
        }
    }
}