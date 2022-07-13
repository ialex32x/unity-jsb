using System;
using System.Collections.Generic;

namespace QuickJS.Binding
{
    public class TSModuleBindingInfo
    {
        private HashSet<string> _moduleAccessNames = new HashSet<string>();

        public bool Contains(string name)
        {
            return _moduleAccessNames.Contains(name);
        }

        public void Add(string moduleEntry)
        {
            _moduleAccessNames.Add(moduleEntry);
        }
    }
}
