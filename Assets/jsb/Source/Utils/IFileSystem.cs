using System;
using System.Collections.Generic;

namespace QuickJS.Utils
{
    public interface IFileSystem
    {
        bool Exists(string path);

        string GetFullPath(string path);

        /// <summary>
        /// 读取文件内容, 无法读取时返回 null (不应该抛异常)
        /// </summary>
        byte[] ReadAllBytes(string path);

        /// <summary>
        /// 读取文件内容, 无法读取时返回 null (不应该抛异常)
        /// </summary>
        string ReadAllText(string path);
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

        public string ReadAllText(string path)
        {
            try
            {
                return System.IO.File.ReadAllText(path);
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
