
namespace QuickJS.Unity
{
    using IO;
    using Utils;
    using Module;

    public static class ScriptRuntimeExtensions
    {
        public static void AddModuleResolvers(this ScriptRuntime runtime)
        {
            runtime.AddModuleResolver(new StaticModuleResolver());
            runtime.AddModuleResolver(new JsonModuleResolver());
            runtime.AddModuleResolver(new SourceModuleResolver(new DefaultJsonConverter()));
        }

        public static void Initialize(this ScriptRuntime runtime, IScriptRuntimeListener listener)
        {
            var logger = new DefaultLogger();
            var fileResolver = new PathResolver();
            var fileSystem = new DefaultFileSystem(logger);
            var asyncManager = new DefaultAsyncManager();

            runtime.AddModuleResolvers();
            runtime.Initialize(fileSystem, fileResolver, listener, asyncManager, logger, new ByteBufferPooledAllocator());
        }
    }
}
