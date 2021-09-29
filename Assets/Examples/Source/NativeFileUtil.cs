using System;
using System.IO;
using System.Collections.Generic;

namespace Example
{
    using System.Runtime.InteropServices;
    using size_t = QuickJS.Native.size_t;

    // ! ONLY FOR DEVELOPMENT !
    /// <summary>
    /// read content of file without io-exception of sharing violation (if you use System.IO.File)
    /// </summary>
    [QuickJS.JSType]
    public static class NativeFileUtil
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr fopen(string filename, string mode);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern unsafe size_t fread(byte* buffer, size_t elemSize, size_t elemCount, IntPtr fd);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern Int32 fclose(IntPtr fd);

        public static string ReadAllText(string path)
        {
            return System.Text.Encoding.UTF8.GetString(ReadAllBytes(path));
        }

        private static unsafe byte[] _ReadAllBytes(string path)
        {
            var fd = IntPtr.Zero;
            try
            {
                var size = 4096;
                var buffer = new byte[size];
                var stream = new MemoryStream();
                fd = fopen(path, "rb");
                if (fd == IntPtr.Zero)
                {
                    throw new FileLoadException("fopen failed", path);
                }

                while (true)
                {
                    fixed (byte* p = buffer)
                    {
                        var rd = fread(p, 1, size, fd);
                        if (rd <= 0)
                        {
                            break;
                        }

                        stream.Write(buffer, 0, rd);
                    }
                }

                return stream.ToArray();
            }
            finally
            {
                fclose(fd);
            }
        }

        public static unsafe byte[] ReadAllBytes(string path)
        {
            try
            {
                return _ReadAllBytes(path);
            }
            catch (Exception)
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    return bytes;
                }
            }
        }
    }
}
