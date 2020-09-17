using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    public class BindingPoints
    {
        public const string METHOD_BINDING_FULL = "METHOD_BINDING_FULL";
        public const string METHOD_BINDING_BEFORE_INVOKE = "METHOD_BINDING_BEFORE_INVOKE";
    }

    public class TypeTransform
    {
        private Type _type;
        private JSHotfixAttribute _hotfix;
        private string _typeNaming;

        public TypeBindingFlags bindingFlags = TypeBindingFlags.Default;

        // 扩展方法
        public readonly List<MethodInfo> extensionMethods = new List<MethodInfo>();

        // 按名字屏蔽导出
        private HashSet<string> _memberBlacklist = new HashSet<string>();

        // 强制不导出的方法
        private HashSet<MethodBase> _blockedMethods = new HashSet<MethodBase>();

        // 方法返回值 push 方法覆盖
        private Dictionary<MethodBase, string> _mehotdReturnPusher = new Dictionary<MethodBase, string>();

        // 针对特定方法的 ts 声明优化
        private Dictionary<MethodBase, string> _tsMethodDeclarations = new Dictionary<MethodBase, string>();
        private Dictionary<MethodBase, string> _tsMethodRenames = new Dictionary<MethodBase, string>();
        private Dictionary<MethodBase, Func<string, CodeGenerator, object, bool>> _csMethodWriter = new Dictionary<MethodBase, Func<string, CodeGenerator, object, bool>>();

        // d.ts 中额外输出附加方法声明 (例如 Vector3, js中需要通过方法调用进行 +-*/== 等运算)
        private List<string> _tsAdditionalMethodDeclarations = new List<string>();

        private Dictionary<string, string> _redirectedMethods = new Dictionary<string, string>();

        private Dictionary<Type, Delegate> _filters = new Dictionary<Type, Delegate>();
        private Func<ConstructorInfo, bool> _filterConstructorInfo;
        private Func<PropertyInfo, bool> _filterPropertyInfo;
        private Func<FieldInfo, bool> _filterFieldInfo;
        private Func<EventInfo, bool> _filterEventInfo;
        private Func<MethodInfo, bool> _filterMethodInfo;

        public bool isEditorRuntime { get { return (bindingFlags & TypeBindingFlags.UnityEditorRuntime) != 0; } }

        public TypeTransform(Type type)
        {
            _type = type;
        }

        public TypeTransform EditorRuntime()
        {
            bindingFlags |= TypeBindingFlags.UnityEditorRuntime;
            return this;
        }

        public TypeTransform SystemRuntime()
        {
            bindingFlags &= ~TypeBindingFlags.UnityRuntime;
            return this;
        }

        public TypeTransform SetRuntime(TypeBindingFlags bf)
        {
            bindingFlags |= bf;
            return this;
        }

        public TypeTransform UnsetRuntime(TypeBindingFlags bf)
        {
            bindingFlags &= ~bf;
            return this;
        }

        public void OnFilter<T>(Func<T, bool> callback)
        {
            _filters[typeof(T)] = callback;
        }

        public bool Filter<T>(T info)
        {
            Delegate d;
            if (!_filters.TryGetValue(typeof(T), out d))
            {
                return false;
            }
            var t = (Func<T, bool>)d;
            return t(info);
        }

        // public void OnFilter(Func<ConstructorInfo, bool> callback)
        // {
        //     _filterConstructorInfo = callback;
        // }

        // public void OnFilter(Func<PropertyInfo, bool> callback)
        // {
        //     _filterPropertyInfo = callback;
        // }

        // public void OnFilter(Func<FieldInfo, bool> callback)
        // {
        //     _filterFieldInfo = callback;
        // }

        // public void OnFilter(Func<EventInfo, bool> callback)
        // {
        //     _filterEventInfo = callback;
        // }

        // public void OnFilter(Func<MethodInfo, bool> callback)
        // {
        //     _filterMethodInfo = callback;
        // }

        // public bool Filter(ConstructorInfo info)
        // {
        //     return _filterConstructorInfo(info);
        // }

        // public bool Filter(PropertyInfo info)
        // {
        //     return _filterPropertyInfo(info);
        // }

        // public bool Filter(FieldInfo info)
        // {
        //     return _filterFieldInfo(info);
        // }

        // public bool Filter(EventInfo info)
        // {
        //     return _filterEventInfo(info);
        // }

        // public bool Filter(MethodInfo info)
        // {
        //     return _filterMethodInfo(info);
        // }

        public void AddExtensionMethod(MethodInfo method)
        {
            if (!extensionMethods.Contains(method))
            {
                extensionMethods.Add(method);
            }
        }

        public JSHotfixAttribute GetHotfix()
        {
            return _hotfix;
        }

        public void SetHotfix(JSHotfixAttribute attr)
        {
            _hotfix = attr;
        }

        public void ForEachAdditionalTSMethodDeclaration(Action<string> fn)
        {
            foreach (var decl in _tsAdditionalMethodDeclarations)
            {
                fn(decl);
            }
        }

        public string GetTypeNaming()
        {
            return _typeNaming;
        }

        public TypeTransform Rename(string name)
        {
            _typeNaming = name;
            return this;
        }

        public TypeTransform AddTSMethodDeclaration(string spec)
        {
            _tsAdditionalMethodDeclarations.Add(spec);
            return this;
        }

        public TypeTransform AddTSMethodDeclaration(params string[] specs)
        {
            _tsAdditionalMethodDeclarations.AddRange(specs);
            return this;
        }

        public bool IsMemberBlocked(string memeberName)
        {
            return _memberBlacklist.Contains(memeberName);
        }

        public TypeTransform SetMemberBlocked(string memberName)
        {
            _memberBlacklist.Add(memberName);
            return this;
        }

        // 指定的方法是否被屏蔽
        public bool IsBlocked(MethodBase method)
        {
            return _blockedMethods.Contains(method);
        }

        // 屏蔽指定签名的构造方法
        public TypeTransform SetConstructorBlocked(params Type[] parameters)
        {
            var method = _type.GetConstructor(parameters);
            if (method != null)
            {
                _blockedMethods.Add(method);
            }
            return this;
        }

        // 屏蔽指定名字与签名的方法
        public TypeTransform SetMethodBlocked(string name, params Type[] parameters)
        {
            var method = _type.GetMethod(name, parameters);
            if (method != null)
            {
                _blockedMethods.Add(method);
            }
            return this;
        }

        public TypeTransform SetMethodReturnPusher(string pusher, string name, params Type[] parameters)
        {
            var method = _type.GetMethod(name, parameters);
            if (method != null)
            {
                _mehotdReturnPusher.Add(method, pusher);
            }
            return this;
        }

        public string GetMethodReturnPusher(MethodBase methodBase)
        {
            string pusher;
            return _mehotdReturnPusher.TryGetValue(methodBase, out pusher) ? pusher : null;
        }

        // TS: 为指定类型的匹配方法添加声明映射 (仅用于优化代码提示体验)
        public TypeTransform AddTSMethodDeclaration(string spec, string name, params Type[] parameters)
        {
            var method = _type.GetMethod(name, parameters);
            if (method != null)
            {
                _tsMethodDeclarations[method] = spec;
            }
            return this;
        }

        public bool GetTSMethodDeclaration(MethodBase method, out string code)
        {
            return _tsMethodDeclarations.TryGetValue(method, out code);
        }

        public TypeTransform RenameTSMethod(string newName, string oldName, params Type[] parameters)
        {
            var method = _type.GetMethod(oldName, parameters);
            if (method != null)
            {
                _tsMethodRenames[method] = newName;
            }
            return this;
        }

        public TypeTransform WriteCSConstructorBinding(Func<string, CodeGenerator, object, bool> writer, params Type[] parameters)
        {
            var ctor = _type.GetConstructor(parameters);
            if (ctor != null)
            {
                _csMethodWriter[ctor] = writer;
            }

            return this;
        }

        public TypeTransform WriteCSMethodBinding(Func<string, CodeGenerator, object, bool> writer, string methodName, params Type[] parameters)
        {
            var method = _type.GetMethod(methodName, parameters);
            if (method != null)
            {
                _csMethodWriter[method] = writer;
            }

            return this;
        }

        public bool OnBinding(string bindPoint, MethodBase method, CodeGenerator cg, object info = null)
        {
            Func<string, CodeGenerator, object, bool> act;
            if (_csMethodWriter.TryGetValue(method, out act))
            {
                return act(bindPoint, cg, info);
            }

            return false;
        }

        public bool GetTSMethodRename(MethodBase method, out string name)
        {
            return _tsMethodRenames.TryGetValue(method, out name);
        }

        public TypeTransform AddRedirectMethod(string from, string to)
        {
            _redirectedMethods[from] = to;
            return this;
        }

        public bool TryRedirectMethod(string name, out string to)
        {
            return _redirectedMethods.TryGetValue(name, out to);
        }

        public bool IsRedirectedMethod(string name)
        {
            return _redirectedMethods.ContainsKey(name);
        }
    }
}
