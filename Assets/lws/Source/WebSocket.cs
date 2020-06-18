using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

namespace WebSockets
{
    using size_t = QuickJS.Native.size_t;
    
    public class WebSocket
    {
        private static List<WebSocket> _websockets = new List<WebSocket>();

        private static WebSocket GetWebSocket(lws_context context)
        {
            var count = _websockets.Count;
            for (var i = 0; i < count; i++)
            {
                var websocket = _websockets[i];
                if (websocket._context == context)
                {
                    return websocket;
                }
            }

            return null;
        }

        private lws _wsi;
        private lws_context _context;
        private bool _is_closing;
        private bool _is_servicing;
        private bool _is_polling;
        private bool _is_context_destroying;
        private bool _is_context_destroyed;
        
        [MonoPInvokeCallback(typeof(lws_callback_function))]
        public static int _callback(lws wsi, lws_callback_reasons reason, IntPtr user, IntPtr @in, size_t len)
        {
            var context = WSApi.lws_get_context(wsi);
            var websocket = GetWebSocket(context);
            if (websocket == null)
            {
                return -1;
            }
            
            websocket._is_servicing = true;
            switch (reason) {
                case lws_callback_reasons.LWS_CALLBACK_OPENSSL_LOAD_EXTRA_CLIENT_VERIFY_CERTS: {
                    return 0;
                } 
                case lws_callback_reasons.LWS_CALLBACK_CLIENT_ESTABLISHED: {
                    websocket._wsi = wsi;
                    websocket.OnConnect(); // _on_connect(websocket, lws_get_protocol(wsi)->name);
                    return 0;
                } 
                case lws_callback_reasons.LWS_CALLBACK_CLIENT_CONNECTION_ERROR: {
                    websocket.OnError();
                    return -1;
                } 
                case lws_callback_reasons.LWS_CALLBACK_WS_PEER_INITIATED_CLOSE: {
                    websocket.OnCloseRequest(@in, len);
                    return 0;
                } 
                case lws_callback_reasons.LWS_CALLBACK_CLIENT_CLOSED: {
                    _duk_lws_close(websocket);
                    _duk_lws_destroy(websocket);
                    _on_disconnect(websocket);
                    return 0;
                } 
                case lws_callback_reasons.LWS_CALLBACK_CLIENT_RECEIVE: {
                    return websocket.OnReceive(@in, len);
                } 
                case lws_callback_reasons.LWS_CALLBACK_CLIENT_WRITEABLE: {
                    if (websocket._is_closing) {
                        WSApi.lws_close_reason(wsi, lws_close_status.LWS_CLOSE_STATUS_NORMAL, "");
                        return -1;
                    }
                    _lws_send(websocket, wsi);
                    return 0;
                } 
                default:  {
                    return 0;
                }
            }
        }

        private static unsafe bool TryParseReason(IntPtr @in, size_t len, out int code, out string reason)
        {
            if (len < 2)
            {
                code = 0;
                reason = null;
                return false;
            }
            byte* ptr = (byte*) @in;
            code = ptr[0] << 8 | ptr[1];
            try
            {
                reason = Encoding.UTF8.GetString(&ptr[2], len - 2);
            }
            catch (Exception)
            {
                reason = null;
            }

            return true;
        }

        // 已建立连接
        private void OnConnect()
        {
            //TODO: dispatch 'open'
        }

        private void OnError()
        {
            // _duk_lws_destroy(websocket);
        }

        private void OnCloseRequest(IntPtr @in, size_t len)
        {
            int code;
            string reason;
            if (TryParseReason(@in, len, out code, out reason))
            {
                //TODO: dispatch 'close request'
            }
        }

        // return -1 if error
        private int OnReceive(IntPtr @in, size_t len)
        {
            if (WSApi.lws_is_first_fragment(_wsi) == 1)
            {
                // init receive buffer .size = 0
            }
            //TODO: check recv buf size
            
            //TODO: copy recv buf

            if (WSApi.lws_is_final_fragment(_wsi))
            {
                
            }
            
            return 0;
        }

        public void Poll()
        {
        }
    }
}
