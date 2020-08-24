using System;

namespace QuickJS.Editor
{
    [Flags]
    public enum TypeBindingFlags
    {
        None = 0,
        BindingCode = 1,  // 生成绑定代码
        TypeDefinition = 2, // 生成 d.ts 声明
        Default = BindingCode | TypeDefinition,

        EditorRuntime = 4, // 编辑器运行时标记
    }
}
