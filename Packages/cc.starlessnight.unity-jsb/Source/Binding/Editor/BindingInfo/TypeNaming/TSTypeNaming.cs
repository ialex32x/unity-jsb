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
        string jsModule { get; }

        /// <summary>
        /// js 命名空间
        /// </summary>
        string jsNamespace { get; }

        string[] jsNamespaceSlice { get; }

        ///<summary>
        /// the purified name for js (without the suffix for generic type args). 
        ///</summary>
        string jsPureName { get; }

        /// <summary>
        /// js注册名 (带平面化的泛型部分)
        /// </summary>
        string jsName { get; }

        string jsNameNormalized { get; }

        /// <summary>
        /// js 模块中的顶层访问名 (内部类的顶层访问名为最外层类的类名, 否则就是类名本身 jsPureName)
        /// </summary>
        string jsModuleAccess { get; }

        string jsModuleImportAccess { get; }

        string jsLocalName { get; }

        /// <summary>
        /// 当前类型的完整JS类型名 (如果是具化泛型类, 则为扁平化的具化泛型类名称)
        /// </summary>
        string jsFullName { get; }

        string[] jsFullNameForReflectBind { get; }
    }
}