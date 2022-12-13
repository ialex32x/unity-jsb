using System;

namespace QuickJS.Diagnostics
{
    public interface ILogWriter
    {
        void Write(ELogSeverity severity, string channel, string text);
        void Write(ELogSeverity severity, string channel, string fmt, object[] args);
    }

    public class DefaultLogWriter : ILogWriter
    {
        public void Write(ELogSeverity severity, string channel, string text)
        {
            var content = string.Format("[{0}] {1}", channel, text);

#if JSB_UNITYLESS
            switch (severity)
            {
                case ELogSeverity.VeryVerbose: System.Console.WriteLine("[VERY ] " + content); break;
                case ELogSeverity.Verbose:     System.Console.WriteLine("[VERB ] " + content); break;
                case ELogSeverity.Debug:       System.Console.WriteLine("[DEBUG] " + content); break;
                case ELogSeverity.Info:        System.Console.WriteLine("[INFO ] " + content); break;
                case ELogSeverity.Warning:     System.Console.WriteLine("[WARN ] " + content); break;
                case ELogSeverity.Fatal:       System.Console.WriteLine("[FATAL] " + content); break;
                case ELogSeverity.Error:
                default:                       System.Console.WriteLine("[ERROR  ] " + content); break;
            }
#else
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
#endif
        }

        public void Write(ELogSeverity severity, string channel, string fmt, object[] args)
        {
            Write(severity, channel, string.Format(fmt, args));
        }
    }
}
