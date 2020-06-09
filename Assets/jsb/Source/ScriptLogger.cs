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
        void Error(Exception exception);

        void Write(LogLevel ll, string text);
        void Write(LogLevel ll, string fmt, params object[] args);

        void ScriptWrite(LogLevel ll, string text);
        void ScriptWrite(LogLevel ll, string fmt, params object[] args);
    }

    public class UnityLogger : IScriptLogger
    {
        public void Error(Exception exception)
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

        public void ScriptWrite(LogLevel ll, string text)
        {
            switch (ll)
            {
                case LogLevel.Info: Debug.Log(text); return;
                case LogLevel.Warn: Debug.LogWarning(text); return;
                case LogLevel.Error: Debug.LogError(text); return;
                default: Debug.LogError(text); return;
            }
        }

        public void ScriptWrite(LogLevel ll, string fmt, params object[] args)
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
