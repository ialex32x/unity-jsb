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
        private ScriptRuntime _runtime;
        private Dictionary<Type, MethodInfo> _delegates = new Dictionary<Type, MethodInfo>(); // 委托对应的 duktape 绑定函数
        private Dictionary<Type, int> _typeIndex = new Dictionary<Type, int>();
        private List<Type> _types = new List<Type>(); // 可用 索引 反查 Type
        private Dictionary<Type, JSValue> _prototypes = new Dictionary<Type, JSValue>();

        public int Count
        {
            get { return _types.Count; }
        }

        public TypeDB(ScriptRuntime runtime)
        {
            _runtime = runtime;
        }

        public Type GetType(int index)
        {
            return index >= 0 && index < _types.Count ? _types[index] : null;
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

        public int AddType(Type type, JSValue proto)
        {
            var index = _types.Count;
            _types.Add(type);
            _typeIndex[type] = index;
            _prototypes.Add(type, proto);
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

        // 将 type 的 prototype 压栈 （未导出则向父类追溯）
        // without reference-count added
        public JSValue FindPrototypeOf(Type cType, out int type_id)
        {
            if (cType == null)
            {
                type_id = -1;
                return JSApi.JS_UNDEFINED;
            }

            if (cType == typeof(Enum))
            {
                type_id = -1;
                return JSApi.JS_UNDEFINED;
            }

            JSValue proto;
            if (_prototypes.TryGetValue(cType, out proto))
            {
                type_id = IndexOf(cType);
                return proto;
            }

            return FindPrototypeOf(cType.BaseType, out type_id);
        }

        public JSValue FindPrototypeOf(Type cType)
        {
            if (cType == null)
            {
                return JSApi.JS_UNDEFINED;
            }

            if (cType == typeof(Enum))
            {
                return JSApi.JS_UNDEFINED;
            }

            JSValue proto;
            if (_prototypes.TryGetValue(cType, out proto))
            {
                return proto;
            }

            return FindPrototypeOf(cType.BaseType);
        }

        public JSValue FindPrototypeOf(Type cType, out Type pType)
        {
            if (cType == null)
            {
                pType = null;
                return JSApi.JS_UNDEFINED;
            }

            if (cType == typeof(Enum))
            {
                pType = null;
                return JSApi.JS_UNDEFINED;
            }

            JSValue proto;
            if (_prototypes.TryGetValue(cType, out proto))
            {
                pType = cType;
                return proto;
            }

            return FindPrototypeOf(cType.BaseType, out pType);
        }

        public TypeDB Finish()
        {
            var ctx = _runtime.GetMainContext();
            foreach (var kv in _prototypes)
            {
                var type = kv.Key;
                var baseType = type.BaseType;
                var parent = FindPrototypeOf(baseType);
                if (!JSApi.JS_IsUndefined(parent))
                {
                    var fn = kv.Value;
                    JSApi.JS_SetPrototype(ctx, fn, parent);
                }
            }

            return this;
        }

        public void Destroy()
        {
            var ctx = _runtime.GetMainContext();
            foreach (var kv in _prototypes)
            {
                var jsValue = kv.Value;
                JSApi.JS_FreeValue(ctx, jsValue);
            }

            _prototypes.Clear();
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