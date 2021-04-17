using System;
using System.IO;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    public class JSScriptFinder
    {
        private FileSystemWatcher _fsw;
        private string _baseDir;

        // filePath => ScriptRef
        private Dictionary<string, JSBehaviourScriptRef> _scripts = new Dictionary<string, JSBehaviourScriptRef>();
        // modulePath+Classname => filePath
        private Dictionary<string, string> _modulesMap = new Dictionary<string, string>();

        public JSScriptFinder(string baseDir)
        {
            _baseDir = baseDir;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
        }

        public void RefreshAll()
        {
            SearchDirectory(_baseDir);
        }

        private void SearchDirectory(string dir)
        {
            foreach (var subDir in Directory.GetDirectories(dir))
            {
                SearchDirectory(subDir);
            }

            foreach (var file in Directory.GetFiles(dir))
            {
                ParseFile(file);
            }
        }

        private void ParseFile(string filePath)
        {
            if (!filePath.EndsWith(".ts"))
            {
                return;
            }

            //TODO 待优化
            var src = File.ReadAllText(filePath);

            
        }

        public void Start()
        {
            if (_fsw != null)
            {
                return;
            }

            _fsw = new FileSystemWatcher(baseDir, "*.ts");
            _fsw.IncludeSubdirectories = true;
            _fsw.Changed += OnChanged;
            _fsw.Created += OnCreated;
            _fsw.Deleted += OnDeleted;
            _fsw.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            if (_fsw == null)
            {
                return;
            }

            _fsw.Dispose();
            _fsw = null;
        }
    }
}
