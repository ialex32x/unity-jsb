using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public partial class BindingManager
    {
        public void Info(string message)
        {
            log.AppendLine(message);
        }

        public void Info(string fmt, object arg1)
        {
            log.AppendLine(fmt, arg1);
        }

        public void Info(string fmt, object arg1, string arg2)
        {
            log.AppendLine(fmt, arg1, arg2);
        }

        public void Info(string fmt, params object[] args)
        {
            log.AppendLine(fmt, args);
        }

        public void Error(string message)
        {
            Debug.LogError(message);
            log.AppendLine(message);
        }

        public void Error(string fmt, object arg1)
        {
            Debug.LogErrorFormat(fmt, arg1);
            log.AppendLine(fmt, arg1);
        }

        public void Error(string fmt, object arg1, string arg2)
        {
            Debug.LogErrorFormat(fmt, arg1, arg2);
            log.AppendLine(fmt, arg1, arg2);
        }

        public void Error(string fmt, params object[] args)
        {
            Debug.LogErrorFormat(fmt, args);
            log.AppendLine(fmt, args);
        }

        public void Warn(string message)
        {
            Debug.LogWarning(message);
            log.AppendLine(message);
        }

        public void Warn(string fmt, object arg1)
        {
            Debug.LogWarningFormat(fmt, arg1);
            log.AppendLine(fmt, arg1);
        }

        public void Warn(string fmt, object arg1, string arg2)
        {
            Debug.LogWarningFormat(fmt, arg1, arg2);
            log.AppendLine(fmt, arg1, arg2);
        }

        public void Warn(string fmt, params object[] args)
        {
            Debug.LogWarningFormat(fmt, args);
            log.AppendLine(fmt, args);
        }
    }
}
