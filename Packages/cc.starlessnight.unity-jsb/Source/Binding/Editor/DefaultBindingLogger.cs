#if UNITY_EDITOR || JSB_RUNTIME_REFLECT_BINDING
namespace QuickJS.Binding
{
    public class DefaultBindingLogger : IBindingLogger
    {
        public DefaultBindingLogger() { }

        public void Log(string message) => Diagnostics.Logger.Binding.Info(message);

        public void LogWarning(string message) => Diagnostics.Logger.Binding.Warning(message);

        public void LogError(string message) => Diagnostics.Logger.Binding.Error(message);
    }
}
#endif
