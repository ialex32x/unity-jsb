#if !JSB_UNITYLESS
using System;
using System.IO;
using System.Collections.Generic;

namespace QuickJS.Unity
{
    public class JSScriptFinder
    {
        private FileSystemWatcher _fsw;
        private string _baseDir;
        private string _fileExt;

        // classPath => JSScriptClassPathHint
        private Dictionary<string, JSScriptClassPathHint> _scriptClassPaths = new Dictionary<string, JSScriptClassPathHint>();

        public JSScriptFinder(string baseDir, string fileExt)
        {
            _baseDir = baseDir;
            _fileExt = fileExt;
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
            _scriptClassPaths.Clear();
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
                ParseFile(file.Replace('\\', '/'));
            }
        }

        private void ParseFile(string filePath)
        {
            if (!filePath.EndsWith(_fileExt))
            {
                return;
            }

            var results = new List<JSScriptClassPathHint>();
            if (UnityHelper.ResolveScriptRef(_baseDir, filePath, results))
            {
                foreach (var result in results)
                {
                    _scriptClassPaths.Add(result.ToClassPath(), result);
                }
            }
        }

        public bool Search(string pattern, JSScriptClassType classType, List<JSScriptClassPathHint> results)
        {
            foreach (var pair in _scriptClassPaths)
            {
                //TODO: need optimization
                if (pair.Value.classType == classType && pair.Key.Contains(pattern))
                {
                    results.Add(pair.Value);
                }
            }

            return true;
        }

        public void Start()
        {
            if (_fsw != null)
            {
                return;
            }

            _fsw = new FileSystemWatcher(_baseDir, $"*{_fileExt}");
            _fsw.IncludeSubdirectories = true;
            _fsw.Changed += OnChanged;
            _fsw.Created += OnCreated;
            _fsw.Deleted += OnDeleted;
            _fsw.EnableRaisingEvents = true;
            RefreshAll();
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
#endif
