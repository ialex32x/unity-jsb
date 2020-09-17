using System;

namespace QuickJS.Unity
{
    [Flags]
    public enum TypeBindingFlags
    {
        None = 0,
        BindingCode = 1,  // 生成绑定代码
        TypeDefinition = 2, // 生成 d.ts 声明

        UnityEditorRuntime = 4, // mark as editor runtime only 
        UnityRuntime = 8, 

        Default = BindingCode | TypeDefinition | UnityRuntime,
    }
}
