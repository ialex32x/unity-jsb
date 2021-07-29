using System;
using System.Collections.Generic;

namespace QuickJS.Utils
{
    public interface IFileSystem
    {
        bool Exists(string path);

        string GetFullPath(string path);

        /// <summary>
        /// the content of file, return null if any error occurs (do not throw exception)
        /// </summary>
        byte[] ReadAllBytes(string path);
    }

    public class DefaultFileSystem : IFileSystem
    {
        private IScriptLogger _logger;

        public DefaultFileSystem(IScriptLogger logger)
        {
            _logger = logger;
        }

        public bool Exists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public string GetFullPath(string path)
        {
            return System.IO.Path.GetFullPath(path);
        }

        public byte[] ReadAllBytes(string path)
        {
            try
            {
                return System.IO.File.ReadAllBytes(path);
            }
            catch (Exception exception)
            {
                if (_logger != null)
                {
                    _logger.Write(LogLevel.Error, "{0}: {1}\n{2}", path, exception.Message, exception.StackTrace);
                }
                return null;
            }
        }
    }
}
