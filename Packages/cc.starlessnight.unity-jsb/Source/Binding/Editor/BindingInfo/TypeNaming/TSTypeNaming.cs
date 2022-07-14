using System;
using System.Collections.Generic;

namespace QuickJS.Binding
{
    public abstract class ITSTypeNaming
    {
        /// <summary>
        /// js module name <br/>
        /// NOTE: in legacy mode, it will be empty if the corresponding csharp type is not in namespace
        /// </summary>
        public string moduleName { get; protected set; }

        /// <summary>
        /// full path of class apart from the generic arguments of generic type definition
        /// </summary>
        public string[] classPath { get; protected set; }

        /// <summary>
        /// (optional) only for renamed types
        /// </summary>
        public string genericDefinition { get; protected set; }

        /// <summary>
        /// e.g NS1 from NS1.OUT1.THIS
        /// </summary>
        public string moduleEntry => classPath[0];

        public string className => classPath[classPath.Length - 1];

        /// <summary>
        /// joined the class path without the type name (the last element)
        /// e.g NS1.OUT1 from NS1.OUT1.THIS
        /// </summary>
        public string ns => classPath.Length == 1 ? string.Empty : CodeGenUtils.Join(".", classPath, 0, classPath.Length - 1);

        /// <summary>
        /// full name without generic part
        /// </summary>
        public string GetFullName(string alias = null)
        {
            if (alias == null)
            {
                return CodeGenUtils.Join(".", CodeGenUtils.Join(".", classPath, 0, classPath.Length), genericDefinition);
            }
            return CodeGenUtils.Join(".", CodeGenUtils.Join(".", alias, CodeGenUtils.Join(".", classPath, 1, classPath.Length - 1)), genericDefinition);
        }

        protected string StripCSharpGenericDefinition(string typeName)
        {
            var gArgIndex = typeName.IndexOf('`');
            return gArgIndex < 0 ? typeName : typeName.Substring(0, gArgIndex);
        }

        public abstract void Initialize(BindingManager bindingManager, Type type);
    }
}
