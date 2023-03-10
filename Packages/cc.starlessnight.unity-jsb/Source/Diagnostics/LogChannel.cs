using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace QuickJS.Diagnostics
{
    public sealed class LogChannel
    {
        private bool _enabled = true;
        private string _name = string.Empty;

        public bool enabled { get => _enabled; set => _enabled = value; }

        public string name => _name;

        public LogChannel()
        {
            _name = "Default";
        }

        public LogChannel(string name)
        {
            _name = string.IsNullOrEmpty(name) ? "Default" : name;
        }

        [Conditional("JSB_DEBUG")]
        public void VeryVerbose(string text)
        {
            if (!_enabled) return;
            Logger.Write(ELogSeverity.VeryVerbose, _name, text);
        }

        [Conditional("JSB_DEBUG")]
        public void VeryVerbose(object obj)
        {
            if (!_enabled || obj == null) return;
            Logger.Write(ELogSeverity.VeryVerbose, _name, obj.ToString());
        }

        [Conditional("JSB_DEBUG")]
        public void VeryVerbose(string fmt, params object[] args)
        {
            if (!_enabled) return;
            Logger.Write(ELogSeverity.VeryVerbose, _name, fmt, args);
        }

        [Conditional("JSB_DEBUG")]
        public void Verbose(string text)
        {
            if (!_enabled) return;
            Logger.Write(ELogSeverity.Verbose, _name, text);
        }

        [Conditional("JSB_DEBUG")]
        public void Verbose(object obj)
        {
            if (!_enabled || obj == null) return;
            Logger.Write(ELogSeverity.Verbose, _name, obj.ToString());
        }

        [Conditional("JSB_DEBUG")]
        public void Verbose(string fmt, params object[] args)
        {
            if (!_enabled) return;
            Logger.Write(ELogSeverity.Verbose, _name, fmt, args);
        }

        [Conditional("JSB_DEBUG")]
        public void Debug(string text)
        {
            if (!_enabled) return;
            Logger.Write(ELogSeverity.Debug, _name, text);
        }

        [Conditional("JSB_DEBUG")]
        public void Debug(object obj)
        {
            if (!_enabled || obj == null) return;
            Logger.Write(ELogSeverity.Debug, _name, obj.ToString());
        }

        [Conditional("JSB_DEBUG")]
        public void Debug(string fmt, params object[] args)
        {
            if (!_enabled) return;
            Logger.Write(ELogSeverity.Debug, _name, fmt, args);
        }

        [Conditional("JSB_DEBUG")]
        public void Info(string text)
        {
            if (!_enabled) return;
            Logger.Write(ELogSeverity.Info, _name, text);
        }

        [Conditional("JSB_DEBUG")]
        public void Info(object obj)
        {
            if (!_enabled || obj == null) return;
            Logger.Write(ELogSeverity.Info, _name, obj.ToString());
        }

        [Conditional("JSB_DEBUG")]
        public void Info(string fmt, params object[] args)
        {
            if (!_enabled) return;
            Logger.Write(ELogSeverity.Info, _name, fmt, args);
        }

        [Conditional("JSB_DEBUG")]
        public void Warning(string text)
        {
            if (!_enabled) return;
            Logger.Write(ELogSeverity.Warning, _name, text);
        }

        [Conditional("JSB_DEBUG")]
        public void Warning(object obj)
        {
            if (!_enabled || obj == null) return;
            Logger.Write(ELogSeverity.Warning, _name, obj.ToString());
        }

        [Conditional("JSB_DEBUG")]
        public void Warning(string fmt, params object[] args)
        {
            if (!_enabled) return;
            Logger.Write(ELogSeverity.Warning, _name, fmt, args);
        }

        [Conditional("JSB_DEBUG")]
        [Conditional("JSB_RELEASE")]
        public void Error(string text)
        {
            if (!_enabled) return;
            Logger.Write(ELogSeverity.Error, _name, text);
        }

        [Conditional("JSB_DEBUG")]
        [Conditional("JSB_RELEASE")]
        public void Error(object obj)
        {
            if (!_enabled || obj == null) return;
            Logger.Write(ELogSeverity.Error, _name, obj.ToString());
        }

        [Conditional("JSB_DEBUG")]
        [Conditional("JSB_RELEASE")]
        public void Error(string fmt, params object[] args)
        {
            if (!_enabled) return;
            Logger.Write(ELogSeverity.Error, _name, fmt, args);
        }

        [Conditional("JSB_DEBUG")]
        [Conditional("JSB_RELEASE")]
        public void Fatal(string text)
        {
            if (!_enabled) return;
            Logger.Write(ELogSeverity.Fatal, _name, text);
        }

        [Conditional("JSB_DEBUG")]
        [Conditional("JSB_RELEASE")]
        public void Fatal(object obj)
        {
            if (!_enabled || obj == null) return;
            Logger.Write(ELogSeverity.Fatal, _name, obj.ToString());
        }

        [Conditional("JSB_DEBUG")]
        [Conditional("JSB_RELEASE")]
        public void Fatal(string fmt, params object[] args)
        {
            if (!_enabled) return;
            Logger.Write(ELogSeverity.Fatal, _name, fmt, args);
        }

        public void Write(ELogSeverity severity, object obj)
        {
            if (!_enabled || obj == null) return;
            Logger.Write(severity, _name, obj.ToString());
        }

        public void Write(ELogSeverity severity, string fmt, params object[] args)
        {
            if (!_enabled) return;
            Logger.Write(severity, _name, fmt, args);
        }

        public void Exception(Exception exception)
        {
            if (!_enabled) return;
            Logger.Write(ELogSeverity.Error, _name, exception.ToString());
        }

        public void Exception(string description, Exception exception)
        {
            if (!_enabled) return;
            Logger.Write(ELogSeverity.Error, _name, "{0}\n{1}", description, exception);
        }
    }
}
