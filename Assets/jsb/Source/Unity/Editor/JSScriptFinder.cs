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
        private bool _isRefreshing;

        // classPath => JSScriptClassPathHint
        private Dictionary<string, JSScriptClassPathHint> _scriptClassPaths = new Dictionary<string, JSScriptClassPathHint>();
        private Dictionary<string, List<string>> _fullPathToClassPath = new Dictionary<string, List<string>>();

        private bool _cachedChangesDirty = false;
        // fullPath => WatcherChangeType
        private Dictionary<string, WatcherChangeTypes> _cachedChanges = new Dictionary<string, WatcherChangeTypes>();

        public JSScriptFinder(string baseDir, string fileExt)
        {
            _baseDir = baseDir;
            _fileExt = fileExt;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            lock (_cachedChanges)
            {
                _cachedChanges[e.FullPath] = WatcherChangeTypes.Changed;
                _cachedChangesDirty = true;
                // UnityEngine.Debug.Log($"changed {e.FullPath}");
            }
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            lock (_cachedChanges)
            {
                _cachedChanges[e.FullPath] = WatcherChangeTypes.Created;
                _cachedChangesDirty = true;
                // UnityEngine.Debug.Log($"created {e.FullPath}");
            }
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            lock (_cachedChanges)
            {
                _cachedChanges[e.FullPath] = WatcherChangeTypes.Deleted;
                _cachedChangesDirty = true;
                // UnityEngine.Debug.Log($"deleted {e.FullPath}");
            }
        }

        public void RefreshAll()
        {
            _isRefreshing = true;
            _scriptClassPaths.Clear();
            SearchDirectory(_baseDir);
            _isRefreshing = false;
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
            string normalizedPath;
            if (UnityHelper.ResolveScriptRef(_baseDir, filePath, out normalizedPath, results))
            {
                List<string> list;
                if (_fullPathToClassPath.TryGetValue(normalizedPath, out list))
                {
                    foreach (var item in list)
                    {
                        _scriptClassPaths.Remove(item);
                    }
                    list.Clear();
                }
                else
                {
                    list = _fullPathToClassPath[normalizedPath] = new List<string>();
                }

                foreach (var result in results)
                {
                    var classPath = result.ToClassPath();

                    list.Add(classPath);
                    _scriptClassPaths[classPath] = result;
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
            UnityEditor.EditorApplication.update += OnUpdate;
            RefreshAll();
        }

        private void OnUpdate()
        {
            if (_isRefreshing)
            {
                return;
            }

            lock (_cachedChanges)
            {
                if (_cachedChangesDirty)
                {
                    foreach (var pair in _cachedChanges)
                    {
                        var fullPath = pair.Key;
                        var changeType = pair.Value;

                        if (changeType == WatcherChangeTypes.Deleted)
                        {
                            List<string> classPathList;
                            var lowerCasedFullPath = fullPath.ToLower();
                            if (_fullPathToClassPath.TryGetValue(lowerCasedFullPath, out classPathList))
                            {
                                _fullPathToClassPath.Remove(lowerCasedFullPath);
                                foreach (var item in classPathList)
                                {
                                    _scriptClassPaths.Remove(item);
                                }
                            }
                        }
                        else
                        {
                            ParseFile(fullPath);
                        }
                    }
                    _cachedChanges.Clear();
                    _cachedChangesDirty = false;
                }
            }
        }

        public void Stop()
        {
            if (_fsw == null)
            {
                return;
            }

            UnityEditor.EditorApplication.update -= OnUpdate;
            _fsw.Dispose();
            _fsw = null;
        }
    }
}
#endif
