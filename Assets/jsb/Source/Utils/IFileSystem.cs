using System;
using System.Collections.Generic;

namespace QuickJS.Utils
{
    using UnityEngine;

    public interface IFileSystem
    {
        bool Exists(string path);
        byte[] ReadAllBytes(string path);
    }

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
                Debug.LogError($"{path}: {exception}");
                return null;
            }
        }
    }
}
