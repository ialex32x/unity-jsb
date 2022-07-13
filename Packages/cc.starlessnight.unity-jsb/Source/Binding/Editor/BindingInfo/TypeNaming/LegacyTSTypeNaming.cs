using System;
using System.Collections.Generic;

namespace QuickJS.Binding
{
    public class LegacyTSTypeNaming : ITSTypeNaming
    {
        /// <summary>
        /// js module name
        /// </summary>
        public string moduleName { get; private set; }

        /// <summary>
        /// type path in module (without the name of this type, e.g TypeA.TypeB for TypeA.TypeB.ThisType)
        /// </summary>
        public string typePath { get; private set; }

        public string[] typePathSlice { get; private set; }

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
        /// top level name for registering in module
        /// </summary>
        public string moduleEntry { get; private set; }

        public string jsModuleImportAccess { get; private set; }

        public string jsLocalName { get; private set; }

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
                    this.moduleName = naming.Substring(0, indexOfInnerTypeName);
                    var rightName = naming.Substring(indexOfInnerTypeName + 1);
                    var lastIndexOfInnerTypeName = rightName.LastIndexOf('+');
                    if (lastIndexOfInnerTypeName >= 0)
                    {
                        this.typePath = rightName.Substring(0, lastIndexOfInnerTypeName);
                        this.jsName = rightName.Substring(lastIndexOfInnerTypeName + 1);
                    }
                    else
                    {
                        this.typePath = "";
                        this.jsName = rightName;
                    }
                }
                else
                {
                    this.moduleName = naming.Substring(0, indexOfHierarchicalName);
                    this.typePath = "";
                    this.jsName = naming.Substring(indexOfHierarchicalName + 1);
                }

                this.jsPureName = CodeGenUtils.StripGenericDeclaration(this.jsName);
            }
            else
            {
                this.moduleName = type.Namespace ?? "";
                this.typePath = "";

                // 处理内部类层级
                var declaringType = type.DeclaringType;
                while (declaringType != null)
                {
                    this.typePath = this.typePath.Length == 0 ? declaringType.Name : $"{declaringType.Name}.{this.typePath}";
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
                    // it's possible to get a name with the generic declaration part because few types (e.g System.Array) are specially processed
                    this.jsPureName = CodeGenUtils.StripGenericDeclaration(naming);
                }
            }

            if (this.typePath.Length == 0)
            {
                this.moduleEntry = this.jsName;
                this.jsModuleImportAccess = this.jsPureName;
                this.jsLocalName = "";
            }
            else
            {
                var i = this.typePath.IndexOf('.');
                this.moduleEntry = i < 0 ? this.typePath : this.typePath.Substring(0, i);
                this.jsModuleImportAccess = this.moduleEntry;
                this.jsLocalName = CodeGenUtils.Join(".", i < 0 ? "" : this.typePath.Substring(i + 1), this.jsName);
            }

            if (this.moduleEntry.EndsWith("[]"))
            {
                this.moduleEntry = this.moduleEntry.Substring(0, this.moduleEntry.Length - 2);
            }

            this.typePathSlice = typePath.Split('.');
            this.jsNameNormalized = CodeGenUtils.StripGenericDeclaration(this.jsName);
            this.jsFullNameForReflectBind = CodeGenUtils.Strip(typePathSlice, this.jsNameNormalized);
        }
    }
}
