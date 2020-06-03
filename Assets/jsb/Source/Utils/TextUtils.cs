using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace QuickJS.Utils
{
    public static class TextUtils
    {
        // 剔除行注释
        public static string NormalizeJson(string json)
        {
            var outstr = new StringBuilder();
            var state = 0;
            for (int i = 0; i < json.Length; i++)
            {
                if (state == 0)
                {
                    if (json[i] == '/')
                    {
                        state = 1;
                        continue;
                    }
                }
                else if (state == 1)
                {
                    if (json[i] == '/')
                    {
                        state = 2;
                        continue;
                    }
                    state = 0;
                    outstr.Append('/');
                }
                else if (state == 2)
                {
                    if (json[i] != '\n')
                    {
                        continue;
                    }
                    state = 0;
                }
                outstr.Append(json[i]);
            }
            return outstr.ToString();
        }

        public static byte[] GetNullTerminatedBytes(string str)
        {
            if (str.EndsWith("\0"))
            {
                return Encoding.UTF8.GetBytes(str);
            }
            var count = Encoding.UTF8.GetByteCount(str);
            var bytes = new byte[count + 1];
            Encoding.UTF8.GetBytes(str, 0, str.Length, bytes, 0);
            return bytes;
        }

        public static byte[] GetNullTerminatedBytes(byte[] str)
        {
            var count = str.Length;
            if (str[count-1] == 0)
            {
                return str;
            }
            var bytes = new byte[count + 1];
            Array.Copy(str, 0, bytes, 0, count);
            return bytes;
        }
    }
}
