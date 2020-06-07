using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    public class ClassCodeGen : TypeCodeGen
    {
        public ClassCodeGen(CodeGenerator cg, TypeBindingInfo bindingInfo)
        : base(cg, bindingInfo)
        {
            this.cg.AppendJSDoc(this.bindingInfo.type);
            var transform = this.bindingInfo.transform;
            var prefix = this.bindingInfo.jsNamespace != null ? "" : "declare ";
            var super = this.cg.bindingManager.GetTSSuperName(this.bindingInfo);
            var interfaces = this.cg.bindingManager.GetTSInterfacesName(this.bindingInfo);
            var extends = string.IsNullOrEmpty(super) ? "" : $" extends {super}";
            var implements = string.IsNullOrEmpty(interfaces) ? "" : $" implements {interfaces}";
            var regName = this.bindingInfo.jsName;
            if (bindingInfo.type.IsAbstract)
            {
                prefix += "abstract ";
            }
            this.cg.tsDeclare.AppendLine($"{prefix}class {regName}{extends}{implements} {{");
            this.cg.tsDeclare.AddTabLevel();

            // 生成函数体
            // 构造函数
            if (this.bindingInfo.constructors.available)
            {
                using (new PInvokeGuardCodeGen(cg, typeof(Native.JSCFunctionMagic)))
                {
                    using (new BindingConstructorDeclareCodeGen(cg, this.bindingInfo.constructors.name))
                    {
                        using (new TryCatchGuradCodeGen(cg))
                        {
                            using (new ConstructorCodeGen(cg, this.bindingInfo))
                            {
                            }
                        }
                    }
                }
            }
            // 非静态成员方法
            foreach (var kv in this.bindingInfo.methods)
            {
                var methodBindingInfo = kv.Value;
                if (transform == null || !transform.IsRedirectedMethod(methodBindingInfo.regName))
                {
                    using (new PInvokeGuardCodeGen(cg))
                    {
                        using (new BindingFuncDeclareCodeGen(cg, methodBindingInfo.name))
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
                using (new TSMethodCodeGen(cg, methodBindingInfo))
                {
                }
            }
            //TODO: C# 抽象类可以不提供方法实现, d.ts 需要补充声明
            // if (this.bindingInfo.type.IsAbstract && !this.bindingInfo.type.IsInterface)
            // {
            // }
            // 静态成员方法
            foreach (var kv in this.bindingInfo.staticMethods)
            {
                var methodBindingInfo = kv.Value;
                if (transform == null || !transform.IsRedirectedMethod(methodBindingInfo.regName))
                {
                    using (new PInvokeGuardCodeGen(cg))
                    {
                        using (new BindingFuncDeclareCodeGen(cg, methodBindingInfo.name))
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
                using (new TSMethodCodeGen(cg, methodBindingInfo))
                {
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
            foreach (var kv in this.bindingInfo.properties)
            {
                var propertyBindingInfo = kv.Value;
                // 静态
                if (propertyBindingInfo.staticPair.IsValid())
                {
                    // 可读属性
                    if (propertyBindingInfo.staticPair.getterName != null)
                    {
                        using (new PInvokeGuardCodeGen(cg))
                        {
                            using (new BindingFuncDeclareCodeGen(cg, propertyBindingInfo.staticPair.getterName))
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
                        using (new PInvokeGuardCodeGen(cg))
                        {
                            using (new BindingFuncDeclareCodeGen(cg, propertyBindingInfo.staticPair.setterName))
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
                        using (new PInvokeGuardCodeGen(cg))
                        {
                            using (new BindingFuncDeclareCodeGen(cg, propertyBindingInfo.instancePair.getterName))
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
                        using (new PInvokeGuardCodeGen(cg))
                        {
                            using (new BindingFuncDeclareCodeGen(cg, propertyBindingInfo.instancePair.setterName))
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
            foreach (var kv in this.bindingInfo.fields)
            {
                var fieldBindingInfo = kv.Value;
                if (fieldBindingInfo.getterName != null)
                {
                    using (new PInvokeGuardCodeGen(cg))
                    {
                        using (new BindingFuncDeclareCodeGen(cg, fieldBindingInfo.getterName))
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
                // 可写字段 
                if (fieldBindingInfo.setterName != null)
                {
                    using (new PInvokeGuardCodeGen(cg))
                    {
                        using (new BindingFuncDeclareCodeGen(cg, fieldBindingInfo.setterName))
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
            // 所有事件 (当做field相似处理)
            foreach (var kv in this.bindingInfo.events)
            {
                var eventBindingInfo = kv.Value;
                using (new PInvokeGuardCodeGen(cg))
                {
                    using (new BindingFuncDeclareCodeGen(cg, eventBindingInfo.adderName))
                    {
                        using (new TryCatchGuradCodeGen(cg))
                        {
                            using (new EventAdderCodeGen(cg, eventBindingInfo))
                            {
                            }
                        }
                    }
                }
                using (new PInvokeGuardCodeGen(cg))
                {
                    using (new BindingFuncDeclareCodeGen(cg, eventBindingInfo.removerName))
                    {
                        using (new TryCatchGuradCodeGen(cg))
                        {
                            using (new EventRemoverCodeGen(cg, eventBindingInfo))
                            {
                            }
                        }
                    }
                }
                if (!eventBindingInfo.isStatic)
                {
                    using (new PInvokeGuardCodeGen(cg))
                    {
                        using (new BindingFuncDeclareCodeGen(cg, eventBindingInfo.proxyName))
                        {
                            using (new TryCatchGuradCodeGen(cg))
                            {
                                using (new EventProxyCodeGen(cg, eventBindingInfo))
                                {
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Dispose()
        {
            using (new PreservedCodeGen(cg))
            {
                using (new RegFuncCodeGen(cg))
                {
                    using (new RegFuncNamespaceCodeGen(cg, bindingInfo))
                    {
                        var constructor = bindingInfo.constructors.available ? bindingInfo.constructors.name : "JSApi.class_private_ctor";
                        if (!bindingInfo.constructors.available && !bindingInfo.type.IsAbstract)
                        {
                            if (bindingInfo.type.IsSubclassOf(typeof(Component)))
                            {
                                // 因为 ts 泛型约束需要 new() 形式, 所以在定义中产生一个 public 定义
                                // 例如: GetComponent<T extends Component>(type: { new(): T }): T
                                cg.tsDeclare.AppendLine("/*protected*/ constructor()");
                            }
                            else
                            {
                                cg.tsDeclare.AppendLine("protected constructor()");
                            }
                        }
                        cg.cs.AppendLine("var cls = ns.CreateClass(\"{0}\", typeof({1}), {2});",
                            bindingInfo.jsName,
                            this.cg.bindingManager.GetCSTypeFullName(bindingInfo.type),
                            constructor);
                        foreach (var kv in bindingInfo.methods)
                        {
                            var regName = kv.Value.regName;
                            var funcName = kv.Value.name;
                            string redirect;
                            if (this.bindingInfo.transform != null && this.bindingInfo.transform.TryRedirectMethod(regName, out redirect))
                            {
                                funcName = redirect;
                            }

                            cg.cs.AppendLine("cls.AddMethod(false, \"{0}\", {1});", regName, funcName);
                        }
                        foreach (var kv in bindingInfo.staticMethods)
                        {
                            var regName = kv.Value.regName;
                            var funcName = kv.Value.name;
                            string redirect;
                            if (this.bindingInfo.transform != null && this.bindingInfo.transform.TryRedirectMethod(regName, out redirect))
                            {
                                funcName = redirect;
                            }
                            cg.cs.AppendLine("cls.AddMethod(true, \"{0}\", {1});", regName, funcName);
                        }
                        foreach (var kv in bindingInfo.properties)
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
                        foreach (var kv in bindingInfo.fields)
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
                        foreach (var kv in bindingInfo.events)
                        {
                            var eventBindingInfo = kv.Value;
                            var bStatic = eventBindingInfo.isStatic;
                            //NOTE: 静态事件在绑定过程直接定义， 非静态事件推迟到构造时直接赋值创建
                            var tsFieldVar = BindingManager.GetTSVariable(eventBindingInfo.regName);
                            var tsFieldType = this.cg.bindingManager.GetTSTypeFullName(eventBindingInfo.eventInfo.EventHandlerType);
                            var tsFieldPrefix = "";
                            if (bStatic)
                            {
                                tsFieldPrefix += "static ";
                                cg.cs.AppendLine($"cls.AddEvent(true, \"{tsFieldVar}\", {eventBindingInfo.adderName}, {eventBindingInfo.removerName});");
                            }
                            else
                            {
                                cg.cs.AppendLine($"cls.AddProperty(false, \"{tsFieldVar}\", {eventBindingInfo.proxyName}, null);");
                            }
                            cg.tsDeclare.AppendLine($"{tsFieldPrefix}{tsFieldVar}: DuktapeJS.event<{tsFieldType}>");
                        }
                        cg.cs.AppendLine("cls.Close();");
                    }
                }
                base.Dispose();
            }

            this.cg.tsDeclare.DecTabLevel();
            this.cg.tsDeclare.AppendLine("}");
        }
    }
}
