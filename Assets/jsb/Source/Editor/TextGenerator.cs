using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace QuickJS.Editor
{
    using UnityEngine;
    using UnityEditor;

    public class TextGenerator
    {
        public bool enabled = true;
        private string newline;
        private string tab;
        private StringBuilder sb = new StringBuilder();
        private int tabLevel;

        public int size { get { return sb.Length; } }

        public TextGenerator(string newline, string tab)
        {
            this.newline = newline;
            this.tab = tab;
            this.tabLevel = 0;
        }

        public override string ToString()
        {
            return sb.ToString();
        }

        public void AddTabLevel()
        {
            tabLevel++;
        }

        public void DecTabLevel()
        {
            tabLevel--;
        }

        public void AppendTab()
        {
            for (var i = 0; i < tabLevel; i++)
            {
                sb.Append(tab);
            }
        }

        public void AppendLines(params string[] lines)
        {
            foreach (var line in lines)
            {
                AppendLine(line);
            }
        }

        public void AppendLine()
        {
            sb.Append(newline);
        }

        public void AppendLine(string text)
        {
            AppendTab();
            sb.Append(text);
            sb.Append(newline);
        }

        public void AppendLine(string text, object arg1)
        {
            AppendTab();
            sb.AppendFormat(text, arg1);
            sb.Append(newline);
        }

        public void AppendLine(string text, object arg1, object arg2)
        {
            AppendTab();
            sb.AppendFormat(text, arg1, arg2);
            sb.Append(newline);
        }

        public void AppendLine(string text, object arg1, object arg2, object arg3)
        {
            AppendTab();
            sb.AppendFormat(text, arg1, arg2, arg3);
            sb.Append(newline);
        }

        public void AppendLine(string text, params object[] args)
        {
            AppendTab();
            sb.AppendFormat(text, args);
            sb.Append(newline);
        }

        public void AppendLineL(string text)
        {
            sb.Append(text);
            sb.Append(newline);
        }

        public void AppendLineL(string text, object arg1)
        {
            sb.AppendFormat(text, arg1);
            sb.Append(newline);
        }

        public void AppendLineL(string text, object arg1, object arg2)
        {
            sb.AppendFormat(text, arg1, arg2);
            sb.Append(newline);
        }

        public void AppendLineL(string text, object arg1, object arg2, object arg3)
        {
            sb.AppendFormat(text, arg1, arg2, arg3);
            sb.Append(newline);
        }

        public void AppendLineL(string text, params object[] args)
        {
            sb.AppendFormat(text, args);
            sb.Append(newline);
        }

        public void Append(string text)
        {
            AppendTab();
            sb.Append(text);
        }

        public void Append(string text, object arg1)
        {
            AppendTab();
            sb.AppendFormat(text, arg1);
        }

        public void Append(string text, object arg1, object arg2)
        {
            AppendTab();
            sb.AppendFormat(text, arg1, arg2);
        }

        public void Append(string text, object arg1, object arg2, object arg3)
        {
            AppendTab();
            sb.AppendFormat(text, arg1, arg2, arg3);
        }

        public void Append(string text, params object[] args)
        {
            AppendTab();
            sb.AppendFormat(text, args);
        }

        public void AppendL(string text)
        {
            sb.Append(text);
        }

        public void AppendL(string text, object arg1)
        {
            sb.AppendFormat(text, arg1);
        }

        public void AppendL(string text, object arg1, object arg2)
        {
            sb.AppendFormat(text, arg1, arg2);
        }

        public void AppendL(string text, object arg1, object arg2, object arg3)
        {
            sb.AppendFormat(text, arg1, arg2, arg3);
        }

        public void AppendL(string text, params object[] args)
        {
            sb.AppendFormat(text, args);
        }

        public void Clear()
        {
            tabLevel = 0;
            sb.Clear();
        }
    }
}
