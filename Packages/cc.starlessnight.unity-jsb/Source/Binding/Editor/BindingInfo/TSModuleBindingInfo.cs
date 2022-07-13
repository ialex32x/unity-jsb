using System;
using System.Collections.Generic;

namespace QuickJS.Binding
{
    public class TSModuleBindingInfo
    {
        private HashSet<string> _moduleAccessNames = new HashSet<string>();

        public TSModuleBindingInfo()
        {
        }

        public bool Contains(string name)
        {
            return _moduleAccessNames.Contains(name);
        }

        public void Add(TypeBindingInfo typeBindingInfo)
        {
            _moduleAccessNames.Add(typeBindingInfo.tsTypeNaming.moduleEntry);
        }
    }
}
