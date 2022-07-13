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

        /// <summary>
        /// top level name for registering in module
        /// </summary>
        public string moduleEntry { get; private set; }

        public string[] fullPathSlice => CodeGenUtils.Strip(this.typePathSlice, CodeGenUtils.StripGenericDeclaration(this.className));

        /// <summary>
        /// class name for registering class in js <br/>
        /// - for generic type definitions, it's the type name with generic args (only for generating d.ts) <br/>
        /// - for constructed generic types, it's transformed into 'Type_GenericType' <br/>
        /// </summary>
        public string className { get; private set; }

        ///<summary>
        /// the purified name for js (without the suffix for generic type args). 
        ///</summary>
        public string jsPureName { get; private set; }

        public string jsLocalName { get; private set; }

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
                        this.className = rightName.Substring(lastIndexOfInnerTypeName + 1);
                    }
                    else
                    {
                        this.typePath = "";
                        this.className = rightName;
                    }
                }
                else
                {
                    this.moduleName = naming.Substring(0, indexOfHierarchicalName);
                    this.typePath = "";
                    this.className = naming.Substring(indexOfHierarchicalName + 1);
                }

                this.jsPureName = CodeGenUtils.StripGenericDeclaration(this.className);
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
                    this.className = CodeGenUtils.StripGenericDefinition(naming);
                    this.jsPureName = this.className;

                    if (type.IsGenericTypeDefinition)
                    {
                        if (!naming.Contains("<"))
                        {
                            this.className += "<";
                            var gArgs = type.GetGenericArguments();

                            for (var i = 0; i < gArgs.Length; i++)
                            {
                                this.className += gArgs[i].Name;
                                if (i != gArgs.Length - 1)
                                {
                                    this.className += ", ";
                                }
                            }
                            this.className += ">";
                        }
                    }
                    else
                    {
                        foreach (var gp in type.GetGenericArguments())
                        {
                            this.className += "_" + gp.Name;
                        }
                    }
                }
                else
                {
                    this.className = naming;
                    // it's possible to get a name with the generic declaration part because few types (e.g System.Array) are specially processed
                    this.jsPureName = CodeGenUtils.StripGenericDeclaration(naming);
                }
            }

            if (this.typePath.Length == 0)
            {
                this.moduleEntry = this.jsPureName;
                this.jsLocalName = "";
            }
            else
            {
                var i = this.typePath.IndexOf('.');
                this.moduleEntry = i < 0 ? this.typePath : this.typePath.Substring(0, i);
                this.jsLocalName = CodeGenUtils.Join(".", i < 0 ? "" : this.typePath.Substring(i + 1), this.className);
            }

            if (this.moduleEntry.EndsWith("[]"))
            {
                CodeGenUtils.Assert(false, "should be unreachable?");
                this.moduleEntry = this.moduleEntry.Substring(0, this.moduleEntry.Length - 2);
            }
            this.typePathSlice = typePath.Split('.');
        }
    }
}
