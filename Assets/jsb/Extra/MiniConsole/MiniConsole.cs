using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using AOT;
using System.Text;

namespace QuickJS.Extra
{
    using QuickJS;
    using QuickJS.IO;
    using QuickJS.Native;
    using QuickJS.Binding;
    using QuickJS.Extra.WebSockets;
    using UnityEngine;
    using UnityEngine.UI;

    public class MiniConsole : IScriptLogger
    {
        private int _maxLines = 100;
        public Text textTemplate;
        public ScrollRect scrollRect;

        private List<Text> _lines = new List<Text>();

        public MiniConsole(ScrollRect scrollRect, Text textTemplate, int maxLines)
        {
            this._maxLines = maxLines;
            this.scrollRect = scrollRect;
            this.textTemplate = textTemplate;
            textTemplate.gameObject.SetActive(false);
        }

        private void NewEntry(string text, Color color)
        {
            if (scrollRect == null)
            {
                return;
            }

            if (_lines.Count > _maxLines)
            {
                var textInst = _lines[0];
                _lines.RemoveAt(0);
                textInst.text = text;
                textInst.color = color;
                textInst.transform.SetSiblingIndex(textInst.transform.parent.childCount - 1);
                _lines.Add(textInst);
            }
            else
            {
                var textInst = Object.Instantiate(textTemplate);
                textInst.text = text;
                textInst.color = color;
                textInst.transform.SetParent(textTemplate.transform.parent);
                textInst.gameObject.SetActive(true);
                _lines.Add(textInst);
            }
        }

        public void Error(Exception exception)
        {
            LogException(exception);
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

        public void ScriptWrite(LogLevel ll, string text)
        {
            switch (ll)
            {
                case LogLevel.Info: Log(text); return;
                case LogLevel.Warn: LogWarning(text); return;
                case LogLevel.Error: LogError(text); return;
                default: LogError(text); return;
            }
        }

        public void ScriptWrite(LogLevel ll, string fmt, params object[] args)
        {
            switch (ll)
            {
                case LogLevel.Info: LogFormat(fmt, args); return;
                case LogLevel.Warn: LogWarningFormat(fmt, args); return;
                case LogLevel.Error: LogErrorFormat(fmt, args); return;
                default: LogErrorFormat(fmt, args); return;
            }
        }

        private void LogError(string text)
        {
            NewEntry(text, Color.red);
        }

        private void LogErrorFormat(string fmt, object[] args)
        {
            NewEntry(string.Format(fmt, args), Color.red);
        }

        private void LogWarningFormat(string fmt, object[] args)
        {
            NewEntry(string.Format(fmt, args), Color.yellow);
        }

        private void LogFormat(string fmt, object[] args)
        {
            NewEntry(string.Format(fmt, args), Color.white);
        }

        private void LogException(Exception exception)
        {
            NewEntry(exception.ToString(), Color.red);
        }

        private void Log(string text)
        {
            NewEntry(text, Color.white);
        }

        private void LogWarning(string text)
        {
            NewEntry(text, Color.yellow);
        }
    }
}
