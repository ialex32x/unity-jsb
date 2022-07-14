using System;
using System.Collections.Generic;

namespace QuickJS.Binding
{
    public interface ITSTypeNaming
    {
        /// <summary>
        /// js module name <br/>
        /// NOTE: in legacy mode, it will be empty if the corresponding csharp type is not in namespace
        /// </summary>
        string moduleName { get; }

        /// <summary>
        /// top level name for registering in module
        /// </summary>
        string moduleEntry { get; }

        /// <summary>
        /// type path in module (without the name of this type, e.g TypeA.TypeB for TypeA.TypeB.ThisType)
        /// </summary>
        string typePath { get; }

        /// <summary>
        /// class name for registering class in js <br/>
        /// - for generic type definitions, it's the type name with generic args (only for generating d.ts) <br/>
        /// - for constructed generic types, it's transformed into 'Type_GenericType' <br/>
        /// </summary>
        string className { get; }

#region WIP - refactoring module structure
        ///<summary>
        /// the purified name for js (without the suffix for generic type args). 
        ///</summary>
        string jsPureName { get; }

        /// <summary>
        /// local class name path (e.g TypeB.ThisType<T>, apart from the moduleEntry)
        /// </summary>
        string jsLocalName { get; }
#endregion
    }
}
