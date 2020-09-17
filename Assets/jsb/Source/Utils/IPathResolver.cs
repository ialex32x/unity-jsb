using System;
using System.Collections.Generic;

namespace QuickJS.Utils
{
    public interface IPathResolver
    {
        void AddSearchPath(string path);
        bool ResolvePath(IFileSystem fileSystem, string fileName, out string resolvedPath);
    }

    [Serializable]
    public class PackageConfig
    {
        public string main;
    }

    public class SubPathResolver : IPathResolver
    {
        private IPathResolver _resolver;

        public SubPathResolver(IPathResolver resolver)
        {
            _resolver = resolver;
        }

        public void AddSearchPath(string path)
        {
            throw new NotSupportedException();
        }

        public bool ResolvePath(IFileSystem fileSystem, string fileName, out string resolvedPath)
        {
            return _resolver.ResolvePath(fileSystem, fileName, out resolvedPath);
        }
    }

    public class PathResolver : IPathResolver
    {
        private JsonConverter _jsonConv;
        private List<string> _searchPaths = new List<string>();

        public PathResolver(JsonConverter jsonConv)
        {
            _jsonConv = jsonConv;
        }

        public void AddSearchPath(string path)
        {
            if (!_searchPaths.Contains(path))
            {
                _searchPaths.Add(path);
            }
        }

        public bool ResolvePath(IFileSystem fileSystem, string fileName, out string resolvedPath)
        {
            string searchPath;
            if (_ResolvePath(fileSystem, fileName, out searchPath, out resolvedPath))
            {
                return true;
            }

            // var extIndex = fileName.LastIndexOf('.');
            // var slashIndex = fileName.LastIndexOf('/');
            // if (extIndex < 0 || slashIndex > extIndex)
            {
                // try resolve bytecode file
                if (_ResolvePath(fileSystem, fileName + ".js.bytes", out searchPath, out resolvedPath))
                {
                    return true;
                }

                if (_ResolvePath(fileSystem, fileName + ".js", out searchPath, out resolvedPath))
                {
                    return true;
                }

                if (_ResolvePath(fileSystem, PathUtils.Combine(fileName, "index.js"), out searchPath, out resolvedPath))
                {
                    return true;
                }

                if (_ResolvePath(fileSystem, PathUtils.Combine(fileName, "package.json"), out searchPath, out resolvedPath))
                {
                    var packageData = fileSystem.ReadAllText(resolvedPath);
                    if (packageData != null)
                    {
                        var packageConfig = _jsonConv.Deserialize(packageData, typeof(PackageConfig)) as PackageConfig;
                        if (packageConfig != null)
                        {
                            var main = PathUtils.Combine(searchPath, fileName, packageConfig.main);
                            if (!main.EndsWith(".js"))
                            {
                                main += ".js";
                            }
                            main = PathUtils.ExtractPath(main, '/');
                            if (fileSystem.Exists(main))
                            {
                                resolvedPath = main;
                                return true;
                            }
                        }
                    }
                }
            }

            resolvedPath = null;
            return false;
        }

        private bool _ResolvePath(IFileSystem fileSystem, string fileName, out string searchPath, out string resolvedPath)
        {
            if (fileSystem.Exists(fileName))
            {
                resolvedPath = fileName;
                searchPath = "";
                return true;
            }

            if (!fileName.StartsWith("/"))
            {
                for (int i = 0, count = _searchPaths.Count; i < count; i++)
                {
                    var path = _searchPaths[i];
                    var vpath = PathUtils.Combine(path, fileName);
                    if (fileSystem.Exists(vpath))
                    {
                        searchPath = path;
                        resolvedPath = vpath;
                        return true;
                    }
                }
            }

            searchPath = null;
            resolvedPath = null;
            return false;
        }
    }
}
