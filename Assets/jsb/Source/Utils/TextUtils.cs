using System;
using System.Collections.Generic;
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

    }
}
