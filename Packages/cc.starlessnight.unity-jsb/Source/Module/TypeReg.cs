using System;
using System.Collections.Generic;

namespace QuickJS.Module
{
    [Obsolete]
    public struct TypeReg
    {
        public string[] ns;
    }

    public class TypeTree
    {
        public Type type; 
        public Dictionary<string, TypeTree> children;
    }
}
