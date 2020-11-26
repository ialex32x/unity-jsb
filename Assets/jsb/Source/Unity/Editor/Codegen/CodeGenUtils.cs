using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public static class CodeGenUtils
    {
        public static bool IsSpecialParameterType(Type parameterType)
        {
            return parameterType == typeof(Native.JSContext) || parameterType == typeof(Native.JSRuntime) 
                || parameterType == typeof(ScriptContext) || parameterType == typeof(ScriptRuntime);
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

        public static string Concat(string sp, params string[] values)
        {
            return string.Join(sp, from value in values where !string.IsNullOrEmpty(value) select value);
        }

        public static string ConcatAsLiteral(string sp, params string[] values)
        {
            return string.Join(sp, from value in values where !string.IsNullOrEmpty(value) select $"\"{value}\"");
        }
    }
}