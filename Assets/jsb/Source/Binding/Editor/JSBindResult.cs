using System;
using System.Collections.Generic;

namespace QuickJS.Binding
{
    /// <summary>
    /// 用于记录导出过程中的辅助信息
    /// 可供 webpack 配置使用
    /// </summary>
    public class JSBindResult
    {
        public string comment;

        /// <summary>
        /// 导出的模块名列表 (可用于 webpack externals 配置)
        /// </summary>
        public List<string> modules = new List<string>();
    }
}
