using System;
using QuickJS;
using QuickJS.Binding;
using QuickJS.Utils;
using QuickJS.IO;
using System.Net;
using System.IO;
using System.Text;

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

    public class HttpFileSystem : IFileSystem
    {
        private string _url;

        public HttpFileSystem(string baseUrl)
        {
            _url = baseUrl;
        }

        private string GetRemote(string path)
        {
            try
            {
                var uri = _url.EndsWith("/") ? _url + path : $"{_url}/{path}";
                var request = WebRequest.CreateHttp(uri);
                var response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var reader = new StreamReader(response.GetResponseStream());
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
            }
            return null;
        }

        public bool Exists(string path)
        {
            var asset = GetRemote(path);
            return asset != null;
        }

        public byte[] ReadAllBytes(string path)
        {
            try
            {
                var asset = GetRemote(path);
                return Encoding.UTF8.GetBytes(asset);
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
                var asset = GetRemote(path);
                return asset;
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
