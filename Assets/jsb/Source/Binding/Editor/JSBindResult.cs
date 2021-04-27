using System;
using System.Collections.Generic;

namespace QuickJS.Binding
{
    /// <summary>
    /// 用于记录导出过程中的辅助信息
    /// 可供 webpack 配置使用
    /// 以及反射方式运行时的辅助信息 (未实现)
    /// </summary>
    public class JSBindResult
    {
        /// <summary>
        /// 备注
        /// </summary>
        public string comment;

        /// <summary>
        /// 导出的模块名列表 (可用于 webpack externals 配置)
        /// </summary>
        public List<string> modules = new List<string>();
    }
}
