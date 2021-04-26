using System;

namespace QuickJS.Module
{
    using Native;
    using Utils;

    public interface IModuleResolver
    {
        bool ResolveModule(IFileSystem fileSystem, IPathResolver pathResolver, string parent_module_id, string module_id, out string resolved_id);
        bool ContainsModule(IFileSystem fileSystem, IPathResolver pathResolver, string resolved_id);
        JSValue LoadModule(ScriptContext context, string parent_module_id, string resolved_id);
        
        // this method will not consume module_obj refcount
        bool ReloadModule(ScriptContext context, string resolved_id, JSValue module_obj, out JSValue exports_obj);
    }
}
