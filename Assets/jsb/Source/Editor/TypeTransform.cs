using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    public class TypeTransform
    {
        private Type _type;
        private string _typeNaming;

        // 按名字屏蔽导出
        private HashSet<string> _memberBlacklist = new HashSet<string>();

        // 强制不导出的方法
        private HashSet<MethodBase> _blockedMethods = new HashSet<MethodBase>();

        // 针对特定方法的 ts 声明优化
        private Dictionary<MethodBase, string> _tsMethodDeclarations = new Dictionary<MethodBase, string>();
        private Dictionary<MethodBase, string> _tsMethodRenames = new Dictionary<MethodBase, string>();

        // d.ts 中额外输出附加方法声明 (例如 Vector3, js中需要通过方法调用进行 +-*/== 等运算)
        private List<string> _tsAdditionalMethodDeclarations = new List<string>();

        private Dictionary<string, string> _redirectedMethods = new Dictionary<string, string>();

        public TypeTransform(Type type)
        {
            _type = type;
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
