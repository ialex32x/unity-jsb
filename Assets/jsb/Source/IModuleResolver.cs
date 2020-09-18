using System;

namespace QuickJS
{
    using Native;

    public interface IModuleResolver
    {
        bool ResolveModule(string parent_module_id, string module_id, out string resolved_id);
        JSValue LoadModule(string resolved_id);
    }
}
