using System;

namespace QuickJS.Module
{
    using Native;
    using Utils;

    public interface IModuleResolver
    {
        bool ResolveModule(IFileSystem fileSystem, IPathResolver pathResolver, string parent_module_id, string module_id, out string resolved_id);
        JSValue LoadModule(ScriptContext context, string parent_module_id, string resolved_id);
    }
}
