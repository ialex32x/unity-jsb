using System;

namespace QuickJS
{
    using Native;

    public class FileModuleResolver : IModuleResolver
    {
        public bool ResolveModule(string parent_module_id, string module_id, out string resolved_id)
        {
            throw new NotImplementedException();
        }

        public JSValue LoadModule(string resolved_id)
        {
            throw new NotImplementedException();
        }
    }

    public class RegisterModuleResolver : IModuleResolver
    {
        public bool ResolveModule(string parent_module_id, string module_id, out string resolved_id)
        {
            throw new NotImplementedException();
        }

        public JSValue LoadModule(string resolved_id)
        {
            throw new NotImplementedException();
        }
    }
}
