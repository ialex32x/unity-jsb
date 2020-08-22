using System;

namespace QuickJS.Editor
{
    [Flags]
    public enum TypeBindingFlags
    {
        BindingCode = 1, 
        TypeDefinition = 2, 

        EditorRuntime = 4, 

        Default = BindingCode | TypeDefinition, 
    }
}
