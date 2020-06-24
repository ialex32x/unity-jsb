using System;
using System.Collections.Generic;
using QuickJS.Native;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    public class DynamicType
    {
        private Type _type;
        private IDynamicMethod _constructor;
        private Dictionary<string, IDynamicMethod> _methods = new Dictionary<string, IDynamicMethod>();

        public DynamicType()
        {
        }

        public void Bind(ScriptContext context, Type type)
        {
            // var typeDB = context.GetTypeDB();
            // var ns = new NamespaceDecl();
            // typeDB.AddType()
        }
    }
}
