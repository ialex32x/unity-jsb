using System;
using System.Runtime.InteropServices;
using System.Text;

namespace QuickJS.Native
{
    public partial class JSApi
    {
        public static string GetString(IntPtr ptr, int len)
        {
            var str = Marshal.PtrToStringAnsi(ptr, len);
            if (str == null)
            {
                var buffer = new byte[len];
                Marshal.Copy(ptr, buffer, 0, len);
                return Encoding.UTF8.GetString(buffer);
            }

            return str;
        }
    }
}