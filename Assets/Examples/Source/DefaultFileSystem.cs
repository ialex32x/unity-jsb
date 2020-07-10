using System;
using QuickJS;
using QuickJS.Binding;
using QuickJS.Utils;
using QuickJS.IO;

namespace jsb
{
    using UnityEngine;

    public class DefaultFileSystem : IFileSystem
    {
        public bool Exists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public byte[] ReadAllBytes(string path)
        {
            try
            {
                return System.IO.File.ReadAllBytes(path);
            }
            catch (Exception exception)
            {
                var logger = ScriptEngine.GetLogger();
                logger.Write(LogLevel.Error, "{0}: {1}\n{2}", path, exception.Message, exception.StackTrace);
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
                var logger = ScriptEngine.GetLogger();
                logger.Write(LogLevel.Error, "{0}: {1}\n{2}", path, exception.Message, exception.StackTrace);
                return null;
            }
        }
    }

    public class ResourcesFileSystem : IFileSystem
    {
        public bool Exists(string path)
        {
            var asset = Resources.Load<TextAsset>(path);
            return asset != null;
        }

        public byte[] ReadAllBytes(string path)
        {
            try
            {
                var asset = Resources.Load<TextAsset>(path);
                return asset.bytes;
            }
            catch (Exception exception)
            {
                var logger = ScriptEngine.GetLogger();
                logger.Write(LogLevel.Error, "{0}: {1}\n{2}", path, exception.Message, exception.StackTrace);
                return null;
            }
        }

        public string ReadAllText(string path)
        {
            try
            {
                var asset = Resources.Load<TextAsset>(path);
                return asset.text;
            }
            catch (Exception exception)
            {
                var logger = ScriptEngine.GetLogger();
                logger.Write(LogLevel.Error, "{0}: {1}\n{2}", path, exception.Message, exception.StackTrace);
                return null;
            }
        }
    }
}
