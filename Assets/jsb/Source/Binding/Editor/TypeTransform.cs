using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;

namespace QuickJS.Binding
{
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
        private bool _enableOperatorOverloading = true;
        private bool _disposable;

        public bool crossbind => _csConstructorOverride != null;

        public bool disposable => _disposable;

        public TypeBindingFlags bindingFlags = TypeBindingFlags.Default;

        // 此类型依赖特定的预编译指令
        public HashSet<string> requiredDefines = new HashSet<string>();

        // 扩展方法
        public readonly List<MethodInfo> extensionMethods = new List<MethodInfo>();

        // 附加的静态方法
        public readonly List<MethodInfo> staticMethods = new List<MethodInfo>();

        // 按名字屏蔽导出
        private HashSet<string> _memberBlacklist = new HashSet<string>();

        // 强制不导出的方法
        private HashSet<MethodBase> _blockedMethods = new HashSet<MethodBase>();

        // 针对特定方法的 ts 声明优化
        private Dictionary<MethodBase, string> _tsMethodDeclarations = new Dictionary<MethodBase, string>();
        private Dictionary<MethodBase, Func<string, CodeGenerator, object, bool>> _csMethodWriter = new Dictionary<MethodBase, Func<string, CodeGenerator, object, bool>>();
        private Dictionary<string, Native.JSCFunction> _csMethodOverride = new Dictionary<string, Native.JSCFunction>();
        private Native.JSCFunctionMagic _csConstructorOverride = null;

        // d.ts 中额外输出附加方法声明 (例如 Vector3, js中需要通过方法调用进行 +-*/== 等运算)
        private List<string> _tsAdditionalMethodDeclarations = new List<string>();

        private Dictionary<Type, Delegate> _filters = new Dictionary<Type, Delegate>();
        // private Func<ConstructorInfo, bool> _filterConstructorInfo;
        // private Func<PropertyInfo, bool> _filterPropertyInfo;
        // private Func<FieldInfo, bool> _filterFieldInfo;
        // private Func<EventInfo, bool> _filterEventInfo;
        // private Func<MethodInfo, bool> _filterMethodInfo;

        private Dictionary<MemberInfo, string> _memberNameRules = new Dictionary<MemberInfo, string>();
        
        private Dictionary<MemberInfo, string> _memberNameAlias = new Dictionary<MemberInfo, string>();

        /// <summary>
        /// 是否需要 UNITY_EDITOR 条件
        /// </summary>
        public bool isEditorRuntime { get { return requiredDefines.Contains("UNITY_EDITOR"); } }

        public bool enableOperatorOverloading => _enableOperatorOverloading;

        public Type type => _type;

        public Native.JSCFunctionMagic csConstructorOverride => _csConstructorOverride;

        public TypeTransform(Type type)
        {
            _type = type;
            if (_type.IsGenericTypeDefinition)
            {
                bindingFlags = TypeBindingFlags.Default & ~TypeBindingFlags.BindingCode;
            }
        }

        public TypeTransform EnableOperatorOverloading(bool value)
        {
            _enableOperatorOverloading = value;
            return this;
        }

        /// <summary>
        /// 标记此类型完全由 JS 托管 (JS对象释放时, CS对象即释放).
        /// 该设置只针对由 JS 构造产生的此类型对象实例.
        /// </summary>
        public TypeTransform SetDisposable()
        {
            _disposable = true;
            return this;
        }

        public TypeTransform EditorRuntime()
        {
            return AddRequiredDefines("UNITY_EDITOR");
        }

        /// <summary>
        /// 
        /// </summary>
        public TypeTransform AddRequiredDefines(params string[] defines)
        {
            foreach (var def in defines)
            {
                requiredDefines.Add(def);
            }
            return this;
        }

        /// <summary>
        /// 标记此类型不限制于目标平台编译
        /// </summary>
        public TypeTransform SystemRuntime()
        {
            bindingFlags &= ~TypeBindingFlags.BuildTargetPlatformOnly;
            return this;
        }

        public TypeTransform OnFilter<T>(Func<T, bool> callback)
        {
            _filters[typeof(T)] = callback;
            return this;
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

        public string GetNameAlias(MemberInfo info)
        {
            string alias;
            return _memberNameAlias.TryGetValue(info, out alias) ? alias : info.Name;
        }

        public string GetNameRule(MemberInfo info)
        {
            string rule;
            return _memberNameRules.TryGetValue(info, out rule) ? rule : null;
        }

        public void SetNameRule(Func<MemberInfo, string> callback)
        {
            foreach (var m in _type.GetMembers())
            {
                var r = callback(m);
                if (r != null)
                {
                    _memberNameRules[m] = r;
                }
            }
        }

        #region Extension Method Management
        public TypeTransform AddExtensionMethod<T>(Action<T> method, string tsDecl = null)
        {
            return AddExtensionMethod(method.Method, tsDecl);
        }

        public TypeTransform AddExtensionMethod<T1, T2>(Action<T1, T2> method, string tsDecl = null)
        {
            return AddExtensionMethod(method.Method, tsDecl);
        }

        public TypeTransform AddExtensionMethod<T1, T2, T3>(Action<T1, T2, T3> method, string tsDecl = null)
        {
            return AddExtensionMethod(method.Method, tsDecl);
        }

        public TypeTransform AddExtensionMethod<TResult>(Func<TResult> method, string tsDecl = null)
        {
            return AddExtensionMethod(method.Method, tsDecl);
        }

        public TypeTransform AddExtensionMethod<T1, TResult>(Func<T1, TResult> method, string tsDecl = null)
        {
            return AddExtensionMethod(method.Method, tsDecl);
        }

        public TypeTransform AddExtensionMethod<T1, T2, TResult>(Func<T1, T2, TResult> method, string tsDecl = null)
        {
            return AddExtensionMethod(method.Method, tsDecl);
        }

        public TypeTransform AddExtensionMethod(MethodInfo method, string tsDecl = null)
        {
            if (!extensionMethods.Contains(method) && !Filter(method))
            {
                extensionMethods.Add(method);

                AddTSMethodDeclaration(method, tsDecl);
            }

            return this;
        }
        #endregion

        #region Extended Static Method Management
        public TypeTransform AddStaticMethod<T>(Action<T> method, string tsDecl = null)
        {
            return AddStaticMethod(method.Method, tsDecl);
        }

        public TypeTransform AddStaticMethod<T1, T2>(Action<T1, T2> method, string tsDecl = null)
        {
            return AddStaticMethod(method.Method, tsDecl);
        }

        public TypeTransform AddStaticMethod<T1, T2, T3>(Action<T1, T2, T3> method, string tsDecl = null)
        {
            return AddStaticMethod(method.Method, tsDecl);
        }

        public TypeTransform AddStaticMethod<TResult>(Func<TResult> method, string tsDecl = null)
        {
            return AddStaticMethod(method.Method, tsDecl);
        }

        public TypeTransform AddStaticMethod<T1, TResult>(Func<T1, TResult> method, string tsDecl = null)
        {
            return AddStaticMethod(method.Method, tsDecl);
        }

        public TypeTransform AddStaticMethod<T1, T2, TResult>(Func<T1, T2, TResult> method, string tsDecl = null)
        {
            return AddStaticMethod(method.Method, tsDecl);
        }

        public TypeTransform AddStaticMethod(Native.JSCFunction method, string tsDecl = null)
        {
            return AddStaticMethod(method.Method, tsDecl);
        }

        public TypeTransform AddStaticMethod(MethodInfo method, string tsDecl = null)
        {
            if (!staticMethods.Contains(method))
            {
                staticMethods.Add(method);

                AddTSMethodDeclaration(method, tsDecl);
            }

            return this;
        }
        #endregion

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

        public bool IsBlocked(int token)
        {
            return _blockedMethods.Any(i => i.MetadataToken == token);
        }

        /// <summary>
        /// 屏蔽所有构造函数
        /// </summary>
        public TypeTransform SetAllConstructorsBlocked()
        {
            foreach (var ctor in _type.GetConstructors())
            {
                _blockedMethods.Add(ctor);
            }

            return this;
        }

        /// <summary>
        /// 屏蔽指定签名的构造方法
        /// </summary>
        public TypeTransform SetConstructorBlocked(params Type[] parameters)
        {
            var method = _type.GetConstructor(parameters);
            if (method != null)
            {
                _blockedMethods.Add(method);
            }
            return this;
        }

        /// <summary>
        /// 屏蔽指定名字与签名的方法
        /// </summary>
        public TypeTransform SetMethodBlocked(string name, params Type[] parameters)
        {
            var method = _type.GetMethod(name, parameters);
            if (method != null)
            {
                _blockedMethods.Add(method);
            }
            return this;
        }

        /// <summary>
        /// specify the method name in JS instead of it's C# name
        /// </summary>
        public TypeTransform SetMethodJSName(string jsName, string name, params Type[] parameters)
        {
            var method = _type.GetMethod(name, parameters);
            if (method != null)
            {
                _memberNameAlias[method] = jsName;
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

        public TypeTransform AddTSMethodDeclaration(MethodBase method, string spec)
        {
            if (method != null && spec != null)
            {
                _tsMethodDeclarations[method] = spec;
            }
            return this;
        }

        public bool GetTSMethodDeclaration(MethodBase method, out string code)
        {
            return _tsMethodDeclarations.TryGetValue(method, out code);
        }

        [Obsolete("此方法不利于统一反射模式绑定逻辑，将被移除")]
        public TypeTransform WriteCSConstructorBinding(Func<string, CodeGenerator, object, bool> writer, params Type[] parameters)
        {
            var ctor = _type.GetConstructor(parameters);
            if (ctor != null)
            {
                _csMethodWriter[ctor] = writer;
            }

            return this;
        }

        public TypeTransform WriteCrossBindingConstructor(params Type[] parameters)
        {
            _csConstructorOverride = CommonFix.CrossBindConstructor;
            return this;
        }

        /// <summary>
        /// 替换静态绑定代码中的部分执行逻辑 (考虑废弃, 更难维护静态绑定与反射绑定的一致性)
        /// </summary>
        [Obsolete("此方法不利于统一反射模式绑定逻辑，将被移除")]
        public TypeTransform WriteCSMethodBinding(Func<string, CodeGenerator, object, bool> writer, string methodName, params Type[] parameters)
        {
            var method = _type.GetMethod(methodName, parameters);
            if (method != null)
            {
                _csMethodWriter[method] = writer;
            }

            return this;
        }

        public TypeTransform WriteCSMethodOverrideBinding(string methodName, Native.JSCFunction writer)
        {
            _csMethodOverride[methodName] = writer;
            return this;
        }

        public Native.JSCFunction GetCSMethodOverrideBinding(string methodName)
        {
            return _csMethodOverride.TryGetValue(methodName, out var func) ? func : null;
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
    }
}
