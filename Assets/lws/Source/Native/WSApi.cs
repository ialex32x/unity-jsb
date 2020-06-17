using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WebSockets
{
    using size_t = QuickJS.Native.size_t;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate int lws_callback_function(lws wsi, lws_callback_reasons reason, IntPtr user, IntPtr @in, size_t len);

    public class WSApi
    {
#if UNITY_IPHONE && !UNITY_EDITOR
	    const string WSDLL = "__Internal";
#else
        const string WSDLL = "libwebsockets";
#endif
        [DllImport(WSDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe lws_context ulws_create(byte* name, IntPtr callback, size_t rx_buffer_size, size_t tx_packet_size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static lws_context ulws_create(string name, lws_callback_function callback, size_t rx_buffer_size, size_t tx_packet_size)
        {
            var fn = Marshal.GetFunctionPointerForDelegate(callback);
            var bytes = QuickJS.Utils.TextUtils.GetNullTerminatedBytes(name);
            unsafe
            {
                fixed (byte* pointer = bytes)
                {
                    return ulws_create(pointer, fn, rx_buffer_size, tx_packet_size);
                }
            }
        }

        [DllImport(WSDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lws_context_destroy(lws_context context);

        [DllImport(WSDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lws_context_destroy(IntPtr context);
    }
}

