using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using QuickJS.Native;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public class ClassCodeGen : TypeCodeGen
    {
        public ClassCodeGen(CodeGenerator cg, TypeBindingInfo typeBindingInfo)
        : base(cg, typeBindingInfo)
        {
            this.cg.AppendJSDoc(this.typeBindingInfo.type);
            var transform = this.typeBindingInfo.transform;
            var prefix = string.IsNullOrEmpty(this.typeBindingInfo.jsNamespace) ? "declare " : "";
            var super = this.cg.bindingManager.GetTSSuperName(this.typeBindingInfo);
            var interfaces = this.cg.bindingManager.GetTSInterfacesName(this.typeBindingInfo.type);
            var implements = "";
            var jsClassName = this.typeBindingInfo.jsName;
            var jsClassType = "";

            if (typeBindingInfo.isEditorRuntime)
            {
                this.cg.tsDeclare.AppendLine("@jsb.EditorRuntime");
            }


            if (typeBindingInfo.type.IsInterface)
            {
                jsClassType = "interface";

                if (string.IsNullOrEmpty(interfaces))
                {
                    if (!string.IsNullOrEmpty(super))
                    {
                        implements += $" extends {super}"; // something wrong 
                    }
                }
                else
                {
                    implements += $" extends {interfaces}";

                    if (!string.IsNullOrEmpty(super))
                    {
                        implements += $", {super}"; // something wrong 
                    }
                }
            }
            else
            {
                jsClassType = typeBindingInfo.type.IsAbstract ? "abstract class" : "class";

                if (!string.IsNullOrEmpty(super))
                {
                    implements += $" extends {super}";
                }

                if (!string.IsNullOrEmpty(interfaces))
                {
                    implements += $" implements {interfaces}";
                }
            }

            this.cg.tsDeclare.AppendLine($"{prefix}{jsClassType} {jsClassName}{implements} {{");
            this.cg.tsDeclare.AddTabLevel();

            // 生成函数体
            // 构造函数
            if (this.typeBindingInfo.constructors.available)
            {
                using (new PInvokeGuardCodeGen(cg, typeof(Native.JSCFunctionMagic)))
                {
                    using (new BindingConstructorDeclareCodeGen(cg, this.typeBindingInfo.constructors.csBindName))
                    {
                        using (new TryCatchGuradCodeGen(cg))
                        {
                            using (new ConstructorCodeGen(cg, this.typeBindingInfo))
                            {
                            }
                        }
                    }
                }
            }

            // 非静态成员方法
            foreach (var kv in this.typeBindingInfo.methods)
            {
                var methodBindingInfo = kv.Value;

                if (transform == null || !transform.IsRedirectedMethod(methodBindingInfo.jsName))
                {
                    using (new PInvokeGuardCodeGen(cg))
                    {
                        using (new BindingFuncDeclareCodeGen(cg, methodBindingInfo.csBindName))
                        {
                            using (new TryCatchGuradCodeGen(cg))
                            {
                                using (new MethodCodeGen(cg, methodBindingInfo))
                                {
                                }
                            }
                        }
                    }
                }

                using (new TSMethodCodeGen(cg, this.typeBindingInfo, methodBindingInfo))
                {
                }
            }

            //TODO: C# 抽象类可以不提供方法实现, d.ts 需要补充声明
            // if (this.bindingInfo.type.IsAbstract && !this.bindingInfo.type.IsInterface)
            // {
            // }
            // 静态成员方法
            if (!typeBindingInfo.type.IsGenericTypeDefinition)
            {
                foreach (var kv in this.typeBindingInfo.staticMethods)
                {
                    var methodBindingInfo = kv.Value;
                    if (transform == null || !transform.IsRedirectedMethod(methodBindingInfo.jsName))
                    {
                        if (methodBindingInfo._cfunc != null)
                        {
                            continue;
                        }

                        using (new PInvokeGuardCodeGen(cg))
                        {
                            using (new BindingFuncDeclareCodeGen(cg, methodBindingInfo.csBindName))
                            {
                                using (new TryCatchGuradCodeGen(cg))
                                {
                                    using (new MethodCodeGen(cg, methodBindingInfo))
                                    {
                                    }
                                }
                            }
                        }
                    }

                    using (new TSMethodCodeGen(cg, typeBindingInfo, methodBindingInfo))
                    {
                    }
                }
            }

            if (!typeBindingInfo.type.IsGenericTypeDefinition)
            {
                foreach (var operatorBindingInfo in this.typeBindingInfo.operators)
                {
                    if (transform == null || !transform.IsRedirectedMethod(operatorBindingInfo.jsName))
                    {
                        using (new PInvokeGuardCodeGen(cg))
                        {
                            using (new BindingFuncDeclareCodeGen(cg, operatorBindingInfo.csBindName))
                            {
                                using (new TryCatchGuradCodeGen(cg))
                                {
                                    using (new OperatorCodeGen(cg, operatorBindingInfo))
                                    {
                                    }
                                }
                            }
                        }
                    }

                    using (new TSOperatorCodeGen(cg, typeBindingInfo, operatorBindingInfo))
                    {
                    }
                }
            }

            // 所有附加方法
            if (transform != null)
            {
                transform.ForEachAdditionalTSMethodDeclaration(decl =>
                {
                    this.cg.tsDeclare.AppendLine(decl);
                });
            }

            // 所有属性
            foreach (var kv in this.typeBindingInfo.properties)
            {
                var propertyBindingInfo = kv.Value;

                // 静态
                if (propertyBindingInfo.staticPair.IsValid())
                {
                    // 可读属性
                    if (propertyBindingInfo.staticPair.getterName != null)
                    {
                        using (new PInvokeGuardCodeGen(cg, typeof(JSGetterCFunction)))
                        {
                            using (new BindingGetterFuncDeclareCodeGen(cg, propertyBindingInfo.staticPair.getterName))
                            {
                                using (new TryCatchGuradCodeGen(cg))
                                {
                                    using (new PropertyGetterCodeGen(cg, propertyBindingInfo))
                                    {
                                    }
                                }
                            }
                        }
                    }

                    // 可写属性
                    if (propertyBindingInfo.staticPair.setterName != null)
                    {
                        using (new PInvokeGuardCodeGen(cg, typeof(JSSetterCFunction)))
                        {
                            using (new BindingSetterFuncDeclareCodeGen(cg, propertyBindingInfo.staticPair.setterName))
                            {
                                using (new TryCatchGuradCodeGen(cg))
                                {
                                    using (new PropertySetterCodeGen(cg, propertyBindingInfo))
                                    {
                                    }
                                }
                            }
                        }
                    }
                }

                // 非静态
                if (propertyBindingInfo.instancePair.IsValid())
                {
                    // 可读属性
                    if (propertyBindingInfo.instancePair.getterName != null)
                    {
                        using (new PInvokeGuardCodeGen(cg, typeof(JSGetterCFunction)))
                        {
                            using (new BindingGetterFuncDeclareCodeGen(cg, propertyBindingInfo.instancePair.getterName))
                            {
                                using (new TryCatchGuradCodeGen(cg))
                                {
                                    using (new PropertyGetterCodeGen(cg, propertyBindingInfo))
                                    {
                                    }
                                }
                            }
                        }
                    }
                    // 可写属性
                    if (propertyBindingInfo.instancePair.setterName != null)
                    {
                        using (new PInvokeGuardCodeGen(cg, typeof(JSSetterCFunction)))
                        {
                            using (new BindingSetterFuncDeclareCodeGen(cg, propertyBindingInfo.instancePair.setterName))
                            {
                                using (new TryCatchGuradCodeGen(cg))
                                {
                                    using (new PropertySetterCodeGen(cg, propertyBindingInfo))
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 所有字段
            foreach (var kv in this.typeBindingInfo.fields)
            {
                var fieldBindingInfo = kv.Value;

                // 可读
                if (fieldBindingInfo.getterName != null)
                {
                    using (new PInvokeGuardCodeGen(cg, typeof(JSGetterCFunction)))
                    {
                        using (new BindingGetterFuncDeclareCodeGen(cg, fieldBindingInfo.getterName))
                        {
                            using (new TryCatchGuradCodeGen(cg))
                            {
                                using (new FieldGetterCodeGen(cg, fieldBindingInfo))
                                {
                                }
                            }
                        }
                    }
                }

                // 可写 
                if (fieldBindingInfo.setterName != null)
                {
                    using (new PInvokeGuardCodeGen(cg, typeof(JSSetterCFunction)))
                    {
                        using (new BindingSetterFuncDeclareCodeGen(cg, fieldBindingInfo.setterName))
                        {
                            using (new TryCatchGuradCodeGen(cg))
                            {
                                using (new FieldSetterCodeGen(cg, fieldBindingInfo))
                                {
                                }
                            }
                        }
                    }
                }
            }

            // 所有事件
            foreach (var kv in this.typeBindingInfo.events)
            {
                var eventBindingInfo = kv.Value;
                using (new PInvokeGuardCodeGen(cg))
                {
                    using (new BindingFuncDeclareCodeGen(cg, eventBindingInfo.name))
                    {
                        using (new TryCatchGuradCodeGen(cg))
                        {
                            using (new EventOperationCodeGen(cg, eventBindingInfo))
                            {
                            }
                        }
                    }
                }
            }

            // 所有委托 (Field/Property)
            foreach (var kv in this.typeBindingInfo.delegates)
            {
                var delegateBindingInfo = kv.Value;
                using (new PInvokeGuardCodeGen(cg))
                {
                    using (new BindingFuncDeclareCodeGen(cg, delegateBindingInfo.name))
                    {
                        using (new TryCatchGuradCodeGen(cg))
                        {
                            using (new DelegateOperationCodeGen(cg, delegateBindingInfo))
                            {
                            }
                        }
                    }
                }
            }
        }

        public override void Dispose()
        {
            using (new RegFuncCodeGen(cg))
            {
                using (new RegFuncNamespaceCodeGen(cg, typeBindingInfo))
                {
                    var constructor = typeBindingInfo.constructors.available ? typeBindingInfo.constructors.csBindName : "JSApi.class_private_ctor";

                    if (!typeBindingInfo.constructors.available && !typeBindingInfo.type.IsAbstract)
                    {
                        if (typeBindingInfo.type.IsSubclassOf(typeof(Component)))
                        {
                            // 因为 ts 泛型约束需要 new() 形式, 所以在定义中产生一个 public 定义
                            // 例如: GetComponent<T extends Component>(type: { new(): T }): T
                            cg.tsDeclare.AppendLine("/*protected*/ constructor()");
                        }
                        else
                        {
                            if (!typeBindingInfo.type.IsGenericTypeDefinition)
                            {
                                cg.tsDeclare.AppendLine("protected constructor()");
                            }
                        }
                    }

                    cg.cs.AppendLine("var cls = ns.CreateClass(\"{0}\", typeof({1}), {2});",
                        typeBindingInfo.jsName,
                        this.cg.bindingManager.GetCSTypeFullName(typeBindingInfo.type),
                        constructor);

                    // 运算符
                    foreach (var operatorBindingInfo in typeBindingInfo.operators)
                    {
                        var regName = operatorBindingInfo.jsName;
                        var funcName = operatorBindingInfo.csBindName;
                        var parameters = operatorBindingInfo.methodInfo.GetParameters();
                        var declaringType = operatorBindingInfo.methodInfo.DeclaringType;
                        string redirect;
                        if (this.typeBindingInfo.transform != null && this.typeBindingInfo.transform.TryRedirectMethod(regName, out redirect))
                        {
                            funcName = redirect;
                        }

                        do
                        {
                            if (parameters.Length == 2)
                            {
                                if (parameters[0].ParameterType != declaringType)
                                {
                                    var leftType = typeBindingInfo.bindingManager.GetCSTypeFullName(parameters[0].ParameterType);
                                    cg.cs.AppendLine("cls.AddLeftOperator(\"{0}\", {1}, {2}, typeof({3}));", regName, funcName, operatorBindingInfo.length, leftType);
                                    break;
                                }
                                else if (parameters[1].ParameterType != declaringType)
                                {
                                    var rightType = typeBindingInfo.bindingManager.GetCSTypeFullName(parameters[1].ParameterType);
                                    cg.cs.AppendLine("cls.AddRightOperator(\"{0}\", {1}, {2}, typeof({3}));", regName, funcName, operatorBindingInfo.length, rightType);
                                    break;
                                }
                            }

                            cg.cs.AppendLine("cls.AddSelfOperator(\"{0}\", {1}, {2});", regName, funcName, operatorBindingInfo.length);
                        } while (false);
                    }

                    // 非静态方法
                    foreach (var kv in typeBindingInfo.methods)
                    {
                        var regName = kv.Value.jsName;
                        var funcName = kv.Value.csBindName;
                        string redirect;
                        if (this.typeBindingInfo.transform != null && this.typeBindingInfo.transform.TryRedirectMethod(regName, out redirect))
                        {
                            funcName = redirect;
                        }

                        cg.cs.AppendLine("cls.AddMethod(false, \"{0}\", {1});", regName, funcName);
                    }

                    // 静态方法
                    foreach (var kv in typeBindingInfo.staticMethods)
                    {
                        var methodBindingInfo = kv.Value;
                        var regName = methodBindingInfo.jsName;

                        if (methodBindingInfo._cfunc != null)
                        {
                            var attr = (JSCFunctionAttribute)methodBindingInfo._cfunc.GetCustomAttribute(typeof(JSCFunctionAttribute));
                            var methodDeclType = this.cg.bindingManager.GetCSTypeFullName(methodBindingInfo._cfunc.DeclaringType);
                            var isStatic = attr.isStatic ? "true" : "false";
                            cg.cs.AppendLine("cls.AddRawMethod({0}, \"{1}\", {2}.{3});", isStatic, regName, methodDeclType, methodBindingInfo._cfunc.Name);
                            foreach (var defEntry in attr.difinitions)
                            {
                                string tsMethodRename;
                                var prefix = attr.isStatic ? "static" : "";
                                if (defEntry.StartsWith("("))
                                {
                                    if (this.cg.bindingManager.GetTSMethodRename(methodBindingInfo._cfunc, out tsMethodRename))
                                    {
                                        this.cg.tsDeclare.AppendLine($"{prefix}{tsMethodRename}{defEntry}");
                                    }
                                    else
                                    {
                                        this.cg.tsDeclare.AppendLine($"{prefix}{regName}{defEntry}");
                                    }
                                }
                                else
                                {
                                    this.cg.tsDeclare.AppendLine($"{prefix}{defEntry}");
                                }
                            }
                        }
                        else
                        {
                            var funcName = methodBindingInfo.csBindName;
                            string redirect;
                            if (this.typeBindingInfo.transform != null && this.typeBindingInfo.transform.TryRedirectMethod(regName, out redirect))
                            {
                                funcName = redirect;
                            }
                            cg.cs.AppendLine("cls.AddMethod(true, \"{0}\", {1});", regName, funcName);
                        }
                    }

                    // 属性
                    foreach (var kv in typeBindingInfo.properties)
                    {
                        var bindingInfo = kv.Value;
                        if (bindingInfo.staticPair.IsValid())
                        {
                            var tsPropertyVar = BindingManager.GetTSVariable(bindingInfo.regName);
                            cg.cs.AppendLine("cls.AddProperty(true, \"{0}\", {1}, {2});",
                                tsPropertyVar,
                                bindingInfo.staticPair.getterName != null ? bindingInfo.staticPair.getterName : "null",
                                bindingInfo.staticPair.setterName != null ? bindingInfo.staticPair.setterName : "null");

                            var tsPropertyPrefix = "static ";
                            if (bindingInfo.staticPair.setterName == null)
                            {
                                tsPropertyPrefix += "readonly ";
                            }
                            var tsPropertyType = this.cg.bindingManager.GetTSTypeFullName(bindingInfo.propertyInfo.PropertyType);
                            cg.AppendJSDoc(bindingInfo.propertyInfo);
                            cg.tsDeclare.AppendLine($"{tsPropertyPrefix}{tsPropertyVar}: {tsPropertyType}");
                        }

                        if (bindingInfo.instancePair.IsValid())
                        {
                            var tsPropertyVar = BindingManager.GetTSVariable(bindingInfo.regName);
                            cg.cs.AppendLine("cls.AddProperty(false, \"{0}\", {1}, {2});",
                                tsPropertyVar,
                                bindingInfo.instancePair.getterName != null ? bindingInfo.instancePair.getterName : "null",
                                bindingInfo.instancePair.setterName != null ? bindingInfo.instancePair.setterName : "null");

                            var tsPropertyPrefix = "";
                            if (bindingInfo.instancePair.setterName == null)
                            {
                                tsPropertyPrefix += "readonly ";
                            }
                            var tsPropertyType = this.cg.bindingManager.GetTSTypeFullName(bindingInfo.propertyInfo.PropertyType);
                            cg.AppendJSDoc(bindingInfo.propertyInfo);
                            cg.tsDeclare.AppendLine($"{tsPropertyPrefix}{tsPropertyVar}: {tsPropertyType}");
                        }
                    }

                    foreach (var kv in typeBindingInfo.fields)
                    {
                        var bindingInfo = kv.Value;
                        var bStatic = bindingInfo.isStatic;
                        var tsFieldVar = BindingManager.GetTSVariable(bindingInfo.regName);
                        if (bindingInfo.constantValue != null)
                        {
                            var cv = bindingInfo.constantValue;
                            cg.cs.AppendLine($"cls.AddConstValue(\"{tsFieldVar}\", {cv});");
                        }
                        else
                        {
                            cg.cs.AppendLine("cls.AddField({0}, \"{1}\", {2}, {3});",
                                bStatic ? "true" : "false",
                                tsFieldVar,
                                bindingInfo.getterName != null ? bindingInfo.getterName : "null",
                                bindingInfo.setterName != null ? bindingInfo.setterName : "null");
                        }
                        var tsFieldPrefix = bStatic ? "static " : "";
                        if (bindingInfo.setterName == null)
                        {
                            tsFieldPrefix += "readonly ";
                        }
                        var tsFieldType = this.cg.bindingManager.GetTSTypeFullName(bindingInfo.fieldInfo.FieldType);
                        cg.AppendJSDoc(bindingInfo.fieldInfo);
                        cg.tsDeclare.AppendLine($"{tsFieldPrefix}{tsFieldVar}: {tsFieldType}");
                    }

                    foreach (var kv in typeBindingInfo.events)
                    {
                        var eventBindingInfo = kv.Value;
                        var bStatic = eventBindingInfo.isStatic;
                        var tsFieldVar = BindingManager.GetTSVariable(eventBindingInfo.regName);
                        var tsFieldType = this.cg.bindingManager.GetTSTypeFullName(eventBindingInfo.eventInfo.EventHandlerType);
                        var tsFieldPrefix = "";
                        if (bStatic)
                        {
                            tsFieldPrefix += "static ";
                            cg.cs.AppendLine($"cls.AddMethod(true, \"{tsFieldVar}\", {eventBindingInfo.name});");
                        }
                        else
                        {
                            cg.cs.AppendLine($"cls.AddMethod(false, \"{tsFieldVar}\", {eventBindingInfo.name});");
                        }
                        // tsFieldPrefix += "readonly ";
                        // cg.tsDeclare.AppendLine($"{tsFieldPrefix}{tsFieldVar}: jsb.event<{tsFieldType}>");
                        cg.tsDeclare.AppendLine($"{tsFieldPrefix}{tsFieldVar}(op: \"add\" | \"remove\", fn: {tsFieldType}): void");
                    }

                    foreach (var kv in typeBindingInfo.delegates)
                    {
                        var delegateBindingInfo = kv.Value;
                        var bStatic = delegateBindingInfo.isStatic;
                        var tsFieldVar = BindingManager.GetTSVariable(delegateBindingInfo.regName);
                        var tsFieldType = this.cg.bindingManager.GetTSTypeFullName(delegateBindingInfo.delegateType);
                        var tsFieldPrefix = "";
                        if (bStatic)
                        {
                            tsFieldPrefix += "static ";
                            cg.cs.AppendLine($"cls.AddMethod(true, \"{tsFieldVar}\", {delegateBindingInfo.name});");
                        }
                        else
                        {
                            cg.cs.AppendLine($"cls.AddMethod(false, \"{tsFieldVar}\", {delegateBindingInfo.name});");
                        }
                        // tsFieldPrefix += "readonly ";
                        // cg.tsDeclare.AppendLine($"{tsFieldPrefix}{tsFieldVar}: jsb.event<{tsFieldType}>");

                        if (delegateBindingInfo.readable)
                        {
                            if (delegateBindingInfo.writable)
                            {
                                cg.tsDeclare.AppendLine($"{tsFieldPrefix}{tsFieldVar}(op: \"get\"): {tsFieldType}");
                                cg.tsDeclare.AppendLine($"{tsFieldPrefix}{tsFieldVar}(op: \"add\" | \"remove\" | \"set\", fn?: {tsFieldType}): void");
                                cg.tsDeclare.AppendLine($"{tsFieldPrefix}{tsFieldVar}(op: \"add\" | \"remove\" | \"set\" | \"get\", fn?: {tsFieldType}): {tsFieldType} | void");
                            }
                            else
                            {
                                cg.tsDeclare.AppendLine($"{tsFieldPrefix}{tsFieldVar}(op: \"get\"): {tsFieldType}");
                            }
                        }
                        else
                        {
                            cg.tsDeclare.AppendLine($"{tsFieldPrefix}{tsFieldVar}(op: \"set\", fn: {tsFieldType})");
                        }
                    }

                    cg.cs.AppendLine("cls.Close();");
                }
            }
            base.Dispose();

            this.cg.tsDeclare.DecTabLevel();
            this.cg.tsDeclare.AppendLine("}");
        }
    }
}

