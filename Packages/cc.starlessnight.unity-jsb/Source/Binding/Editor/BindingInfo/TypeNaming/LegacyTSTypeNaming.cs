using System;
using System.Collections.Generic;

namespace QuickJS.Binding
{
    public class LegacyTSTypeNaming : ITSTypeNaming
    {
        /// <summary>
        /// js module name
        /// </summary>
        public string jsModule { get; private set; }

        /// <summary>
        /// js 命名空间
        /// </summary>
        public string jsNamespace { get; private set; }

        /// <summary>
        /// splitted jsNamespace
        /// </summary>
        public string[] jsNamespaceSlice { get; private set; }

        ///<summary>
        /// the purified name for js (without the suffix for generic type args). 
        ///</summary>
        public string jsPureName { get; private set; }

        /// <summary>
        /// js注册名 (带平面化的泛型部分)
        /// </summary>
        public string jsName { get; private set; }

        public string jsNameNormalized { get; private set; }

        /// <summary>
        /// js 模块中的顶层访问名 (内部类的顶层访问名为最外层类的类名, 否则就是类名本身 jsPureName)
        /// </summary>
        public string jsModuleAccess { get; private set; }

        public string jsModuleImportAccess { get; private set; }

        public string jsLocalName { get; private set; }

        /// <summary>
        /// 当前类型的完整JS类型名 (如果是具化泛型类, 则为扁平化的具化泛型类名称)
        /// </summary>
        public string jsFullName { get; private set; }

        public string[] jsFullNameForReflectBind { get; private set; }

        public LegacyTSTypeNaming(BindingManager bindingManager, Type type)
        {
            var naming = bindingManager.GetTypeTransform(type)?.GetTSNaming();
            var indexOfHierarchicalName = -1;

            if (naming == null)
            {
                naming = type.Name;
            }
            else
            {
                indexOfHierarchicalName = naming.LastIndexOf('.');
            }

            // check if this type has a given hierarchical name with 'TypeTransform.Rename'
            if (indexOfHierarchicalName >= 0)
            {
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
                    this.jsModule = naming.Substring(0, indexOfHierarchicalName);
                    this.jsNamespace = "";
                    this.jsName = naming.Substring(indexOfHierarchicalName + 1);
                }

                this.jsPureName = CodeGenUtils.StripGenericDeclaration(this.jsName);
            }
            else
            {
                this.jsModule = type.Namespace ?? "";
                this.jsNamespace = "";

                // 处理内部类层级
                var declaringType = type.DeclaringType;
                while (declaringType != null)
                {
                    this.jsNamespace = this.jsNamespace.Length == 0 ? declaringType.Name : $"{declaringType.Name}.{this.jsNamespace}";
                    declaringType = declaringType.DeclaringType;
                }

                if (type.IsGenericType)
                {
                    this.jsName = CodeGenUtils.StripGenericDefinition(naming);
                    this.jsPureName = this.jsName;

                    //TODO 泛型部分改为在使用时当场生成 (无法提前构造嵌套泛型的描述)
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
                    this.jsPureName = CodeGenUtils.StripGenericDeclaration(this.jsName);
                }
            }

            if (string.IsNullOrEmpty(this.jsNamespace))
            {
                this.jsModuleAccess = this.jsName;
                this.jsModuleImportAccess = this.jsPureName;
                this.jsLocalName = "";
            }
            else
            {
                var i = this.jsNamespace.IndexOf('.');
                this.jsModuleAccess = i < 0 ? this.jsNamespace : this.jsNamespace.Substring(0, i);
                this.jsModuleImportAccess = this.jsModuleAccess;
                this.jsLocalName = CodeGenUtils.Join(".", i < 0 ? "" : this.jsNamespace.Substring(i + 1), this.jsName);
            }

            if (this.jsModuleAccess.EndsWith("[]"))
            {
                this.jsModuleAccess = this.jsModuleAccess.Substring(0, this.jsModuleAccess.Length - 2);
            }

            this.jsFullName = CodeGenUtils.Join(".", jsModule, jsNamespace, this.jsName);
            this.jsNamespaceSlice = jsNamespace.Split('.');
            this.jsNameNormalized = CodeGenUtils.StripGenericDeclaration(this.jsName);
            this.jsFullNameForReflectBind = CodeGenUtils.Strip(jsNamespaceSlice, this.jsNameNormalized);
        }
    }
}
