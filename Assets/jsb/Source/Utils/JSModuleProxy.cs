using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QuickJS.Utils
{
    using Binding;

    public interface IModuleProxy
    {
        void Load(ScriptContext context, JSValue module_obj, JSValue exports_obj);
    }

    public class JSModuleProxy : IModuleProxy
    {
        public delegate void TypeBind(TypeRegister register);

        private Dictionary<Type, TypeBind> _types = new Dictionary<Type, TypeBind>();

        public void Add(Type type, TypeBind bind)
        {
            _types[type] = bind;
        }

        public void Load(ScriptContext context, JSValue module_obj, JSValue exports_obj)
        {
            // create js proxy object as module.exports
        }
    }
}