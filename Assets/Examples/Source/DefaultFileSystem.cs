using System;
using QuickJS;
using QuickJS.Binding;
using QuickJS.Utils;
using QuickJS.IO;
using System.Net;
using System.IO;
using System.Text;

namespace Example
{
    using UnityEngine;

    public class ResourcesFileSystem : IFileSystem
    {
        private IScriptLogger _logger;

        public ResourcesFileSystem(IScriptLogger logger)
        {
            _logger = logger;
        }

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
                var asset = Resources.Load<TextAsset>(path);
                return asset.text;
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

    public class HttpFileSystem : IFileSystem
    {
        private string _url;
        private IScriptLogger _logger;


        public HttpFileSystem(IScriptLogger logger, string baseUrl)
        {
            _url = baseUrl;
            _logger = logger;
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
            if (!path.EndsWith(".js") && !path.EndsWith(".json") && !path.EndsWith(".jsonc"))
            {
                return false;
            }
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
                var asset = GetRemote(path);
                return asset;
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
