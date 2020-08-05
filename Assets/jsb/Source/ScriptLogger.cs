using System;
using QuickJS.Native;
using UnityEngine;

namespace QuickJS
{
    public enum LogLevel
    {
        Info = 0,
        Warn = 1,
        Error = 2,
        Assert = 3,
    }

    public interface IScriptLogger
    {
        void Write(LogLevel ll, string text);
        void Write(LogLevel ll, string fmt, params object[] args);
        void WriteException(Exception exception);
    }

    public class UnityLogger : IScriptLogger
    {
        public void WriteException(Exception exception)
        {
            Debug.LogException(exception);
        }

        public void Write(LogLevel ll, string text)
        {
            switch (ll)
            {
                case LogLevel.Info: Debug.Log(text); return;
                case LogLevel.Warn: Debug.LogWarning(text); return;
                case LogLevel.Error: Debug.LogError(text); return;
                default: Debug.LogError(text); return;
            }
        }

        public void Write(LogLevel ll, string fmt, params object[] args)
        {
            switch (ll)
            {
                case LogLevel.Info: Debug.LogFormat(fmt, args); return;
                case LogLevel.Warn: Debug.LogWarningFormat(fmt, args); return;
                case LogLevel.Error: Debug.LogErrorFormat(fmt, args); return;
                default: Debug.LogErrorFormat(fmt, args); return;
            }
        }
    }
}
