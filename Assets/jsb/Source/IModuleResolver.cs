using System;

namespace QuickJS
{
    using Native;

    /*
foreach (var r in list)
{
    JSValue mod;
    string id;
    if (r.ResolveModule(p, m, out id))
    {
        if (LoadCache(id, out mod))
        {
            return mod;
        }

        if (r.LoadModule(id, out mod))
        {

        }

        return error;
    }
}
return error;
    */
    public interface IModuleResolver
    {
        bool ResolveModule(string parent_module_id, string module_id, out string resolved_id);
        JSValue LoadModule(string resolved_id);
    }
}
