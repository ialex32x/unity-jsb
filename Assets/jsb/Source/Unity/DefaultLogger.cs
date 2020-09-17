using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;

namespace QuickJS.Unity
{
    using UnityEngine;
    using Native;
    using Utils;

    public class DefaultLogger : IScriptLogger
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
