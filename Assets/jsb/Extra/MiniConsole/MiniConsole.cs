using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace QuickJS.Extra
{
    using QuickJS;
    using QuickJS.IO;
    using QuickJS.Utils;
    using QuickJS.Native;
    using QuickJS.Binding;
    using QuickJS.Extra.WebSockets;
    using UnityEngine;
    using UnityEngine.UI;

    public class MiniConsole : IScriptLogger, ILogHandler
    {
        private bool _loopCheck;
        private int _mainThreadId;
        private int _maxLines = 50;
        private ILogHandler _defaultHandler;

        public Text textTemplate;
        public ScrollRect scrollRect;

        private List<Text> _lines = new List<Text>();

        public MiniConsole(ScrollRect scrollRect, Text textTemplate, int maxLines)
        {
            this._mainThreadId = Thread.CurrentThread.ManagedThreadId;
            this._maxLines = maxLines;
            this.scrollRect = scrollRect;
            this.textTemplate = textTemplate;
            textTemplate.gameObject.SetActive(false);

            _defaultHandler = Debug.unityLogger.logHandler;
            Debug.unityLogger.logHandler = this;
        }

        private void NewEntry(string text, Color color)
        {
            if (_mainThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                return;
            }
            
            if (scrollRect == null)
            {
                return;
            }

            try
            {
                if (text.Length > 503)
                {
                    text = text.Substring(0, 500) + "...";
                }
                if (_lines.Count > _maxLines)
                {
                    var textInst = _lines[0];
                    _lines.RemoveAt(0);
                    textInst.text = text;
                    textInst.color = color;
                    textInst.transform.SetSiblingIndex(textInst.transform.parent.childCount - 1);
                    _lines.Add(textInst);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(textInst.transform.parent.GetComponent<RectTransform>());
                }
                else
                {
                    var textInst = Object.Instantiate(textTemplate);
                    textInst.text = text;
                    textInst.color = color;
                    textInst.transform.SetParent(textTemplate.transform.parent);
                    textInst.gameObject.SetActive(true);
                    _lines.Add(textInst);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(textInst.transform.parent.GetComponent<RectTransform>());
                }
            }
            catch (Exception)
            {
            }
        }

        public void Write(LogLevel ll, string text)
        {
            switch (ll)
            {
                case LogLevel.Info: Log(text); return;
                case LogLevel.Warn: LogWarning(text); return;
                case LogLevel.Error: LogError(text); return;
                default: LogError(text); return;
            }
        }

        public void Write(LogLevel ll, string fmt, params object[] args)
        {
            switch (ll)
            {
                case LogLevel.Info: LogFormat(fmt, args); return;
                case LogLevel.Warn: LogWarningFormat(fmt, args); return;
                case LogLevel.Error: LogErrorFormat(fmt, args); return;
                default: LogErrorFormat(fmt, args); return;
            }
        }

        public void WriteException(Exception exception)
        {
            LogException(exception);
        }

        private void LogError(string text)
        {
            if (_loopCheck)
            {
                return;
            }

            _loopCheck = true;
            _defaultHandler.LogFormat(LogType.Error, null, "{0}", text);
            NewEntry(text, Color.red);
            _loopCheck = false;
        }

        private void LogException(Exception exception)
        {
            if (_loopCheck)
            {
                return;
            }

            _loopCheck = true;
            var text = exception.ToString();
            _defaultHandler.LogFormat(LogType.Error, null, "{0}", text);
            NewEntry(text, Color.cyan);
            _loopCheck = false;
        }

        private void LogException(string text)
        {
            if (_loopCheck)
            {
                return;
            }

            _loopCheck = true;
            _defaultHandler.LogFormat(LogType.Error, null, "{0}", text);
            NewEntry(text, Color.cyan);
            _loopCheck = false;
        }

        private void Log(string text)
        {
            if (_loopCheck)
            {
                return;
            }

            _loopCheck = true;
            _defaultHandler.LogFormat(LogType.Log, null, "{0}", text);
            NewEntry(text, Color.white);
            _loopCheck = false;
        }

        private void LogWarning(string text)
        {
            if (_loopCheck)
            {
                return;
            }

            _loopCheck = true;
            _defaultHandler.LogFormat(LogType.Warning, null, "{0}", text);
            NewEntry(text, Color.yellow);
            _loopCheck = false;
        }

        private void LogErrorFormat(string fmt, object[] args)
        {
            LogError(string.Format(fmt, args));
        }

        private void LogWarningFormat(string fmt, object[] args)
        {
            LogWarning(string.Format(fmt, args));
        }

        private void LogFormat(string fmt, object[] args)
        {
            Log(string.Format(fmt, args));
        }

        public void LogException(Exception exception, Object context)
        {
            LogException(exception);
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            switch (logType)
            {
                case LogType.Log:
                    LogFormat(format, args);
                    break;
                case LogType.Warning:
                    LogWarningFormat(format, args);
                    break;
                case LogType.Error:
                    LogErrorFormat(format, args);
                    break;
                case LogType.Exception:
                    LogException(string.Format(format, args));
                    break;
                case LogType.Assert:
                    LogException(string.Format(format, args));
                    break;
            }
        }
    }
}
