using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using QuickJS.Native;
using System.Reflection;

namespace QuickJS.Utils
{
    public class TypeRegistry
    {
        private Dictionary<Type, MethodInfo> _delegates = new Dictionary<Type, MethodInfo>(); // 委托对应的 duktape 绑定函数
        private Dictionary<Type, int> _exportedTypeIndexer = new Dictionary<Type, int>();
        private List<Type> _exportedTypes = new List<Type>(); // 可用 索引 反查 Type
        
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

        public int AddExportedType(Type type)
        {
            var index = _exportedTypes.Count;
            _exportedTypes.Add(type);
            _exportedTypeIndexer[type] = index;
            return index;
        }

        public int GetExportedTypeCount()
        {
            return _exportedTypes.Count;
        }

        public Type GetExportedType(int index)
        {
            return index >= 0 && index < _exportedTypes.Count ? _exportedTypes[index] : null;
        }

        public bool TryGetExportedTypeIndex(Type type, out int index)
        {
            return _exportedTypeIndexer.TryGetValue(type, out index);
        }

    }
}