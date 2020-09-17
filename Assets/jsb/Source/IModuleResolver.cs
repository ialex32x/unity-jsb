using System;

namespace QuickJS
{
    using Native;

    public interface IModuleResolver
    {
        bool Resolve(string parent_module_id, string module_id, out string resolved_id, out JSValue mod_obj);
    }
}
