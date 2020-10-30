using System;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    public class TSTypeNaming
    {
        public bool topLevel => string.IsNullOrEmpty(jsModule) && string.IsNullOrEmpty(jsNamespace);

        /// <summary>
        /// js 模块名
        /// </summary>
        public readonly string jsModule;

        /// <summary>
        /// js 命名空间
        /// </summary>
        public readonly string jsNamespace;

        ///<summary>
        /// 不带泛型部分的js注册名
        ///</summary>
        public readonly string jsPureName;

        /// <summary>
        /// js注册名 (带平面化的泛型部分)
        /// </summary>
        public readonly string jsName;

        /// <summary>
        /// js 模块中的顶层访问名 (内部类的顶层访问名为最外层类的类名, 否则就是类名本身 jsPureName)
        /// </summary>
        public readonly string jsModuleAccess;

        public readonly string jsLocalName;

        /// <summary>
        /// 当前类型的完整JS类型名 (如果是具化泛型类, 则为扁平化的具化泛型类名称)
        /// </summary>
        public readonly string jsFullName;

        public TSTypeNaming(BindingManager bindingManager, Type type, TypeTransform typeTransform)
        {
            var naming = typeTransform.GetTypeNaming() ?? bindingManager.GetNamingAttribute(type);
            var indexOfTypeName = naming.LastIndexOf('.');

            if (indexOfTypeName >= 0)
            {
                // 指定的命名中已经携带了"."
                var indexOfInnerTypeName = naming.IndexOf('+');
                if (indexOfInnerTypeName >= 0)
                {
                    this.jsModule = naming.Substring(0, indexOfInnerTypeName);
                    var rightName = naming.Substring(indexOfInnerTypeName + 1);
                    var lastIndexOfInnerTypeName = rightName.LastIndexOf('+');
                    if (lastIndexOfInnerTypeName >= 0)
                    {
                        this.jsNamespace = rightName.Substring(0, lastIndexOfInnerTypeName);
                        this.jsName = rightName.Substring(lastIndexOfInnerTypeName + 1);
                    }
                    else
                    {
                        this.jsNamespace = "";
                        this.jsName = rightName;
                    }
                }
                else
                {
                    this.jsModule = naming.Substring(0, indexOfTypeName);
                    this.jsNamespace = "";
                    this.jsName = naming.Substring(indexOfTypeName + 1);
                }

                this.jsPureName = this.jsName;
            }
            else
            {
                this.jsModule = type.Namespace ?? "";
                this.jsNamespace = "";

                // 处理内部类层级
                var declaringType = type.DeclaringType;
                while (declaringType != null)
                {
                    this.jsNamespace = string.IsNullOrEmpty(this.jsNamespace) ? declaringType.Name : $"{declaringType.Name}.{this.jsNamespace}";
                    declaringType = declaringType.DeclaringType;
                }

                if (type.IsGenericType)
                {
                    this.jsName = naming.Contains("`") ? naming.Substring(0, naming.IndexOf('`')) : naming;
                    this.jsPureName = this.jsName;

                    if (type.IsGenericTypeDefinition)
                    {
                        if (!naming.Contains("<"))
                        {
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
                    }
                    else
                    {
                        foreach (var gp in type.GetGenericArguments())
                        {
                            this.jsName += "_" + gp.Name;
                        }
                    }
                }
                else
                {
                    this.jsName = naming;
                    this.jsPureName = this.jsName;
                }
            }

            if (string.IsNullOrEmpty(this.jsNamespace))
            {
                this.jsModuleAccess = this.jsPureName;
                this.jsLocalName = "";
            }
            else
            {
                var i = this.jsNamespace.IndexOf('.');
                this.jsModuleAccess = i < 0 ? this.jsNamespace : this.jsNamespace.Substring(0, i);
                this.jsLocalName = CodeGenUtils.Concat(".", i < 0 ? "" : this.jsNamespace.Substring(i + 1), this.jsName);
            }

            this.jsFullName = CodeGenUtils.Concat(".", jsModule, jsNamespace, jsName);
        }
    }
}
