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
        /// js 模块中的顶层访问名 (内部类的顶层访问名为最外层类的类名, 否则就是类名本身 jsPureName)
        /// </summary>
        string moduleEntry { get; }

        /// <summary>
        /// name of the module entry for accessing this type
        /// </summary>
        string typePath { get; }

        string[] typePathSlice { get; }

        /// <summary>
        /// e.g ["TypeA", "TypeB", "ThisType"]
        /// </summary>
        string[] fullPathSlice { get; }

#region WIP - refactoring module structure
        ///<summary>
        /// the purified name for js (without the suffix for generic type args). 
        ///</summary>
        string jsPureName { get; }

        /// <summary>
        /// js注册名 (带平面化的泛型部分)
        /// </summary>
        string jsName { get; }

        string jsNameNormalized { get; }

        string jsModuleImportAccess { get; }

        string jsLocalName { get; }
#endregion
    }
}
