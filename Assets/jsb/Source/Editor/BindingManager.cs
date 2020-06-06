using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    public partial class BindingManager
    {
        public DateTime dateTime;
        public TextGenerator log;
        public Prefs prefs;

        private List<string> _implicitAssemblies = new List<string>(); // 默认导出所有类型
        private List<string> _explicitAssemblies = new List<string>(); // 仅导出指定需要导出的类型

        private HashSet<Type> blacklist;
        private HashSet<Type> whitelist;
        private List<string> typePrefixBlacklist;
        private Dictionary<Type, TypeBindingInfo> exportedTypes = new Dictionary<Type, TypeBindingInfo>();
        private Dictionary<Type, DelegateBindingInfo> exportedDelegates = new Dictionary<Type, DelegateBindingInfo>();
        private Dictionary<Type, Type> redirectDelegates = new Dictionary<Type, Type>();
        // 类型修改
        private Dictionary<Type, TypeTransform> typesTarnsform = new Dictionary<Type, TypeTransform>();
        private Dictionary<string, List<string>> _outputFiles = new Dictionary<string, List<string>>();
        private List<string> removedFiles = new List<string>();

        private Dictionary<Type, List<string>> _tsTypeNameMap = new Dictionary<Type, List<string>>();
        private Dictionary<Type, string> _csTypeNameMap = new Dictionary<Type, string>();
        private Dictionary<Type, string> _csTypePusherMap = new Dictionary<Type, string>();
        private Dictionary<string, string> _csTypeNameMapS = new Dictionary<string, string>();
        private static HashSet<string> _tsKeywords = new HashSet<string>();

        // 自定义的处理流程
        private List<IBindingProcess> _bindingProcess = new List<IBindingProcess>();

        static BindingManager()
        {
            AddTSKeywords(
                "function",
                "interface",
                "class",
                "let",
                "break",
                "as",
                "any",
                "switch",
                "case",
                "if",
                "throw",
                "else",
                "var",
                "number",
                "string",
                "get",
                "module",
                // "type",
                "instanceof",
                "typeof",
                "public",
                "private",
                "enum",
                "export",
                "finally",
                "for",
                "while",
                "void",
                "null",
                "super",
                "this",
                "new",
                "in",
                "await", 
                "async", 
                "extends",
                "static",
                "package",
                "implements",
                "interface",
                "continue",
                "yield",
                "const"
            );
        }

        public BindingManager(Prefs prefs)
        {
            this.prefs = prefs;
            this.dateTime = DateTime.Now;
            var tab = prefs.tab;
            var newline = prefs.newline;
            typePrefixBlacklist = prefs.typePrefixBlacklist;
            log = new TextGenerator(newline, tab);
            blacklist = new HashSet<Type>(new Type[]
            {
                typeof(AOT.MonoPInvokeCallbackAttribute),
            });
            whitelist = new HashSet<Type>(new Type[]
            {
            });

            TransformType(typeof(GameObject))
                // .AddRedirectMethod("AddComponent", "_AddComponent")
                // .AddRedirectMethod("GetComponent", "_GetComponent")
                // .AddRedirectMethod("GetComponentInChildren", "_GetComponentInChildren")
                // .AddRedirectMethod("GetComponentInParent", "_GetComponentInParent")
                // .AddRedirectMethod("GetComponents", "_GetComponents")
                // .AddRedirectMethod("GetComponentsInChildren", "_GetComponentsInChildren")
                // .AddRedirectMethod("GetComponentsInParent", "_GetComponentsInParent")
                .AddTSMethodDeclaration("AddComponent<T extends UnityEngine.Component>(type: { new(): T }): T",
                    "AddComponent", typeof(Type))
                .AddTSMethodDeclaration("GetComponent<T extends UnityEngine.Component>(type: { new(): T }): T",
                    "GetComponent", typeof(Type))
                .AddTSMethodDeclaration("GetComponentInChildren<T extends UnityEngine.Component>(type: { new(): T }, includeInactive: boolean): T",
                    "GetComponentInChildren", typeof(Type), typeof(bool))
                .AddTSMethodDeclaration("GetComponentInChildren<T extends UnityEngine.Component>(type: { new(): T }): T",
                    "GetComponentInChildren", typeof(Type))
                .AddTSMethodDeclaration("GetComponentInParent<T extends UnityEngine.Component>(type: { new(): T }): T",
                    "GetComponentInParent", typeof(Type))
                // .AddTSMethodDeclaration("GetComponents<T extends UnityEngine.Component>(type: { new(): T }, results: any): void", 
                //     "GetComponents", typeof(Type))
                .AddTSMethodDeclaration("GetComponents<T extends UnityEngine.Component>(type: { new(): T }): T[]",
                    "GetComponents", typeof(Type))
                .AddTSMethodDeclaration("GetComponentsInChildren<T extends UnityEngine.Component>(type: { new(): T }, includeInactive: boolean): T[]",
                    "GetComponentsInChildren", typeof(Type), typeof(bool))
                .AddTSMethodDeclaration("GetComponentsInChildren<T extends UnityEngine.Component>(type: { new(): T }): T[]",
                    "GetComponentsInChildren", typeof(Type))
                .AddTSMethodDeclaration("GetComponentsInParent<T extends UnityEngine.Component>(type: { new(): T }, includeInactive: boolean): T[]",
                    "GetComponentsInParent", typeof(Type), typeof(bool))
                .AddTSMethodDeclaration("GetComponentsInParent<T extends UnityEngine.Component>(type: { new(): T }): T[]",
                    "GetComponentsInParent", typeof(Type))
            ;

            TransformType(typeof(Component))
                .AddTSMethodDeclaration("GetComponent<T extends UnityEngine.Component>(type: { new(): T }): T",
                    "GetComponent", typeof(Type))
                .AddTSMethodDeclaration("GetComponentInChildren<T extends UnityEngine.Component>(type: { new(): T }, includeInactive: boolean): T",
                    "GetComponentInChildren", typeof(Type), typeof(bool))
                .AddTSMethodDeclaration("GetComponentInChildren<T extends UnityEngine.Component>(type: { new(): T }): T",
                    "GetComponentInChildren", typeof(Type))
                .AddTSMethodDeclaration("GetComponentInParent<T extends UnityEngine.Component>(type: { new(): T }): T",
                    "GetComponentInParent", typeof(Type))
                // .AddTSMethodDeclaration("GetComponents<T extends UnityEngine.Component>(type: { new(): T }, results: any): void", 
                //     "GetComponents", typeof(Type))
                .AddTSMethodDeclaration("GetComponents<T extends UnityEngine.Component>(type: { new(): T }): T[]",
                    "GetComponents", typeof(Type))
                .AddTSMethodDeclaration("GetComponentsInChildren<T extends UnityEngine.Component>(type: { new(): T }, includeInactive: boolean): T[]",
                    "GetComponentsInChildren", typeof(Type), typeof(bool))
                .AddTSMethodDeclaration("GetComponentsInChildren<T extends UnityEngine.Component>(type: { new(): T }): T[]",
                    "GetComponentsInChildren", typeof(Type))
                .AddTSMethodDeclaration("GetComponentsInParent<T extends UnityEngine.Component>(type: { new(): T }, includeInactive: boolean): T[]",
                    "GetComponentsInParent", typeof(Type), typeof(bool))
                .AddTSMethodDeclaration("GetComponentsInParent<T extends UnityEngine.Component>(type: { new(): T }): T[]",
                    "GetComponentsInParent", typeof(Type))
            ;

            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            if (buildTarget != BuildTarget.iOS)
            {
                typePrefixBlacklist.Add("UnityEngine.Apple");
            }
            if (buildTarget != BuildTarget.Android)
            {
                typePrefixBlacklist.Add("UnityEngine.Android");
            }

            // fix d.ts, some C# classes use explicit implemented interface method
            SetTypeBlocked(typeof(UnityEngine.ILogHandler));
            SetTypeBlocked(typeof(UnityEngine.ISerializationCallbackReceiver));

            TransformType(typeof(object))
                .RenameTSMethod("$Equals", "Equals", typeof(object))
                .RenameTSMethod("$Equals", "Equals", typeof(object), typeof(object))
            ;

            TransformType(typeof(Vector3))
                .SetMethodBlocked("SqrMagnitude", typeof(Vector3))
                .SetMethodBlocked("Magnitude", typeof(Vector3))
                .AddTSMethodDeclaration("static Add(a: Vector3, b: Vector3): Vector3")
                .AddTSMethodDeclaration("static Sub(a: Vector3, b: Vector3): Vector3")
                .AddTSMethodDeclaration("static Mul(a: Vector3, b: Vector3): Vector3")
                .AddTSMethodDeclaration("static Div(a: Vector3, b: Vector3): Vector3")
                .AddTSMethodDeclaration("static Equals(a: Vector3, b: Vector3): boolean")
                .AddTSMethodDeclaration("Equals(b: Vector3): boolean")
                .AddTSMethodDeclaration("Inverse(): Vector3")
                .AddTSMethodDeclaration("Clone(): Vector3")
            ;

            TransformType(typeof(Vector2))
                .SetMethodBlocked("SqrMagnitude")
                .SetMethodBlocked("SqrMagnitude", typeof(Vector2))
                .AddTSMethodDeclaration("static Add(a: Vector2, b: Vector2): Vector2")
                .AddTSMethodDeclaration("static Sub(a: Vector2, b: Vector2): Vector2")
                .AddTSMethodDeclaration("static Mul(a: Vector2, b: Vector2): Vector2")
                .AddTSMethodDeclaration("static Div(a: Vector2, b: Vector2): Vector2")
                .AddTSMethodDeclaration("static Equals(a: Vector2, b: Vector2): boolean")
                .AddTSMethodDeclaration("Equals(b: Vector2): boolean")
                .AddTSMethodDeclaration("Inverse(): Vector2")
                .AddTSMethodDeclaration("Clone(): Vector2")
            ;

            TransformType(typeof(Quaternion))
                .AddTSMethodDeclaration("Clone(): Quaternion")
                .AddTSMethodDeclaration("static Mul(lhs: Quaternion, rhs: Vector3): Vector3")
                .AddTSMethodDeclaration("static Mul(lhs: Quaternion, rhs: Quaternion): Quaternion")
            ;
            // SetTypeBlocked(typeof(RendererExtensions));
            SetTypeBlocked(typeof(UnityEngine.UI.ILayoutGroup));
            SetTypeBlocked(typeof(UnityEngine.UI.ILayoutSelfController));

            TransformType(typeof(UnityEngine.UI.PositionAsUV1))
                .SetMemberBlocked("ModifyMesh");
            TransformType(typeof(UnityEngine.UI.Shadow))
                .SetMemberBlocked("ModifyMesh");
            TransformType(typeof(UnityEngine.UI.Outline))
                .SetMemberBlocked("ModifyMesh");
            TransformType(typeof(UnityEngine.UI.Graphic))
                .SetMemberBlocked("OnRebuildRequested");
            TransformType(typeof(UnityEngine.Texture))
                .SetMemberBlocked("imageContentsHash");
            TransformType(typeof(UnityEngine.UI.Text))
                .SetMemberBlocked("OnRebuildRequested");
            TransformType(typeof(UnityEngine.Input))
                .SetMemberBlocked("IsJoystickPreconfigured"); // specific platform available only
            TransformType(typeof(UnityEngine.MonoBehaviour))
                .SetMemberBlocked("runInEditMode"); // editor only
            TransformType(typeof(UnityEngine.QualitySettings))
                .SetMemberBlocked("streamingMipmapsRenderersPerFrame");

            // editor 使用的 .net 与 player 所用存在差异, 这里屏蔽不存在的成员
            TransformType(typeof(double))
                .SetMemberBlocked("IsFinite")
            ;
            TransformType(typeof(float))
                .SetMemberBlocked("IsFinite")
            ;
            TransformType(typeof(string))
                .SetMemberBlocked("Chars")
            ;

            AddTSTypeNameMap(typeof(sbyte), "number");
            AddTSTypeNameMap(typeof(byte), "number");
            AddTSTypeNameMap(typeof(int), "number");
            AddTSTypeNameMap(typeof(uint), "number");
            AddTSTypeNameMap(typeof(short), "number");
            AddTSTypeNameMap(typeof(ushort), "number");
            AddTSTypeNameMap(typeof(long), "number");
            AddTSTypeNameMap(typeof(ulong), "number");
            AddTSTypeNameMap(typeof(float), "number");
            AddTSTypeNameMap(typeof(double), "number");
            AddTSTypeNameMap(typeof(bool), "boolean");
            AddTSTypeNameMap(typeof(string), "string");
            AddTSTypeNameMap(typeof(char), "string");
            AddTSTypeNameMap(typeof(void), "void");
            AddTSTypeNameMap(typeof(LayerMask), "UnityEngine.LayerMask", "number");
            AddTSTypeNameMap(typeof(Color), "UnityEngine.Color");
            AddTSTypeNameMap(typeof(Color32), "UnityEngine.Color32");
            AddTSTypeNameMap(typeof(Vector2), "UnityEngine.Vector2");
            AddTSTypeNameMap(typeof(Vector2Int), "UnityEngine.Vector2Int");
            AddTSTypeNameMap(typeof(Vector3), "UnityEngine.Vector3"); // 已优化, 在 VM 初始化时替换为 DuktapeJS.Vector3
            AddTSTypeNameMap(typeof(Vector3Int), "UnityEngine.Vector3Int");
            AddTSTypeNameMap(typeof(Vector4), "UnityEngine.Vector4");
            AddTSTypeNameMap(typeof(Quaternion), "UnityEngine.Quaternion");
            AddTSTypeNameMap(typeof(ScriptValueArray), "any[]");
            AddTSTypeNameMap(typeof(QuickJS.IO.ByteBuffer), "jsb.ByteBuffer", "Buffer");

            TransformType(typeof(QuickJS.IO.ByteBuffer))
                .Rename("jsb.ByteBuffer")
                .SetMemberBlocked("_SetPosition")
                .SetMethodBlocked("ReadAllBytes", typeof(IntPtr))
                .SetMethodBlocked("WriteBytes", typeof(IntPtr), typeof(int));

            AddExportedType(typeof(ScriptBehaviour));
            AddExportedType(typeof(QuickJS.IO.ByteBuffer));

            AddCSTypeNameMap(typeof(sbyte), "sbyte");
            AddCSTypeNameMap(typeof(byte), "byte");
            AddCSTypeNameMap(typeof(int), "int");
            AddCSTypeNameMap(typeof(uint), "uint");
            AddCSTypeNameMap(typeof(short), "short");
            AddCSTypeNameMap(typeof(ushort), "ushort");
            AddCSTypeNameMap(typeof(long), "long");
            AddCSTypeNameMap(typeof(ulong), "ulong");
            AddCSTypeNameMap(typeof(float), "float");
            AddCSTypeNameMap(typeof(double), "double");
            AddCSTypeNameMap(typeof(bool), "bool");
            AddCSTypeNameMap(typeof(string), "string");
            AddCSTypeNameMap(typeof(char), "char");
            AddCSTypeNameMap(typeof(System.Object), "object");
            AddCSTypeNameMap(typeof(void), "void");

            // AddCSTypePusherMap(typeof(bool), "DuktapeDLL.duk_push_boolean");
            // AddCSTypePusherMap(typeof(char), "DuktapeDLL.duk_push_int");
            // AddCSTypePusherMap(typeof(byte), "DuktapeDLL.duk_push_int");
            // AddCSTypePusherMap(typeof(sbyte), "DuktapeDLL.duk_push_int");
            // AddCSTypePusherMap(typeof(short), "DuktapeDLL.duk_push_int");
            // AddCSTypePusherMap(typeof(ushort), "DuktapeDLL.duk_push_int");
            // AddCSTypePusherMap(typeof(int), "DuktapeDLL.duk_push_int");
            // AddCSTypePusherMap(typeof(uint), "DuktapeDLL.duk_push_uint");
            // AddCSTypePusherMap(typeof(long), "DuktapeDLL.duk_push_number");
            // AddCSTypePusherMap(typeof(ulong), "DuktapeDLL.duk_push_number");
            // AddCSTypePusherMap(typeof(float), "DuktapeDLL.duk_push_number");
            // AddCSTypePusherMap(typeof(double), "DuktapeDLL.duk_push_number");
            // AddCSTypePusherMap(typeof(string), "DuktapeDLL.duk_push_string");

            Initialize();
        }

        public void SetTypeBlocked(Type type)
        {
            blacklist.Add(type);
        }

        public bool GetTSMethodDeclaration(MethodBase method, out string code)
        {
            var transform = GetTypeTransform(method.DeclaringType);
            if (transform != null)
            {
                return transform.GetTSMethodDeclaration(method, out code);
            }
            code = null;
            return false;
        }

        public bool GetTSMethodRename(MethodBase method, out string code)
        {
            var transform = GetTypeTransform(method.DeclaringType);
            if (transform != null)
            {
                return transform.GetTSMethodRename(method, out code);
            }
            code = null;
            return false;
        }

        public TypeTransform GetTypeTransform(Type type)
        {
            TypeTransform transform;
            return typesTarnsform.TryGetValue(type, out transform) ? transform : null;
        }

        public TypeTransform TransformType(Type type)
        {
            TypeTransform transform;
            if (!typesTarnsform.TryGetValue(type, out transform))
            {
                typesTarnsform[type] = transform = new TypeTransform(type);
            }
            return transform;
        }

        private static bool _FindFilterBindingProcess(Type type, object l)
        {
            return type == typeof(IBindingProcess);
        }

        private void Initialize()
        {
            var assembly = Assembly.Load("Assembly-CSharp-Editor");
            var types = assembly.GetExportedTypes();
            for (int i = 0, size = types.Length; i < size; i++)
            {
                var type = types[i];
                if (type.IsAbstract)
                {
                    continue;
                }
                try
                {
                    var interfaces = type.FindInterfaces(_FindFilterBindingProcess, null);
                    if (interfaces != null && interfaces.Length > 0)
                    {
                        var ctor = type.GetConstructor(Type.EmptyTypes);
                        var inst = ctor.Invoke(null) as IBindingProcess;
                        inst.OnInitialize(this);
                        _bindingProcess.Add(inst);
                        Debug.Log($"add binding process: {type}");
                        // _bindingProcess.Add
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"failed to add binding process: {type}\n{exception}");
                }
            }
        }

        // TS: 添加保留字, CS中相关变量名等会自动重命名注册到js中
        public static void AddTSKeywords(params string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                _tsKeywords.Add(keyword);
            }
        }

        // 指定类型在 ts 声明中的映射名 (可以指定多项)
        public void AddTSTypeNameMap(Type type, params string[] names)
        {
            List<string> list;
            if (!_tsTypeNameMap.TryGetValue(type, out list))
            {
                _tsTypeNameMap[type] = list = new List<string>();
            }
            list.AddRange(names);
        }

        // CS, 添加类型名称映射, 用于简化导出时的常用类型名
        public void AddCSTypeNameMap(Type type, string name)
        {
            _csTypeNameMap[type] = name;
            _csTypeNameMapS[type.FullName] = name;
            _csTypeNameMapS[GetCSNamespace(type) + type.Name] = name;
        }

        public void AddCSTypePusherMap(Type type, string name)
        {
            _csTypePusherMap[type] = name;
        }

        // 增加导出类型 (需要在 Collect 阶段进行)
        //NOTE: editor mscorlib 与 runtime 存在差异, 需要手工 block 差异
        public TypeTransform AddExportedType(Type type, bool importBaseType = false)
        {
            if (type.IsGenericTypeDefinition)
            {
                whitelist.Add(type);
                return null;
            }
            var tt = TransformType(type);
            if (!exportedTypes.ContainsKey(type))
            {
                var typeBindingInfo = new TypeBindingInfo(this, type);
                exportedTypes.Add(type, typeBindingInfo);
                log.AppendLine($"AddExportedType: {type} Assembly: {type.Assembly}");

                var baseType = type.BaseType;
                if (baseType != null && !IsExportingBlocked(baseType))
                {
                    // 检查具体化泛型基类 (如果基类泛型定义在显式导出清单中, 那么导出此具体化类)
                    // Debug.LogFormat("{0} IsConstructedGenericType:{1} {2} {3}", type, type.IsConstructedGenericType, type.IsGenericType, importBaseType);
                    if (baseType.IsConstructedGenericType)
                    {
                        if (IsExportingExplicit(baseType.GetGenericTypeDefinition()))
                        {
                            AddExportedType(baseType);
                        }
                    }
                    else if (!baseType.IsGenericType)
                    {
                        if (importBaseType)
                        {
                            AddExportedType(baseType, importBaseType);
                        }
                    }
                }
            }
            return tt;
        }

        public bool RemoveExportedType(Type type)
        {
            return exportedTypes.Remove(type);
        }

        public DelegateBindingInfo GetDelegateBindingInfo(Type type)
        {
            Type target;
            if (redirectDelegates.TryGetValue(type, out target))
            {
                type = target;
            }
            DelegateBindingInfo delegateBindingInfo;
            if (exportedDelegates.TryGetValue(type, out delegateBindingInfo))
            {
                return delegateBindingInfo;
            }
            return null;
        }

        public static bool ContainsPointer(MethodBase method)
        {
            var parameters = method.GetParameters();
            for (int i = 0, size = parameters.Length; i < size; i++)
            {
                var parameterType = parameters[i].ParameterType;
                if (parameterType.IsPointer)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsGenericMethod(MethodBase method)
        {
            return method.GetGenericArguments().Length > 0;
        }

        public static bool IsUnsupported(MethodBase method)
        {
            return ContainsPointer(method) || IsGenericMethod(method);
        }

        // 收集所有 delegate 类型
        public void CollectDelegate(Type type)
        {
            if (type == null || type.BaseType != typeof(MulticastDelegate))
            {
                return;
            }
            if (!exportedDelegates.ContainsKey(type))
            {
                var invoke = type.GetMethod("Invoke");
                var returnType = invoke.ReturnType;
                var parameters = invoke.GetParameters();
                if (ContainsPointer(invoke))
                {
                    log.AppendLine("skip unsafe (pointer) delegate: [{0}] {1}", type, invoke);
                    return;
                }
                // 是否存在等价 delegate
                foreach (var kv in exportedDelegates)
                {
                    if (kv.Value.Equals(returnType, parameters))
                    {
                        log.AppendLine("skip delegate: {0} && {1}", kv.Value, type);
                        kv.Value.types.Add(type);
                        redirectDelegates[type] = kv.Key;
                        return;
                    }
                }
                var delegateBindingInfo = new DelegateBindingInfo(returnType, parameters);
                delegateBindingInfo.types.Add(type);
                exportedDelegates.Add(type, delegateBindingInfo);
                log.AppendLine("add delegate: {0}", type);
                for (var i = 0; i < parameters.Length; i++)
                {
                    CollectDelegate(parameters[i].ParameterType);
                }
            }
        }

        public bool IsExported(Type type)
        {
            return exportedTypes.ContainsKey(type);
        }

        public string GetTSRefWrap(string name)
        {
            return $"DuktapeJS.Ref<{name}>";
        }

        public string GetTSTypeFullName(ParameterInfo parameter)
        {
            var parameterType = parameter.ParameterType;
            return GetTSTypeFullName(parameterType, parameter.IsOut);
        }

        // 获取 type 在 typescript 中对应类型名
        public string GetTSTypeFullName(Type type)
        {
            return GetTSTypeFullName(type, false);
        }

        public string GetTSTypeFullName(Type type, bool isOut)
        {
            if (type == null || type == typeof(void))
            {
                return "void";
            }
            if (type.IsByRef)
            {
                if (isOut)
                {
                    return $"DuktapeJS.Out<{GetTSTypeFullName(type.GetElementType())}>";
                }
                return $"DuktapeJS.Ref<{GetTSTypeFullName(type.GetElementType())}>";
            }
            List<string> names;
            if (_tsTypeNameMap.TryGetValue(type, out names))
            {
                return names.Count > 1 ? $"({String.Join(" | ", names)})" : names[0];
            }
            if (type.IsArray)
            {
                if (type.GetElementType() == typeof(byte))
                {
                    return "Buffer";
                }
                var elementType = type.GetElementType();
                return GetTSTypeFullName(elementType) + "[]";
            }
            var info = GetExportedType(type);
            if (info != null)
            {
                return info.jsFullName;
            }
            if (type.BaseType == typeof(MulticastDelegate))
            {
                var delegateBindingInfo = GetDelegateBindingInfo(type);
                if (delegateBindingInfo != null)
                {
                    var nargs = delegateBindingInfo.parameters.Length;
                    var ret = GetTSTypeFullName(delegateBindingInfo.returnType);
                    var t_arglist = (nargs > 0 ? ", " : "") + GetTSArglistTypes(delegateBindingInfo.parameters, false);
                    var v_arglist = GetTSArglistTypes(delegateBindingInfo.parameters, true);
                    return $"DuktapeJS.Delegate{nargs}<{ret}{t_arglist}> | (({v_arglist}) => {ret})";
                }
            }
            return "any";
        }

        public string GetCSNamespace(Type type)
        {
            return GetCSNamespace(type.Namespace);
        }

        public string GetCSNamespace(string ns)
        {
            return string.IsNullOrEmpty(ns) ? "" : (ns + ".");
        }

        // 生成参数对应的字符串形式参数列表定义 (typescript)
        public string GetTSArglistTypes(ParameterInfo[] parameters, bool withVarName)
        {
            var size = parameters.Length;
            var arglist = "";
            if (size == 0)
            {
                return arglist;
            }
            for (var i = 0; i < size; i++)
            {
                var parameter = parameters[i];
                var typename = GetTSTypeFullName(parameter.ParameterType);
                // if (parameter.IsOut && parameter.ParameterType.IsByRef)
                // {
                //     arglist += "out ";
                // }
                // else if (parameter.ParameterType.IsByRef)
                // {
                //     arglist += "ref ";
                // }
                if (withVarName)
                {
                    arglist += GetTSVariable(parameter.Name) + ": ";
                }
                arglist += typename;
                // arglist += " ";
                // arglist += parameter.Name;
                if (i != size - 1)
                {
                    arglist += ", ";
                }
            }
            return arglist;
        }

        public string GetThrowError(string err)
        {
            return $"JSApi.JS_ThrowInternalError(ctx, \"{err}\");";
        }

        public string GetScriptObjectGetter(Type type, string ctx, string index, string varname)
        {
            var getter = GetScriptObjectPropertyGetter(type);
            return $"{getter}({ctx}, {index}, out {varname});";
        }

        private string GetScriptObjectPropertyGetter(Type type)
        {
            if (type.IsByRef)
            {
                return GetScriptObjectPropertyGetter(type.GetElementType());
            }
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                return GetScriptObjectPropertyGetter(elementType) + "_array"; //TODO: 嵌套数组的问题
            }
            if (type.IsValueType)
            {
                if (type.IsPrimitive)
                {
                    return "js_get_primitive";
                }
                if (type.IsEnum)
                {
                    return "js_get_enumvalue";
                }
                return "js_get_structvalue";
            }
            if (type == typeof(string))
            {
                return "js_get_primitive";
            }
            if (type.BaseType == typeof(MulticastDelegate))
            {
                return "js_get_delegate";
            }
            if (type == typeof(Type))
            {
                return "js_get_type";
            }
            return "js_get_classvalue";
        }

        public string GetScriptObjectPusher(Type type)
        {
            if (type.IsByRef)
            {
                return GetScriptObjectPusher(type.GetElementType());
            }
            string pusher;
            if (_csTypePusherMap.TryGetValue(type, out pusher))
            {
                return pusher;
            }
            if (type.BaseType == typeof(MulticastDelegate))
            {
                return "js_push_delegate";
            }
            if (type.IsValueType)
            {
                if (type.IsPrimitive)
                {
                    return "js_push_primitive";
                }
                if (type.IsEnum)
                {
                    return "js_push_enumvalue";
                }
                return "js_push_structvalue";
            }
            if (type == typeof(string))
            {
                return "js_push_primitive";
            }
            return "js_push_classvalue";
        }

        public static string GetTSVariable(string name)
        {
            if (_tsKeywords.Contains(name))
            {
                return name + "_";
            }
            return name;
        }

        // 保证生成一个以 prefix 为前缀, 与参数列表中所有参数名不同的名字
        public string GetUniqueName(ParameterInfo[] parameters, string prefix)
        {
            return GetUniqueName(parameters, prefix, 0);
        }

        public string GetUniqueName(ParameterInfo[] parameters, string prefix, int index)
        {
            var size = parameters.Length;
            var name = prefix + index;
            for (var i = 0; i < size; i++)
            {
                var parameter = parameters[i];
                if (parameter.Name == prefix)
                {
                    return GetUniqueName(parameters, prefix, index + 1);
                }
            }
            return name;
        }

        // 获取父类的ts声明 (沿继承链上溯直到存在导出)
        public string GetTSSuperName(TypeBindingInfo typeBindingInfo)
        {
            var super = typeBindingInfo.super;
            while (super != null)
            {
                var superBindingInfo = GetExportedType(super);
                if (superBindingInfo != null)
                {
                    return GetTSTypeFullName(superBindingInfo.type);
                }
                super = super.BaseType;
            }
            return "";
        }

        // 获取实现的接口的ts声明
        public string GetTSInterfacesName(TypeBindingInfo typeBindingInfo)
        {
            var interfaces = typeBindingInfo.type.GetInterfaces();
            var str = "";
            foreach (var @interface in interfaces)
            {
                var interfaceBindingInfo = GetExportedType(@interface);
                if (interfaceBindingInfo != null)
                {
                    // Debug.Log($"{typeBindingInfo.type.Name} implements {@interface.Name}");
                    str += GetTSTypeFullName(interfaceBindingInfo.type) + ", ";
                }
            }
            if (str.Length > 0)
            {
                str = str.Substring(0, str.Length - 2);
            }
            return str;
        }

        // 生成参数对应的字符串形式参数列表 (csharp)
        public string GetCSArglistDecl(ParameterInfo[] parameters)
        {
            var size = parameters.Length;
            var arglist = "";
            if (size == 0)
            {
                return arglist;
            }
            for (var i = 0; i < size; i++)
            {
                var parameter = parameters[i];
                var typename = GetCSTypeFullName(parameter.ParameterType);
                if (parameter.IsOut && parameter.ParameterType.IsByRef)
                {
                    arglist += "out ";
                }
                else if (parameter.ParameterType.IsByRef)
                {
                    arglist += "ref ";
                }
                arglist += typename;
                arglist += " ";
                arglist += parameter.Name;
                if (i != size - 1)
                {
                    arglist += ", ";
                }
            }
            return arglist;
        }

        // 获取 type 在 绑定代码 中对应类型名
        public string GetCSTypeFullName(Type type)
        {
            return GetCSTypeFullName(type, true);
        }

        public string GetCSTypeFullName(Type type, bool shortName)
        {
            // Debug.LogFormat("{0} Array {1} ByRef {2} GetElementType {3}", type, type.IsArray, type.IsByRef, type.GetElementType());
            if (type.IsGenericType)
            {
                var @namespace = string.Empty;
                var classname = type.Name.Substring(0, type.Name.Length - 2);
                if (type.IsNested)
                {
                    var indexOf = type.FullName.IndexOf("+");
                    @namespace = type.FullName.Substring(0, indexOf) + ".";
                }
                else
                {
                    @namespace = GetCSNamespace(type);
                }
                var purename = @namespace + classname;
                var gargs = type.GetGenericArguments();
                purename += "<";
                for (var i = 0; i < gargs.Length; i++)
                {
                    var garg = gargs[i];
                    purename += GetCSTypeFullName(garg, shortName);
                    if (i != gargs.Length - 1)
                    {
                        purename += ", ";
                    }
                }
                purename += ">";
                return purename;
            }
            if (type.IsArray)
            {
                return GetCSTypeFullName(type.GetElementType(), shortName) + "[]";
            }
            if (type.IsByRef)
            {
                return GetCSTypeFullName(type.GetElementType(), shortName);
            }
            string name;
            if (shortName)
            {
                if (_csTypeNameMap.TryGetValue(type, out name))
                {
                    return name;
                }
            }
            var fullname = type.FullName.Replace('+', '.');
            if (fullname.Contains("`"))
            {
                fullname = new Regex(@"`\d", RegexOptions.None).Replace(fullname, "");
                fullname = fullname.Replace("[", "<");
                fullname = fullname.Replace("]", ">");
            }
            if (_csTypeNameMapS.TryGetValue(fullname, out name))
            {
                return name;
            }
            return fullname;
        }

        public TypeBindingInfo GetExportedType(Type type)
        {
            if (type == null)
            {
                return null;
            }
            TypeBindingInfo typeBindingInfo;
            return exportedTypes.TryGetValue(type, out typeBindingInfo) ? typeBindingInfo : null;
        }

        // 是否在黑名单中屏蔽, 或者已知无需导出的类型
        public bool IsExportingBlocked(Type type)
        {
            if (blacklist.Contains(type))
            {
                return true;
            }
            if (type.IsGenericType && !type.IsConstructedGenericType)
            {
                return true;
            }
            if (type.Name.Contains("<"))
            {
                return true;
            }
            if (type.IsDefined(typeof(JSBindingAttribute), false))
            {
                return true;
            }
            if (type.BaseType == typeof(Attribute))
            {
                return true;
            }
            if (type.BaseType == typeof(MulticastDelegate))
            {
                return true;
            }
            if (type.IsPointer)
            {
                return true;
            }
            var encloser = type;
            while (encloser != null)
            {
                if (encloser.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    return true;
                }
                encloser = encloser.DeclaringType;
            }
            for (int i = 0, size = typePrefixBlacklist.Count; i < size; i++)
            {
                if (type.FullName.StartsWith(typePrefixBlacklist[i]))
                {
                    return true;
                }
            }
            return false;
        }

        // 是否显式要求导出
        public bool IsExportingExplicit(Type type)
        {
            if (whitelist.Contains(type))
            {
                return true;
            }
            if (type.IsDefined(typeof(JSTypeAttribute), false))
            {
                return true;
            }
            return false;
        }

        private void OnPreCollectAssemblies()
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnPreCollectAssemblies(this);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnPreCollect]: {exception}");
                }
            }
        }

        private void OnPostCollectAssemblies()
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnPostCollectAssemblies(this);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnPostCollect]: {exception}");
                }
            }
        }

        private void OnPostExporting()
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnPostExporting(this);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnPostExporting]: {exception}");
                }
            }
        }

        private void OnPreExporting()
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnPreExporting(this);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnPreExporting]: {exception}");
                }
            }
        }

        private void OnPreCollectTypes()
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnPreCollectTypes(this);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnPreCollect]: {exception}");
                }
            }
        }

        private void OnPostCollectTypes()
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnPostCollectTypes(this);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnPostCollect]: {exception}");
                }
            }
        }

        private void OnPreGenerateType(TypeBindingInfo bindingInfo)
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnPreGenerateType(this, bindingInfo);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnPreGenerateType]: {exception}");
                }
            }
        }

        private void OnPostGenerateType(TypeBindingInfo bindingInfo)
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnPostGenerateType(this, bindingInfo);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnPostGenerateType]: {exception}");
                }
            }
        }

        public void OnPreGenerateDelegate(DelegateBindingInfo bindingInfo)
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnPreGenerateDelegate(this, bindingInfo);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnPreGenerateDelegate]: {exception}");
                }
            }
        }

        public void OnPostGenerateDelegate(DelegateBindingInfo bindingInfo)
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnPostGenerateDelegate(this, bindingInfo);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnPostGenerateDelegate]: {exception}");
                }
            }
        }

        private void OnCleanup()
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnCleanup(this);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnCleanup]: {exception}");
                }
            }
        }



        public void Collect()
        {
            // 收集直接类型, 加入 exportedTypes
            OnPreCollectAssemblies();
            AddAssemblies(false, prefs.explicitAssemblies.ToArray());
            AddAssemblies(true, prefs.implicitAssemblies.ToArray());
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                if (!assembly.IsDynamic)
                {
                    AddAssemblies(false, assembly.FullName);
                }
            }
            OnPostCollectAssemblies();

            OnPreExporting();
            ExportAssemblies(_explicitAssemblies, false);
            ExportAssemblies(_implicitAssemblies, true);
            ExportBuiltins();
            OnPostExporting();

            log.AppendLine("collecting members");
            log.AddTabLevel();
            OnPreCollectTypes();
            foreach (var typeBindingInfoKV in exportedTypes)
            {
                var typeBindingInfo = typeBindingInfoKV.Value;
                log.AppendLine("type: {0}", typeBindingInfo.type);
                log.AddTabLevel();
                typeBindingInfo.Collect();
                log.DecTabLevel();
            }
            OnPostCollectTypes();
            log.DecTabLevel();
        }

        public void AddAssemblies(bool implicitExport, params string[] assemblyNames)
        {
            if (implicitExport)
            {
                for (var i = 0; i < assemblyNames.Length; i++)
                {
                    var assemblyName = assemblyNames[i];
                    if (!_implicitAssemblies.Contains(assemblyName) && !_explicitAssemblies.Contains(assemblyName))
                    {
                        _implicitAssemblies.Add(assemblyName);
                    }
                }
            }
            else
            {
                for (var i = 0; i < assemblyNames.Length; i++)
                {
                    var assemblyName = assemblyNames[i];
                    if (!_implicitAssemblies.Contains(assemblyName) && !_explicitAssemblies.Contains(assemblyName))
                    {
                        _explicitAssemblies.Add(assemblyName);
                    }
                }
            }
        }

        public void RemoveAssemblies(params string[] assemblyNames)
        {
            foreach (var name in assemblyNames)
            {
                _implicitAssemblies.Remove(name);
                _explicitAssemblies.Remove(name);
            }
        }

        // 导出一些必要的基本类型 (预实现的辅助功能需要用到, DuktapeJS)
        private void ExportBuiltins()
        {
            AddExportedType(typeof(byte));
            AddExportedType(typeof(sbyte));
            AddExportedType(typeof(float));
            AddExportedType(typeof(double));
            AddExportedType(typeof(string));
            AddExportedType(typeof(int));
            AddExportedType(typeof(uint));
            AddExportedType(typeof(short));
            AddExportedType(typeof(ushort));
            AddExportedType(typeof(object));
            AddExportedType(typeof(Array));
            AddExportedType(typeof(Object));
            AddExportedType(typeof(Vector3));
        }

        // implicitExport: 默认进行导出(黑名单例外), 否则根据导出标记或手工添加
        private void ExportAssemblies(List<string> assemblyNames, bool implicitExport)
        {
            foreach (var assemblyName in assemblyNames)
            {
                log.AppendLine("assembly: {0}", assemblyName);
                log.AddTabLevel();
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    var types = assembly.GetExportedTypes();

                    log.AppendLine("types {0}", types.Length);
                    foreach (var type in types)
                    {
                        if (IsExportingBlocked(type))
                        {
                            log.AppendLine("blocked: {0}", type.FullName);
                            continue;
                        }
                        if (implicitExport || IsExportingExplicit(type))
                        {
                            log.AppendLine("export: {0}", type.FullName);
                            this.AddExportedType(type);
                            continue;
                        }
                        log.AppendLine("skip: {0}", type.FullName);
                    }
                }
                catch (Exception exception)
                {
                    log.AppendLine(exception.ToString());
                }
                log.DecTabLevel();
            }
        }

        // 清理多余文件
        public void Cleanup()
        {
            log.AppendLine("cleanup");
            log.AddTabLevel();
            Cleanup(_outputFiles, file =>
            {
                removedFiles.Add(file);
                log.AppendLine("remove unused file {0}", file);
            });
            OnCleanup();
            log.DecTabLevel();
        }

        public static void Cleanup(Dictionary<string, List<string>> excludedFilesKV, Action<string> ondelete)
        {
            foreach (var kv in excludedFilesKV)
            {
                var outDir = kv.Key;
                var excludedFiles = kv.Value;
                if (Directory.Exists(outDir))
                {
                    foreach (var file in Directory.GetFiles(outDir))
                    {
                        var nfile = file;
                        if (file.EndsWith(".meta"))
                        {
                            nfile = file.Substring(0, file.Length - 5);
                        }
                        // Debug.LogFormat("checking file {0}", nfile);
                        if (excludedFiles == null || !excludedFiles.Contains(nfile))
                        {
                            File.Delete(file);
                            if (ondelete != null)
                            {
                                ondelete(file);
                            }
                        }
                    }
                }
            }
        }

        public void AddOutputFile(string outDir, string filename)
        {
            List<string> list;
            if (!_outputFiles.TryGetValue(outDir, out list))
            {
                list = _outputFiles[outDir] = new List<string>();
            }
            list.Add(filename);
        }

        public void Generate(bool bTSDefinitionFiles)
        {
            var cg = new CodeGenerator(this);
            var csOutDir = prefs.procOutDir;
            var tsOutDir = prefs.procTypescriptDir;
            var tx = prefs.extraExt;
            // var tx = "";

            cg.tsDeclare.enabled = bTSDefinitionFiles;
            if (!Directory.Exists(csOutDir))
            {
                Directory.CreateDirectory(csOutDir);
            }
            if (!Directory.Exists(tsOutDir))
            {
                Directory.CreateDirectory(tsOutDir);
            }
            var cancel = false;
            var current = 0;
            var total = exportedTypes.Count;
            foreach (var typeKV in exportedTypes)
            {
                var typeBindingInfo = typeKV.Value;
                try
                {
                    current++;
                    cancel = EditorUtility.DisplayCancelableProgressBar(
                        "Generating",
                        $"{current}/{total}: {typeBindingInfo.FullName}",
                        (float)current / total);
                    if (cancel)
                    {
                        Warn("operation canceled");
                        break;
                    }
                    if (!typeBindingInfo.omit)
                    {
                        cg.Clear();
                        OnPreGenerateType(typeBindingInfo);
                        cg.Generate(typeBindingInfo);
                        OnPostGenerateType(typeBindingInfo);
                        cg.WriteTo(csOutDir, tsOutDir, typeBindingInfo.GetFileName(), tx);
                    }
                }
                catch (Exception exception)
                {
                    Error($"generate failed {typeBindingInfo.type.FullName}: {exception.Message}");
                    Debug.LogError(exception.StackTrace);
                }
            }

            if (!cancel)
            {
                try
                {
                    var exportedDelegatesArray = new DelegateBindingInfo[this.exportedDelegates.Count];
                    this.exportedDelegates.Values.CopyTo(exportedDelegatesArray, 0);

                    cg.Clear();
                    cg.Generate(exportedDelegatesArray);
                    cg.WriteTo(csOutDir, tsOutDir, CodeGenerator.NameOfDelegates, tx);
                }
                catch (Exception exception)
                {
                    Error($"generate delegates failed: {exception.Message}");
                    Debug.LogError(exception.StackTrace);
                }
            }

            var logPath = prefs.logPath;
            File.WriteAllText(logPath, log.ToString());
            EditorUtility.ClearProgressBar();
        }

        public void Report()
        {
            var now = DateTime.Now;
            var ts = now.Subtract(dateTime);
            Debug.LogFormat("generated {0} type(s), {1} delegate(s), {2} deletion(s) in {3:0.##} seconds.",
                exportedTypes.Count,
                exportedDelegates.Count,
                removedFiles.Count,
                ts.TotalSeconds);
        }
    }
}