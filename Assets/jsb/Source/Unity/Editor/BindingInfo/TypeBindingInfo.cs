using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public class TypeBindingInfo
    {
        public readonly BindingManager bindingManager;
        public readonly Type type;
        public readonly TypeTransform transform;

        public TypeBindingFlags bindingFlags { get { return transform.bindingFlags; } }

        /// <summary>
        /// 是否生成绑定代码
        /// </summary>
        public bool genBindingCode => (bindingFlags & TypeBindingFlags.BindingCode) != 0;

        /// <summary>
        /// 是否只生成于编辑器运行时
        /// </summary>
        public bool isEditorRuntime => transform.isEditorRuntime;

        // 父类类型
        public Type super
        {
            get { return type.BaseType; }
        }

        public bool omit
        {
            get { return type.IsDefined(typeof(JSOmitAttribute)); }
        }

        public readonly string csBindingName; // 绑定代码名

        //TODO: 模块包装, 生成模块加载代码, 添加模块依赖
        public readonly string jsModule; // js 模块名

        public readonly string jsNamespace; // js 命名空间

        public readonly string jsTypeName; // 不带泛型部分的js注册名
        public readonly string jsName; // js注册名 (带平面化的泛型部分)

        public List<OperatorBindingInfo> operators = new List<OperatorBindingInfo>();

        public Dictionary<string, MethodBindingInfo> methods = new Dictionary<string, MethodBindingInfo>();
        public Dictionary<string, MethodBindingInfo> staticMethods = new Dictionary<string, MethodBindingInfo>();

        public Dictionary<string, PropertyBindingInfo> properties = new Dictionary<string, PropertyBindingInfo>();
        public Dictionary<string, FieldBindingInfo> fields = new Dictionary<string, FieldBindingInfo>();
        public Dictionary<string, EventBindingInfo> events = new Dictionary<string, EventBindingInfo>();
        public Dictionary<string, DelegateBindingInfo> delegates = new Dictionary<string, DelegateBindingInfo>();
        public ConstructorBindingInfo constructors;

        public Assembly Assembly
        {
            get { return type.Assembly; }
        }

        // full name without generic parameters
        public string jsFullTypeName
        {
            get { return string.IsNullOrEmpty(jsNamespace) ? jsTypeName : jsNamespace + "." + jsTypeName; }
        }

        public string jsFullName
        {
            get { return string.IsNullOrEmpty(jsNamespace) ? jsName : jsNamespace + "." + jsName; }
        }

        public string FullName
        {
            get { return type.FullName; }
        }

        public bool IsEnum
        {
            get { return type.IsEnum; }
        }

        public string GetNamingAttribute(MethodInfo info)
        {
            var naming = info.GetCustomAttribute(typeof(JSNamingAttribute), false) as JSNamingAttribute;
            if (naming != null && !string.IsNullOrEmpty(naming.name))
            {
                return naming.name;
            }

            if (info.IsSpecialName)
            {
                switch (info.Name)
                {
                    case "get_Item": return "$GetValue";
                    case "set_Item": return "$SetValue";
                }
            }

            return bindingManager.ApplyNameRule(transform.GetNameRule(info), info.Name);
        }

        public static string GetNamingAttribute(MemberInfo info)
        {
            var naming = info.GetCustomAttribute(typeof(JSNamingAttribute), false) as JSNamingAttribute;
            if (naming != null && !string.IsNullOrEmpty(naming.name))
            {
                return naming.name;
            }

            return info.Name;
        }

        public TypeBindingInfo(BindingManager bindingManager, Type type, TypeTransform typeTransform)
        {
            this.bindingManager = bindingManager;
            this.type = type;
            this.transform = typeTransform;
            var naming = this.transform.GetTypeNaming() ?? GetNamingAttribute(type);
            var indexOfTypeName = naming.LastIndexOf('.');
            if (indexOfTypeName >= 0) // 内部类
            {
                this.jsNamespace = naming.Substring(0, indexOfTypeName);
                this.jsName = naming.Substring(indexOfTypeName + 1);
                this.jsTypeName = this.jsName;
            }
            else
            {
                if (type.DeclaringType != null)
                {
                    if (string.IsNullOrEmpty(type.Namespace))
                    {
                        this.jsNamespace = type.DeclaringType.Name;
                    }
                    else
                    {
                        this.jsNamespace = $"{type.Namespace}.{type.DeclaringType.Name}";
                    }
                }
                else
                {
                    this.jsNamespace = type.Namespace ?? "";
                }

                if (type.IsGenericType)
                {
                    if (type.IsGenericTypeDefinition)
                    {
                        this.jsName = naming.Substring(0, naming.IndexOf('`'));
                        this.jsTypeName = this.jsName;

                        this.jsName += "<";
                        var gArgs = type.GetGenericArguments();

                        for (var i = 0; i < gArgs.Length; i++)
                        {
                            this.jsName += gArgs[i].Name;
                            if (i != gArgs.Length - 1)
                            {
                                this.jsName += ", ";
                            }
                        }
                        this.jsName += ">";
                    }
                    else
                    {
                        this.jsName = naming.Substring(0, naming.IndexOf('`'));
                        this.jsTypeName = this.jsName;

                        foreach (var gp in type.GetGenericArguments())
                        {
                            this.jsName += "_" + gp.Name;
                        }
                    }
                }
                else
                {
                    this.jsName = naming;
                    this.jsTypeName = this.jsName;
                }
            }

            this.csBindingName = bindingManager.prefs.typeBindingPrefix + (this.jsNamespace + "_" + this.jsName).Replace('.', '_').Replace('+', '_').Replace('<', '_').Replace('>', '_');
            this.constructors = new ConstructorBindingInfo(type);
        }

        // 将类型名转换成简单字符串 (比如用于文件名)
        public string GetFileName()
        {
            if (type.IsGenericType)
            {
                var selfname = string.IsNullOrEmpty(type.Namespace) ? "" : (type.Namespace + "_");
                selfname += type.Name.Substring(0, type.Name.IndexOf('`'));
                foreach (var gp in type.GetGenericArguments())
                {
                    selfname += "_" + gp.Name;
                }

                return selfname.Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace("`", "_");
            }

            var filename = type.FullName.Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace("`", "_");
            return filename;
        }

        public void AddEvent(EventInfo eventInfo)
        {
            try
            {
                bindingManager.CollectDelegate(eventInfo.EventHandlerType);
                events.Add(eventInfo.Name, new EventBindingInfo(type, eventInfo));
                bindingManager.Info("[AddEvent] {0}.{1}", type, eventInfo.Name);
            }
            catch (Exception exception)
            {
                bindingManager.Error("AddEvent failed {0} @ {1}: {2}", eventInfo, type, exception.Message);
            }
        }

        public void AddField(FieldInfo fieldInfo)
        {
            try
            {
                bindingManager.CollectDelegate(fieldInfo.FieldType);
                if (fieldInfo.FieldType.BaseType == typeof(MulticastDelegate))
                {
                    delegates.Add(fieldInfo.Name, new DelegateBindingInfo(type, fieldInfo));
                    bindingManager.Info("[AddField] As Delegate: {0}.{1}", type, fieldInfo.Name);
                }
                else
                {
                    fields.Add(fieldInfo.Name, new FieldBindingInfo(fieldInfo));
                    bindingManager.Info("[AddField] {0}.{1}", type, fieldInfo.Name);
                }
            }
            catch (Exception exception)
            {
                bindingManager.Error("AddField failed {0} @ {1}: {2}", fieldInfo, type, exception.Message);
            }
        }

        public void AddProperty(PropertyInfo propInfo)
        {
            try
            {
                bindingManager.CollectDelegate(propInfo.PropertyType);
                if (propInfo.PropertyType.BaseType == typeof(MulticastDelegate))
                {
                    delegates.Add(propInfo.Name, new DelegateBindingInfo(type, propInfo));
                    bindingManager.Info("[AddProperty] As Delegate: {0}.{1}", type, propInfo.Name);
                }
                else
                {
                    properties.Add(propInfo.Name, new PropertyBindingInfo(propInfo));
                    bindingManager.Info("[AddProperty] {0}.{1}", type, propInfo.Name);
                }
            }
            catch (Exception exception)
            {
                bindingManager.Error("AddProperty failed {0} @ {1}: {2}", propInfo, type, exception.Message);
            }
        }

        public static bool IsSupportedOperators(MethodInfo methodInfo)
        {
            return methodInfo.IsSpecialName && methodInfo.Name.StartsWith("op_");
        }

        public void AddMethod(MethodInfo methodInfo)
        {
            if (this.transform.IsBlocked(methodInfo))
            {
                bindingManager.Info("skip blocked method: {0}", methodInfo.Name);
                return;
            }

            var isExtension = BindingManager.IsExtensionMethod(methodInfo);
            var isStatic = methodInfo.IsStatic && !isExtension;
            var methodCSName = methodInfo.Name;
            var methodJSName = GetNamingAttribute(methodInfo);
            if (IsSupportedOperators(methodInfo))
            {
                var parameters = methodInfo.GetParameters();
                var declaringType = methodInfo.DeclaringType;
                OperatorBindingInfo operatorBindingInfo = null;
                switch (methodCSName)
                {
                    case "op_LessThan":
                        if (parameters.Length == 2)
                        {
                            if (parameters[0].ParameterType == declaringType && parameters[1].ParameterType == declaringType)
                            {
                                operatorBindingInfo = new OperatorBindingInfo(methodInfo, isExtension, isStatic, methodCSName, "<", "<", 2);
                            }
                        }
                        break;
                    case "op_Addition":
                        if (parameters.Length == 2)
                        {
                            if (parameters[0].ParameterType == declaringType && parameters[1].ParameterType == declaringType)
                            {
                                operatorBindingInfo = new OperatorBindingInfo(methodInfo, isExtension, isStatic, methodCSName, "+", "+", 2);
                            }
                        }
                        break;
                    case "op_Subtraction":
                        if (parameters.Length == 2)
                        {
                            if (parameters[0].ParameterType == declaringType && parameters[1].ParameterType == declaringType)
                            {
                                operatorBindingInfo = new OperatorBindingInfo(methodInfo, isExtension, isStatic, methodCSName, "-", "-", 2);
                            }
                        }
                        break;
                    case "op_Equality":
                        if (parameters.Length == 2)
                        {
                            if (parameters[0].ParameterType == declaringType && parameters[1].ParameterType == declaringType)
                            {
                                operatorBindingInfo = new OperatorBindingInfo(methodInfo, isExtension, isStatic, methodCSName, "==", "==", 2);
                            }
                        }
                        break;
                    case "op_Multiply":
                        if (parameters.Length == 2)
                        {
                            var op0 = bindingManager.GetExportedType(parameters[0].ParameterType);
                            var op1 = bindingManager.GetExportedType(parameters[1].ParameterType);
                            if (op0 != null && op1 != null)
                            {
                                var bindingName = methodCSName + "_" + op0.csBindingName + "_" + op1.csBindingName;
                                operatorBindingInfo = new OperatorBindingInfo(methodInfo, isExtension, isStatic, bindingName, "*", "*", 2);
                            }
                        }
                        break;
                    case "op_Division":
                        if (parameters.Length == 2)
                        {
                            var op0 = bindingManager.GetExportedType(parameters[0].ParameterType);
                            var op1 = bindingManager.GetExportedType(parameters[1].ParameterType);
                            if (op0 != null && op1 != null)
                            {
                                var bindingName = methodCSName + "_" + op0.csBindingName + "_" + op1.csBindingName;
                                operatorBindingInfo = new OperatorBindingInfo(methodInfo, isExtension, isStatic, bindingName, "/", "/", 2);
                            }
                        }
                        break;
                    case "op_UnaryNegation":
                        {
                            operatorBindingInfo = new OperatorBindingInfo(methodInfo, isExtension, isStatic, methodCSName, "neg", "-", 1);
                        }
                        break;
                }

                if (operatorBindingInfo != null)
                {
                    operators.Add(operatorBindingInfo);
                    CollectDelegate(methodInfo);
                    bindingManager.Info("[AddOperator] {0}.{1}", type, methodInfo);
                    return;
                }

                // fallback to normal method binding
            }

            var group = isStatic ? staticMethods : methods;
            MethodBindingInfo methodBindingInfo;
            if (!group.TryGetValue(methodCSName, out methodBindingInfo))
            {
                methodBindingInfo = new MethodBindingInfo(isStatic, methodCSName, methodJSName);
                group.Add(methodCSName, methodBindingInfo);
            }
            if (!methodBindingInfo.Add(methodInfo, isExtension))
            {
                bindingManager.Info("fail to add method: {0}", methodInfo.Name);
                return;
            }

            CollectDelegate(methodInfo);
            bindingManager.Info("[AddMethod] {0}.{1}", type, methodInfo);
        }

        private void CollectDelegate(MethodBase method)
        {
            var parameters = method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                bindingManager.CollectDelegate(parameters[i].ParameterType);
            }
        }

        public void AddConstructor(ConstructorInfo constructorInfo)
        {
            if (this.transform.IsBlocked(constructorInfo))
            {
                bindingManager.Info("skip blocked constructor: {0}", constructorInfo.Name);
                return;
            }

            if (!constructors.Add(constructorInfo, false))
            {
                bindingManager.Info("add constructor failed: {0}", constructorInfo.Name);
                return;
            }
            CollectDelegate(constructorInfo);
            this.bindingManager.Info("[AddConstructor] {0}.{1}", type, constructorInfo);
        }

        // 收集所有 字段,属性,方法
        public void Collect()
        {
            var bindingFlags = Binding.DynamicType.PublicFlags;
            var fields = type.GetFields(bindingFlags);
            foreach (var field in fields)
            {
                if (field.IsSpecialName || field.Name.StartsWith("_JSFIX_"))
                {
                    bindingManager.Info("skip special field: {0}", field.Name);
                    continue;
                }

                if (field.FieldType.IsPointer)
                {
                    bindingManager.Info("skip pointer field: {0}", field.Name);
                    continue;
                }

                if (field.IsDefined(typeof(JSOmitAttribute), false))
                {
                    bindingManager.Info("skip omitted field: {0}", field.Name);
                    continue;
                }

                if (field.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    bindingManager.Info("skip obsolete field: {0}", field.Name);
                    continue;
                }

                if (transform.IsMemberBlocked(field.Name))
                {
                    bindingManager.Info("skip blocked field: {0}", field.Name);
                    continue;
                }

                if (transform.Filter(field))
                {
                    bindingManager.Info("skip filtered field: {0}", field.Name);
                    continue;
                }

                AddField(field);
            }

            var events = type.GetEvents(bindingFlags);
            foreach (var evt in events)
            {
                if (evt.IsSpecialName)
                {
                    bindingManager.Info("skip special event: {0}", evt.Name);
                    continue;
                }

                if (evt.EventHandlerType.IsPointer)
                {
                    bindingManager.Info("skip pointer event: {0}", evt.Name);
                    continue;
                }

                if (evt.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    bindingManager.Info("skip obsolete event: {0}", evt.Name);
                    continue;
                }

                if (evt.IsDefined(typeof(JSOmitAttribute), false))
                {
                    bindingManager.Info("skip omitted event: {0}", evt.Name);
                    continue;
                }

                if (transform.IsMemberBlocked(evt.Name))
                {
                    bindingManager.Info("skip blocked event: {0}", evt.Name);
                    continue;
                }

                if (transform.Filter(evt))
                {
                    bindingManager.Info("skip filtered event: {0}", evt.Name);
                    continue;
                }

                AddEvent(evt);
            }

            var properties = type.GetProperties(bindingFlags);
            foreach (var property in properties)
            {
                if (property.IsSpecialName)
                {
                    bindingManager.Info("skip special property: {0}", property.Name);
                    continue;
                }

                if (property.PropertyType.IsPointer)
                {
                    bindingManager.Info("skip pointer property: {0}", property.Name);
                    continue;
                }

                if (property.IsDefined(typeof(JSOmitAttribute), false))
                {
                    bindingManager.Info("skip omitted property: {0}", property.Name);
                    continue;
                }

                if (property.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    bindingManager.Info("skip obsolete property: {0}", property.Name);
                    continue;
                }

                if (transform.IsMemberBlocked(property.Name))
                {
                    bindingManager.Info("skip blocked property: {0}", property.Name);
                    continue;
                }

                if (transform.Filter(property))
                {
                    bindingManager.Info("skip filtered property: {0}", property.Name);
                    continue;
                }

                //NOTE: 索引访问
                if (property.Name == "Item")
                {
                    if (property.CanRead && property.GetMethod != null && property.GetMethod.IsPublic)
                    {
                        if (BindingManager.IsUnsupported(property.GetMethod))
                        {
                            bindingManager.Info("skip unsupported get-method: {0}", property.GetMethod);
                            continue;
                        }

                        AddMethod(property.GetMethod);
                    }

                    if (property.CanWrite && property.SetMethod != null && property.SetMethod.IsPublic)
                    {
                        if (BindingManager.IsUnsupported(property.SetMethod))
                        {
                            bindingManager.Info("skip unsupported set-method: {0}", property.SetMethod);
                            continue;
                        }

                        AddMethod(property.SetMethod);
                    }

                    // bindingManager.Info("skip indexer property: {0}", property.Name);
                    continue;
                }

                AddProperty(property);
            }

            if (!type.IsAbstract)
            {
                var constructors = type.GetConstructors();
                foreach (var constructor in constructors)
                {
                    if (constructor.IsDefined(typeof(JSOmitAttribute), false))
                    {
                        bindingManager.Info("skip omitted constructor: {0}", constructor);
                        continue;
                    }

                    if (constructor.IsDefined(typeof(ObsoleteAttribute), false))
                    {
                        bindingManager.Info("skip obsolete constructor: {0}", constructor);
                        continue;
                    }

                    if (BindingManager.ContainsPointer(constructor))
                    {
                        bindingManager.Info("skip pointer-param constructor: {0}", constructor);
                        continue;
                    }

                    if (BindingManager.ContainsByRefParameters(constructor))
                    {
                        bindingManager.Info("skip byref-param constructor: {0}", constructor);
                        continue;
                    }

                    if (transform.Filter(constructor))
                    {
                        bindingManager.Info("skip filtered constructor: {0}", constructor.Name);
                        continue;
                    }

                    AddConstructor(constructor);
                }
            }

            CollectMethods(type.GetMethods(bindingFlags));
            CollectMethods(bindingManager.GetTypeTransform(type).extensionMethods);
        }

        private void CollectMethods(IEnumerable<MethodInfo> methods)
        {
            foreach (var method in methods)
            {
                if (BindingManager.IsGenericMethod(method))
                {
                    bindingManager.Info("skip generic method: {0}", method);
                    continue;
                }

                if (BindingManager.ContainsPointer(method))
                {
                    bindingManager.Info("skip unsafe (pointer) method: {0}", method);
                    continue;
                }

                if (method.IsSpecialName)
                {
                    if (!IsSupportedOperators(method))
                    {
                        bindingManager.Info("skip special method: {0}", method);
                        continue;
                    }
                }

                if (method.IsDefined(typeof(JSOmitAttribute), false))
                {
                    bindingManager.Info("skip omitted method: {0}", method);
                    continue;
                }

                if (method.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    bindingManager.Info("skip obsolete method: {0}", method);
                    continue;
                }

                if (transform.IsMemberBlocked(method.Name))
                {
                    bindingManager.Info("skip blocked method: {0}", method.Name);
                    continue;
                }

                if (transform.Filter(method))
                {
                    bindingManager.Info("skip filtered method: {0}", method.Name);
                    continue;
                }

                if (BindingManager.IsExtensionMethod(method))
                {
                    var targetType = method.GetParameters()[0].ParameterType;
                    var targetInfo = bindingManager.GetExportedType(targetType);
                    if (targetInfo != null)
                    {
                        targetInfo.AddMethod(method);
                        continue;
                    }
                }

                AddMethod(method);
            }
        }
    }
}