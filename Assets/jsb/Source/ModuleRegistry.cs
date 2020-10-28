using System;
using System.Collections.Generic;

namespace QuickJS
{
    using Binding;

    public delegate void TypeBindAction(TypeRegister register);

    public class ModuleRegistry
    {
        private Dictionary<string, TypeBindAction> _actions = new Dictionary<string, TypeBindAction>();

        public void Add(string name, TypeBindAction action)
        {
            _actions[name] = action;
        }

        public void Bind(string name, TypeRegister register)
        {
            TypeBindAction action;
            if (_actions.TryGetValue(name, out action))
            {
                action(register);
            }
        }
    }
}
