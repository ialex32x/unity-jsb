using System;

namespace QuickJS.Diagnostics
{
    public interface ILogHandler
    {
        void Write(ELogSeverity severity, string channel, string text);
        void Write(ELogSeverity severity, string channel, string fmt, object[] args);
    }

    public class UnityLogHandler : ILogHandler
    {
        public void Write(ELogSeverity severity, string channel, string text)
        {
            var content = string.Format("[{0}] {1}", channel, text);
            switch (severity)
            {
                case ELogSeverity.VeryVerbose:
                case ELogSeverity.Verbose:
                case ELogSeverity.Debug:
                case ELogSeverity.Info: UnityEngine.Debug.Log(content); break;
                case ELogSeverity.Warning: UnityEngine.Debug.LogWarning(content); break;
                case ELogSeverity.Fatal: UnityEngine.Debug.LogError(content); UnityEngine.Debug.Break(); break;
                case ELogSeverity.Error:
                default: UnityEngine.Debug.LogError(content); break;
            }
        }

        public void Write(ELogSeverity severity, string channel, string fmt, object[] args)
        {
            Write(severity, channel, string.Format(fmt, args));
        }
    }
}
