using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using QuickJS.Native;
using QuickJS.Binding;
using System.Reflection;

namespace QuickJS.Utils
{
    using UnityEngine;

    public class TypeDB
    {
        private ScriptRuntime _runtime;
        private ScriptContext _context;
        private Dictionary<Type, MethodInfo> _delegates = new Dictionary<Type, MethodInfo>(); // 委托对应的 js 绑定函数
        private Dictionary<Type, int> _typeIndex = new Dictionary<Type, int>();
        private List<Type> _types = new List<Type>(); // 可用 索引 反查 Type
        private Dictionary<Type, JSValue> _prototypes = new Dictionary<Type, JSValue>();
        private List<IDynamicMethod> _dynamicMethods = new List<IDynamicMethod>();
        private List<IDynamicField> _dynamicFields = new List<IDynamicField>();

        public int Count
        {
            get { return _types.Count; }
        }

        public TypeDB(ScriptRuntime runtime, ScriptContext context)
        {
            _runtime = runtime;
            _context = context;
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
            else
            {
                throw new ArgumentNullException("unknown delegate type: " + type);
            }
            // return null;
        }

        // 注册新类型, 会增加 proto 的引用计数
        public int AddType(Type type, JSValue proto)
        {
            JSValue old_proto;
            if (_prototypes.TryGetValue(type, out old_proto))
            {
                JSApi.JS_FreeValue(_context, old_proto);
                _prototypes[type] = JSApi.JS_DupValue(_context, proto);
                return _typeIndex[type];
            }

            _prototypes[type] = JSApi.JS_DupValue(_context, proto);
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

        public JSValue GetPropertyOf(Type type)
        {
            JSValue proto;
            if (_prototypes.TryGetValue(type, out proto))
            {
                return proto;
            }
            return JSApi.JS_UNDEFINED;
        }

        public void Destroy()
        {
            var ctx = (JSContext)_context;
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

        public JSValue NewDynamicMethod(JSAtom name, IDynamicMethod method)
        {
            if (method == null)
            {
                var funValue = JSApi.JSB_NewCFunctionMagic(_context, JSApi.class_private_ctor, name, 0, JSCFunctionEnum.JS_CFUNC_generic_magic, 0);
                return funValue;
            }
            else
            {
                var magic = _dynamicMethods.Count;
                var funValue = JSApi.JSB_NewCFunctionMagic(_context, JSApi._DynamicMethodInvoke, name, 0, JSCFunctionEnum.JS_CFUNC_generic_magic, magic);
                _dynamicMethods.Add(method);
                return funValue;
            }
        }

        public void NewDynamicFieldGetter(JSAtom name, IDynamicField field, out JSValue getter, out JSValue setter)
        {
            var magic = _dynamicFields.Count;
            getter = JSApi.JSB_NewCFunction(_context, JSApi._DynamicFieldGetter, name, magic);
            setter = JSApi.JSB_NewCFunction(_context, JSApi._DynamicFieldSetter, name, magic);
            _dynamicFields.Add(field);
        }

        public IDynamicMethod GetDynamicMethod(int index)
        {
            return index >= 0 && index < _dynamicMethods.Count ? _dynamicMethods[index] : null;
        }

        public IDynamicField GetDynamicField(int index)
        {
            return index >= 0 && index < _dynamicFields.Count ? _dynamicFields[index] : null;
        }
    }
}