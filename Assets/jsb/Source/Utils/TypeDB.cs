using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using QuickJS.Native;
using System.Reflection;

namespace QuickJS.Utils
{
    using UnityEngine;

    public class TypeDB
    {
        private Dictionary<Type, MethodInfo> _delegates = new Dictionary<Type, MethodInfo>(); // 委托对应的 duktape 绑定函数
        private Dictionary<Type, int> _typeIndex = new Dictionary<Type, int>();
        private List<Type> _types = new List<Type>(); // 可用 索引 反查 Type

        public int Count
        {
            get { return _types.Count; }
        }

        public Type GetType(int index)
        {
            return _types[index];
        }

        public void AddDelegate(Type type, MethodInfo method)
        {
            _delegates[type] = method;
        }

        public Delegate CreateDelegate(Type type, ScriptDelegate fn)
        {
            MethodInfo method;
            if (_delegates.TryGetValue(type, out method))
            {
                var target = Delegate.CreateDelegate(type, fn, method, true);
                fn.target = target;
                return target;
            }
            return null;
        }

        public int AddType(Type type)
        {
            var index = _types.Count;
            _types.Add(type);
            _typeIndex[type] = index;
            return index;
        }

        public int IndexOf(Type type)
        {
            int index;
            if (_typeIndex.TryGetValue(type, out index))
            {
                return index;
            }

            return -1;
        }

        public static Type GetType(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            var type = Assembly.GetExecutingAssembly().GetType(name);
            return type;
        }
    }
}