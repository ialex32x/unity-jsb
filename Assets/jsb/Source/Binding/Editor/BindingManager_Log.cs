using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Binding
{
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
            _logger.LogError(message);
            log.AppendLine(message);
        }

        public void Error(string fmt, object arg1)
        {
            _logger.LogError(string.Format(fmt, arg1));
            log.AppendLine(fmt, arg1);
        }

        public void Error(string fmt, object arg1, string arg2)
        {
            _logger.LogError(string.Format(fmt, arg1, arg2));
            log.AppendLine(fmt, arg1, arg2);
        }

        public void Error(string fmt, params object[] args)
        {
            _logger.LogError(string.Format(fmt, args));
            log.AppendLine(fmt, args);
        }

        public void Warn(string message)
        {
            _logger.LogWarning(message);
            log.AppendLine(message);
        }

        public void Warn(string fmt, object arg1)
        {
            _logger.LogWarning(string.Format(fmt, arg1));
            log.AppendLine(fmt, arg1);
        }

        public void Warn(string fmt, object arg1, string arg2)
        {
            _logger.LogWarning(string.Format(fmt, arg1, arg2));
            log.AppendLine(fmt, arg1, arg2);
        }

        public void Warn(string fmt, params object[] args)
        {
            _logger.LogWarning(string.Format(fmt, args));
            log.AppendLine(fmt, args);
        }
    }
}
