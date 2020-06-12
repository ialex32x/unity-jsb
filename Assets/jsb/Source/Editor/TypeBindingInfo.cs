using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    // 所有具有相同参数数量的方法变体 (最少参数的情况下)
    public class MethodBaseVariant<T>
        where T : MethodBase
    {
        public int argc; // 最少参数数要求
        public List<T> plainMethods = new List<T>();
        public List<T> varargMethods = new List<T>();

        // 是否包含变参方法
        public bool isVararg
        {
            get { return varargMethods.Count > 0; }
        }

        public int count
        {
            get { return plainMethods.Count + varargMethods.Count; }
        }

        public MethodBaseVariant(int argc)
        {
            this.argc = argc;
        }

        public void Add(T methodInfo, bool isVararg)
        {
            //TODO: method 按照参数的具体程度排序以提高 match_type 的有效命中率
            if (isVararg)
            {
                this.varargMethods.Add(methodInfo);
            }
            else
            {
                this.plainMethods.Add(methodInfo);
            }
        }
    }

    public class MethodVariantComparer : IComparer<int>
    {
        public int Compare(int a, int b)
        {
            return a < b ? 1 : (a == b ? 0 : -1);
        }
    }

    public abstract class MethodBaseBindingInfo<T>
        where T : MethodBase
    {
        public string name { get; set; } // 绑定代码名
        public string regName { get; set; } // 导出名

        private int _count = 0;

        // 按照参数数逆序排序所有变体
        // 有相同参数数量要求的方法记录在同一个 Variant 中 (变参方法按最少参数数计算, 不计变参参数数)
        public SortedDictionary<int, MethodBaseVariant<T>> variants = new SortedDictionary<int, MethodBaseVariant<T>>(new MethodVariantComparer());

        public int count
        {
            get { return _count; }
        }

        public static bool IsVarargMethod(ParameterInfo[] parameters)
        {
            return parameters.Length > 0 && parameters[parameters.Length - 1].IsDefined(typeof(ParamArrayAttribute), false);
        }

        public void Add(T method, bool isExtension)
        {
            var parameters = method.GetParameters();
            var nargs = parameters.Length;
            var isVararg = IsVarargMethod(parameters);
            MethodBaseVariant<T> variants;
            if (isVararg)
            {
                nargs--;
            }

            if (isExtension)
            {
                nargs--;
            }

            if (!this.variants.TryGetValue(nargs, out variants))
            {
                variants = new MethodBaseVariant<T>(nargs);
                this.variants.Add(nargs, variants);
            }

            _count++;
            variants.Add(method, isVararg);
        }
    }

    public class MethodBindingInfo : MethodBaseBindingInfo<MethodInfo>
    {
        public bool isIndexer;

        public MethodBindingInfo(bool isIndexer, bool bStatic, string bindName, string regName)
        {
            this.isIndexer = isIndexer;
            this.name = (bStatic ? "BindStatic_" : "Bind_") + bindName;
            this.regName = regName;
        }
    }

    public class OperatorBindingInfo : MethodBaseBindingInfo<MethodInfo>
    {
        public int length; // 参数数
        public string bindName;
        public MethodInfo methodInfo;
        public bool isExtension;

        public OperatorBindingInfo(MethodInfo methodInfo, bool isExtension, bool bStatic, string bindName, string regName, int length)
        {
            this.methodInfo = methodInfo;
            this.isExtension = isExtension;
            this.length = length;
            this.bindName = bindName;
            this.regName = regName;
            this.name = (bStatic ? "BindStatic_" : "Bind_") + bindName;

            this.Add(methodInfo, isExtension); //NOTE: 旧代码, 待更替
        }
    }

    public class ConstructorBindingInfo : MethodBaseBindingInfo<ConstructorInfo>
    {
        public Type decalringType;

        // public 构造是否可用
        public bool available
        {
            get
            {
                if (decalringType.IsValueType && !decalringType.IsPrimitive && !decalringType.IsAbstract)
                {
                    return true; // default constructor for struct
                }

                return variants.Count > 0;
            }
        }

        public ConstructorBindingInfo(Type decalringType)
        {
            this.decalringType = decalringType;
            this.name = "BindConstructor";
            this.regName = "constructor";
        }
    }

    public struct PropertyBindingPair
    {
        public string getterName; // 绑定代码名
        public string setterName;

        public bool IsValid()
        {
            return this.getterName != null || this.setterName != null;
        }
    }

    public class PropertyBindingInfo
    {
        public PropertyBindingPair staticPair;

        public PropertyBindingPair instancePair;

        // public string getterName; // 绑定代码名
        // public string setterName;
        public string regName; // js 注册名
        public PropertyInfo propertyInfo;

        public PropertyBindingInfo(PropertyInfo propertyInfo)
        {
            this.propertyInfo = propertyInfo;
            if (propertyInfo.CanRead && propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
            {
                if (propertyInfo.GetMethod.IsStatic)
                {
                    staticPair.getterName = "BindStaticRead_" + propertyInfo.Name;
                }
                else
                {
                    instancePair.getterName = "BindRead_" + propertyInfo.Name;
                }
            }

            if (propertyInfo.CanWrite && propertyInfo.SetMethod != null && propertyInfo.SetMethod.IsPublic)
            {
                if (propertyInfo.SetMethod.IsStatic)
                {
                    staticPair.setterName = "BindStaticWrite_" + propertyInfo.Name;
                }
                else
                {
                    instancePair.setterName = "BindWrite_" + propertyInfo.Name;
                }
            }

            this.regName = TypeBindingInfo.GetNamingAttribute(propertyInfo);
        }
    }

    public class FieldBindingInfo
    {
        public string getterName = null; // 绑定代码名
        public string setterName = null;
        public string regName = null; // js 注册名

        public FieldInfo fieldInfo;

        public string constantValue;

        public bool isStatic
        {
            get { return fieldInfo.IsStatic; }
        }

        public FieldBindingInfo(FieldInfo fieldInfo)
        {
            do
            {
                if (fieldInfo.IsLiteral)
                {
                    try
                    {
                        var cv = fieldInfo.GetRawConstantValue();
                        var cvType = cv.GetType();
                        if (cvType == typeof(string))
                        {
                            constantValue = $"\"{cv}\"";
                            break;
                        }

                        if (cvType == typeof(int)
                            || cvType == typeof(uint)
                            || cvType == typeof(byte)
                            || cvType == typeof(sbyte)
                            || cvType == typeof(short)
                            || cvType == typeof(ushort)
                            || cvType == typeof(bool))
                        {
                            constantValue = $"{cv}";
                            break;
                        }

                        if (cvType == typeof(float))
                        {
                            var fcv = (float)cv;
                            if (!float.IsInfinity(fcv)
                                && !float.IsNaN(fcv))
                            {
                                constantValue = $"{cv}";
                                break;
                            }
                        }

                        // if (cvType.IsPrimitive && cvType.IsValueType)
                        // {
                        //     constantValue = $"{cv}";
                        //     break;
                        // }
                    }
                    catch (Exception)
                    {
                    }
                }

                if (fieldInfo.IsStatic)
                {
                    this.getterName = "BindStaticRead_" + fieldInfo.Name;
                    if (!fieldInfo.IsInitOnly && !fieldInfo.IsLiteral)
                    {
                        this.setterName = "BindStaticWrite_" + fieldInfo.Name;
                    }
                }
                else
                {
                    this.getterName = "BindRead_" + fieldInfo.Name;
                    if (!fieldInfo.IsInitOnly && !fieldInfo.IsLiteral)
                    {
                        this.setterName = "BindWrite_" + fieldInfo.Name;
                    }
                }
            } while (false);

            this.regName = TypeBindingInfo.GetNamingAttribute(fieldInfo);
            this.fieldInfo = fieldInfo;
        }
    }

    public class EventBindingInfo
    {
        public string adderName = null; // 绑定代码名
        public string removerName = null;
        public string proxyName = null; // 非静态event需要一个property.getter在实例上创建一个event object实例
        public string regName = null; // js 注册名

        public Type declaringType;
        public EventInfo eventInfo;

        public bool isStatic
        {
            get { return eventInfo.GetAddMethod().IsStatic; }
        }

        public EventBindingInfo(Type declaringType, EventInfo eventInfo)
        {
            this.declaringType = declaringType;
            this.eventInfo = eventInfo;
            do
            {
                if (this.isStatic)
                {
                    this.adderName = "BindStaticAdd_" + eventInfo.Name;
                    this.removerName = "BindStaticRemove_" + eventInfo.Name;
                }
                else
                {
                    this.adderName = "BindAdd_" + eventInfo.Name;
                    this.removerName = "BindRemove_" + eventInfo.Name;
                    this.proxyName = "BindProxy_" + eventInfo.Name;
                }
            } while (false);

            this.regName = TypeBindingInfo.GetNamingAttribute(eventInfo);
        }
    }

    public class TypeBindingInfo
    {
        public BindingManager bindingManager;
        public Type type;
        public TypeTransform transform;

        public Type super
        {
            get { return type.BaseType; }
        } // 父类类型

        public bool omit
        {
            get { return type.IsDefined(typeof(JSOmitAttribute)); }
        }

        public string name; // 绑定代码名

        public string jsNamespace; // js 命名空间

        public string jsName; // js注册名

        public List<OperatorBindingInfo> operators = new List<OperatorBindingInfo>();

        public Dictionary<string, MethodBindingInfo> methods = new Dictionary<string, MethodBindingInfo>();
        public Dictionary<string, MethodBindingInfo> staticMethods = new Dictionary<string, MethodBindingInfo>();

        public Dictionary<string, PropertyBindingInfo> properties = new Dictionary<string, PropertyBindingInfo>();
        public Dictionary<string, FieldBindingInfo> fields = new Dictionary<string, FieldBindingInfo>();
        public Dictionary<string, EventBindingInfo> events = new Dictionary<string, EventBindingInfo>();
        public ConstructorBindingInfo constructors;

        public Assembly Assembly
        {
            get { return type.Assembly; }
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

        public static string GetNamingAttribute(MemberInfo info)
        {
            var naming = info.GetCustomAttribute(typeof(JSNamingAttribute), false) as JSNamingAttribute;
            if (naming != null && !string.IsNullOrEmpty(naming.name))
            {
                return naming.name;
            }

            return info.Name;
        }

        public TypeBindingInfo(BindingManager bindingManager, Type type)
        {
            this.bindingManager = bindingManager;
            this.type = type;
            this.transform = bindingManager.GetTypeTransform(type);
            var naming = this.transform?.GetTypeNaming() ?? GetNamingAttribute(type);
            var indexOfTypeName = naming.LastIndexOf('.');
            if (indexOfTypeName >= 0) // 内部类
            {
                this.jsNamespace = naming.Substring(0, indexOfTypeName);
                this.jsName = naming.Substring(indexOfTypeName + 1);
            }
            else
            {
                if (type.DeclaringType != null)
                {
                    this.jsNamespace = $"{type.Namespace}.{type.DeclaringType.Name}";
                }
                else
                {
                    this.jsNamespace = type.Namespace;
                }

                if (type.IsGenericType)
                {
                    this.jsName = naming.Substring(0, naming.IndexOf('`'));
                    foreach (var gp in type.GetGenericArguments())
                    {
                        this.jsName += "_" + gp.Name;
                    }
                }
                else
                {
                    this.jsName = naming;
                }
            }

            this.name = bindingManager.prefs.typeBindingPrefix + (this.jsNamespace + "_" + this.jsName).Replace('.', '_').Replace('+', '_');
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
                fields.Add(fieldInfo.Name, new FieldBindingInfo(fieldInfo));
                bindingManager.Info("[AddField] {0}.{1}", type, fieldInfo.Name);
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
                properties.Add(propInfo.Name, new PropertyBindingInfo(propInfo));
                bindingManager.Info("[AddProperty] {0}.{1}", type, propInfo.Name);
            }
            catch (Exception exception)
            {
                bindingManager.Error("AddProperty failed {0} @ {1}: {2}", propInfo, type, exception.Message);
            }
        }

        public void AddMethod(MethodInfo methodInfo)
        {
            AddMethod(methodInfo, false, null);
        }

        public static bool IsSupportedOperators(MethodInfo methodInfo)
        {
            return methodInfo.IsSpecialName && methodInfo.Name.StartsWith("op_");
        }

        public void AddMethod(MethodInfo methodInfo, bool isIndexer, string renameRegName)
        {
            if (this.transform != null)
            {
                if (this.transform.IsBlocked(methodInfo))
                {
                    bindingManager.Info("skip blocked method: {0}", methodInfo.Name);
                    return;
                }
            }

            var isExtension = methodInfo.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute));
            var isStatic = methodInfo.IsStatic && !isExtension;
            if (IsSupportedOperators(methodInfo))
            {
                var methodName = TypeBindingInfo.GetNamingAttribute(methodInfo);
                var parameters = methodInfo.GetParameters();
                var declaringType = methodInfo.DeclaringType;
                switch (methodName)
                {
                    case "op_Addition":
                        if (parameters.Length == 2)
                        {
                            if (parameters[0].ParameterType == declaringType && parameters[1].ParameterType == declaringType)
                            {
                                var bindingInfo = new OperatorBindingInfo(methodInfo, isExtension, isStatic, methodName, "+", 2);
                                operators.Add(bindingInfo);
                            }
                        }
                        break;
                    case "op_Subtraction":
                        if (parameters.Length == 2)
                        {
                            if (parameters[0].ParameterType == declaringType && parameters[1].ParameterType == declaringType)
                            {
                                var bindingInfo = new OperatorBindingInfo(methodInfo, isExtension, isStatic, methodName, "-", 2);
                                operators.Add(bindingInfo);
                            }
                        }
                        break;
                    case "op_Equality":
                        if (parameters.Length == 2)
                        {
                            if (parameters[0].ParameterType == declaringType && parameters[1].ParameterType == declaringType)
                            {
                                var bindingInfo = new OperatorBindingInfo(methodInfo, isExtension, isStatic, methodName, "==", 2);
                                operators.Add(bindingInfo);
                            }
                        }
                        break;
                    case "op_Multiply":
                        //TODO: left/right 处理
                        // if (parameters.Length == 2)
                        // {
                        //     var op0 = bindingManager.GetExportedType(parameters[0].ParameterType);
                        //     var op1 = bindingManager.GetExportedType(parameters[0].ParameterType);
                        //     if (op0 == null && op1 == null)
                        //     {
                        //         return;
                        //     }
                        //     var bindingName = methodName + "_" + op0.name + "_" + op1.name;
                        //     var bindingInfo = new OperatorBindingInfo(methodInfo, isExtension, isStatic, bindingName, "*", 2);
                        //     operators.Add(bindingInfo);
                        // }
                        break;
                    default:
                        bindingManager.Info("skip unsupported operator method: {0}", methodInfo.Name);
                        return;
                }
            }
            else
            {
                var group = isStatic ? staticMethods : methods;
                MethodBindingInfo overrides;
                var methodName = TypeBindingInfo.GetNamingAttribute(methodInfo);
                if (!group.TryGetValue(methodName, out overrides))
                {
                    overrides = new MethodBindingInfo(isIndexer, isStatic, methodName, renameRegName ?? methodName);
                    group.Add(methodName, overrides);
                }
                overrides.Add(methodInfo, isExtension);
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
            if (this.transform != null)
            {
                if (this.transform.IsBlocked(constructorInfo))
                {
                    bindingManager.Info("skip blocked constructor: {0}", constructorInfo.Name);
                    return;
                }
            }

            constructors.Add(constructorInfo, false);
            CollectDelegate(constructorInfo);
            this.bindingManager.Info("[AddConstructor] {0}.{1}", type, constructorInfo);
        }

        public bool IsExtensionMethod(MethodInfo methodInfo)
        {
            return methodInfo.IsDefined(typeof(ExtensionAttribute), false);
        }

        // 收集所有 字段,属性,方法
        public void Collect()
        {
            var bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            var fields = type.GetFields(bindingFlags);
            foreach (var field in fields)
            {
                if (field.IsSpecialName)
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

                if (transform != null && transform.IsMemberBlocked(field.Name))
                {
                    bindingManager.Info("skip blocked field: {0}", field.Name);
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

                if (transform != null && transform.IsMemberBlocked(evt.Name))
                {
                    bindingManager.Info("skip blocked event: {0}", evt.Name);
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

                if (transform != null && transform.IsMemberBlocked(property.Name))
                {
                    bindingManager.Info("skip blocked property: {0}", property.Name);
                    continue;
                }

                //NOTE: 索引访问
                if (property.Name == "Item")
                {
                    if (property.CanRead && property.GetMethod != null)
                    {
                        if (BindingManager.IsUnsupported(property.GetMethod))
                        {
                            bindingManager.Info("skip unsupported get-method: {0}", property.GetMethod);
                            continue;
                        }

                        AddMethod(property.GetMethod, true, "$GetValue");
                    }

                    if (property.CanWrite && property.SetMethod != null)
                    {
                        if (BindingManager.IsUnsupported(property.SetMethod))
                        {
                            bindingManager.Info("skip unsupported set-method: {0}", property.SetMethod);
                            continue;
                        }

                        AddMethod(property.SetMethod, true, "$SetValue");
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
                        bindingManager.Info("skip pointer constructor: {0}", constructor);
                        continue;
                    }

                    AddConstructor(constructor);
                }
            }

            var methods = type.GetMethods(bindingFlags);
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

                if (transform != null && transform.IsMemberBlocked(method.Name))
                {
                    bindingManager.Info("skip blocked method: {0}", method.Name);
                    continue;
                }

                // if (IsPropertyMethod(method))
                // {
                //     continue;
                // }

                if (IsExtensionMethod(method))
                {
                    var targetType = method.GetParameters()[0].ParameterType;
                    var targetInfo = bindingManager.GetExportedType(targetType);
                    if (targetInfo != null)
                    {
                        targetInfo.AddMethod(method);
                        continue;
                    }

                    // else fallthrough (as normal static method)
                }

                AddMethod(method);
            }
        }
    }
}