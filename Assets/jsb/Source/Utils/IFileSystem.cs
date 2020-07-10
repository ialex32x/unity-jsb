using System;
using System.Collections.Generic;

namespace QuickJS.Utils
{
    using UnityEngine;

    public interface IFileSystem
    {
        bool Exists(string path);

        /// <summary>
        /// 读取文件内容, 无法读取时返回 null (不应该抛异常)
        /// </summary>
        byte[] ReadAllBytes(string path);

        /// <summary>
        /// 读取文件内容, 无法读取时返回 null (不应该抛异常)
        /// </summary>
        string ReadAllText(string path);
    }
}
