using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QuickJS.Module
{
    using Native;
    using Binding;

    [Obsolete]
    public struct TypeReg
    {
        public string[] ns;
        public ClassBind bind;
    }

    public class TypeTree
    {
        public Type type; 
        public Dictionary<string, TypeTree> children;
    }
}
