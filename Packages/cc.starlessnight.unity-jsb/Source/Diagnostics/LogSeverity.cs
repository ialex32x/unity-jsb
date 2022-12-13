using System;

namespace QuickJS.Diagnostics
{
    [Flags]
    public enum ELogSeverity
    {
        VeryVerbose = 1,
        Verbose = 2,
        Debug = 4,
        Info = 8,
        Warning = 16,
        Error = 32,
        Fatal = 64,

        All = VeryVerbose | Verbose | Debug | Info | Warning | Error | Fatal,
    }
}
