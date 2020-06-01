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
    
    public class TypeRegister
    {
        private ScriptContext _context;

        private TypeDB _db;
        private Dictionary<Type, JSValue> _prototypes = new Dictionary<Type, JSValue>();

        public ScriptContext GetContext()
        {
            return _context;
        }

        public TypeRegister(ScriptContext context)
        {
            _db = new TypeDB();
            _context = context;
        }

        public TypeDB GetTypeDB()
        {
            return _db;
        }

        // 无命名空间, 直接外围对象作为容器 (通常是global)
        public JSValue GetNamespace() // [parent]
        {
            return _context.GetGlobalObject();
        }

        private JSValue _AutoProperty(string name)
        {
            var this_obj = _context.GetGlobalObject();
            var ns = JSApi.JSB_NewPropertyObjectStr(_context, this_obj, name, JSPropFlags.JS_PROP_C_W_E);
            JSApi.JS_FreeValue(_context, this_obj);
            return ns;
        }

        private JSValue _AutoProperty(JSValue this_obj, string name)
        {
            var ns = JSApi.JSB_NewPropertyObjectStr(_context, this_obj, name, JSPropFlags.JS_PROP_C_W_E);
            JSApi.JS_FreeValue(_context, this_obj);
            return ns;
        }

        public JSValue GetNamespace(string el) // [parent]
        {
            return _AutoProperty(el);
        }

        public JSValue GetNamespace(string el1, string el2) // [parent]
        {
            return _AutoProperty(_AutoProperty(el1), el1);
        }

        public JSValue GetNamespace(string el1, string el2, string el3) // [parent]
        {
            return _AutoProperty(_AutoProperty(_AutoProperty(el1), el1), el3);
        }

        // return [parent, el]
        public JSValue GetNamespace(params string[] els) // [parent]
        {
            var ns = _context.GetGlobalObject();
            for (int i = 0, size = els.Length; i < size; i++)
            {
                var el = els[i];
                ns = _AutoProperty(ns, el);
            }

            return ns;
        }

        // return type id
        public int Add(Type type, JSValue jsValue)
        {
            JSApi.JS_DupValue(_context, jsValue);
            _prototypes.Add(type, jsValue);
            return _db.AddType(type);
        }

        public void Cleanup()
        {
            foreach (var kv in _prototypes)
            {
                var jsValue = kv.Value;
                JSApi.JS_FreeValue(_context, jsValue);
            }
            _prototypes.Clear();
        }
        
        // 将 type 的 prototype 压栈 （未导出则向父类追溯）
        // 没有对应的基类 prototype 时, 不压栈
        public JSValue GetChainedPrototypeOf(Type baseType)
        {
            if (baseType == null)
            {
                return JSApi.JS_UNDEFINED;
            }
            if (baseType == typeof(Enum))
            {
                return JSApi.JS_UNDEFINED;
            }
            JSValue fn;
            if (_prototypes.TryGetValue(baseType, out fn))
            {
                return fn;
            }
            return GetChainedPrototypeOf(baseType.BaseType);
        }

        public TypeDB Finish()
        {
            foreach (var kv in _prototypes)
            {
                var type = kv.Key;
                var baseType = type.BaseType;
                var parent = GetChainedPrototypeOf(baseType);
                if (!JSApi.JS_IsUndefined(parent))
                {
                    var fn = kv.Value;
                    JSApi.JS_SetPrototype(_context, fn, parent);
                }
                else
                {
                    Debug.LogWarning($"fail to push prototype, for {type}: {baseType}");
                }
            }

            Cleanup();
            return GetTypeDB();
        }
    }
}