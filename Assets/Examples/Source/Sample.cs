using System;
using QuickJS;
using QuickJS.Binding;
using QuickJS.Utils;
using QuickJS.IO;

namespace jsb
{
    using UnityEngine;

    public class Sample : MonoBehaviour, IScriptRuntimeListener
    {
        public class TextUnityLogger : IScriptLogger
        {
            private UnityEngine.UI.Text _text;

            public TextUnityLogger(UnityEngine.UI.Text text)
            {
                _text = text;
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
                if (_text != null)
                {
                    _text.text += text;
                }
            }

            private void LogErrorFormat(string fmt, object[] args)
            {
                if (_text != null)
                {
                    _text.text += string.Format(fmt, args);
                }
            }

            private void LogWarningFormat(string fmt, object[] args)
            {
                if (_text != null)
                {
                    _text.text += string.Format(fmt, args);
                }
            }

            private void LogFormat(string fmt, object[] args)
            {
                if (_text != null)
                {
                    _text.text += string.Format(fmt, args);
                }
            }

            private void LogException(Exception exception)
            {
                if (_text != null)
                {
                    _text.text += exception;
                }
            }

            private void Log(string text)
            {
                if (_text != null)
                {
                    _text.text += text;
                }
            }

            private void LogWarning(string text)
            {
                if (_text != null)
                {
                    _text.text += text;
                }
            }
        }

        public enum FileLoader
        {
            Default,
            Resources,
            HMR,
        }
        public UnityEngine.UI.Text text;
        public FileLoader fileLoader;
        public string baseUrl = "http://127.0.0.1:8182";
        public bool sourceMap;
        public bool stacktrace;
        private ScriptRuntime _rt;
        private TextUnityLogger _tul;

        void Awake()
        {
            IFileSystem fileSystem;

            _tul = new TextUnityLogger(text);
            _rt = ScriptEngine.CreateRuntime();
            _rt.AddSearchPath("node_modules");

            if (fileLoader == FileLoader.Resources)
            {
                fileSystem = new ResourcesFileSystem();
                _rt.AddSearchPath("dist");
            }
            else if (fileLoader == FileLoader.HMR)
            {
                Debug.LogWarningFormat("功能未完成");
                fileSystem = new HttpFileSystem(baseUrl);
            }
            else
            {
                fileSystem = new DefaultFileSystem();
                _rt.AddSearchPath("Assets/Examples/Scripts/out");
                // _rt.AddSearchPath("Assets/Examples/Scripts/dist");
            }

            _rt.withStacktrace = stacktrace;
            if (sourceMap)
            {
                _rt.EnableSourceMap();
            }
            _tul.Write(LogLevel.Info, "Init");
            _rt.Initialize(fileSystem, this, _tul, new ByteBufferPooledAllocator());
        }

        void Update()
        {
            _rt.Update(Time.deltaTime);
        }

        void OnDestroy()
        {
            _rt.Destroy();
        }

        public void OnBind(ScriptRuntime runtime, TypeRegister register)
        {
            _tul.Write(LogLevel.Info, "Bind");
            QuickJS.Extra.WebSocket.Bind(register);
            QuickJS.Extra.XMLHttpRequest.Bind(register);
            QuickJS.Extra.DOMCompatibleLayer.Bind(register);
            QuickJS.Extra.NodeCompatibleLayer.Bind(register);
            _tul.Write(LogLevel.Info, "Bind Finish");
        }

        public void OnComplete(ScriptRuntime runtime)
        {
            _tul.Write(LogLevel.Info, "run");
            _rt.EvalMain("main.js");
        }
    }
}