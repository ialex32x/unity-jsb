using System;
using System.Collections.Generic;

namespace QuickJS.Utils
{
    using UnityEngine;

    public interface IFileResolver
    {
        void AddSearchPath(string path);

        byte[] ReadAllBytes(string filename);
    }

    public class FileResolver : IFileResolver
    {
        private List<string> _searchPaths = new List<string>();
        private IFileSystem _fileSystem;

        public FileResolver(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void AddSearchPath(string path)
        {
            if (!_searchPaths.Contains(path))
            {
                _searchPaths.Add(path);
            }
        }

        public byte[] ReadAllBytes(string filename)
        {
            if (_fileSystem.Exists(filename))
            {
                return _fileSystem.ReadAllBytes(filename);
            }
            for (int i = 0, count = _searchPaths.Count; i < count; i++)
            {
                var path = _searchPaths[i];
                var vpath = PathUtils.Combine(path, filename);
                if (_fileSystem.Exists(vpath))
                {
                    return _fileSystem.ReadAllBytes(vpath);
                }
            }
            return null;
        }
    }
}
