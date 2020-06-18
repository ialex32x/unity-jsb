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
            switch (reason)
            {
                case lws_callback_reasons.LWS_CALLBACK_OPENSSL_LOAD_EXTRA_CLIENT_VERIFY_CERTS:
                    {
                        return 0;
                    }
                case lws_callback_reasons.LWS_CALLBACK_CLIENT_ESTABLISHED:
                    {
                        websocket._wsi = wsi;
                        websocket.OnConnect(); // _on_connect(websocket, lws_get_protocol(wsi)->name);
                        return 0;
                    }
                case lws_callback_reasons.LWS_CALLBACK_CLIENT_CONNECTION_ERROR:
                    {
                        websocket.OnError();
                        websocket.Destroy();
                        return -1;
                    }
                case lws_callback_reasons.LWS_CALLBACK_WS_PEER_INITIATED_CLOSE:
                    {
                        websocket.OnCloseRequest(@in, len);
                        return 0;
                    }
                case lws_callback_reasons.LWS_CALLBACK_CLIENT_CLOSED:
                    {
                        websocket.SetClose(); // _duk_lws_close(websocket);
                        websocket.Destroy();
                        websocket.OnClose();
                        return 0;
                    }
                case lws_callback_reasons.LWS_CALLBACK_CLIENT_RECEIVE:
                    {
                        return websocket.OnReceive(@in, len);
                    }
                case lws_callback_reasons.LWS_CALLBACK_CLIENT_WRITEABLE:
                    {
                        if (websocket._is_closing)
                        {
                            WSApi.lws_close_reason(wsi, lws_close_status.LWS_CLOSE_STATUS_NORMAL, "");
                            return -1;
                        }
                        websocket.OnWrite();
                        return 0;
                    }
                default:
                    {
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
            byte* ptr = (byte*)@in;
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

        private void SetClose()
        {
            if (_wsi.IsValid())
            {
                _is_closing = true;
                WSApi.lws_callback_on_writable(_wsi);
                _wsi = lws.Null;
            }
        }

        // _duk_lws_destroy
        private void Destroy()
        {
            if (_is_context_destroyed)
            {
                return;
            }

            if (_is_polling)
            {
                _is_context_destroying = true;
                return;
            }

            _is_context_destroyed = true;
            if (_context.IsValid())
            {
                WSApi.lws_context_destroy(_context);
                _context = lws_context.Null;
            }

            //TODO: free all pending buffer
        }

        private void OnClose()
        {
            //TODO: dispatch 'close' event
        }

        private void OnWrite()
        {
            //TODO: dequeue pending buf, write to wsi (lws_write)
            // WSApi.lws_write(_wsi, &buf[LWS_PRE], len, protocol);
            // if (pending queue not empty)
            // {
            //     WSApi.lws_callback_on_writable(_wsi);
            // }
        }

        // 已建立连接
        private void OnConnect()
        {
            //TODO: dispatch 'open' event
        }

        private void OnError()
        {
            //TODO: dispatch 'event' event
            // _duk_lws_destroy(websocket);
        }

        private void OnCloseRequest(IntPtr @in, size_t len)
        {
            int code;
            string reason;
            if (TryParseReason(@in, len, out code, out reason))
            {
                //TODO: dispatch 'close request' event
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
            // return -1;

            //TODO: copy recv buf

            if (WSApi.lws_is_final_fragment(_wsi) == 1)
            {
                var is_binary = WSApi.lws_frame_is_binary(_wsi);
                // dispatch recv buf (data) event
            }

            return 0;
        }

        private void _js_send()
        {
            //TODO: send
        }

        private void _js_close()
        {
            SetClose();
        }

        //TODO: make it auto update
        private int _js_poll()
        {
            if (!_context.IsValid())
            {
                return -1;
            }

            _is_polling = true;
            do
            {
                _is_servicing = false;
                WSApi.lws_service(_context, 0);
            } while (_is_servicing);
            _is_polling = false;

            if (_is_context_destroying)
            {
                Destroy();
            }

            return 0;
        }

        private int _js_constructor()
        {
            // JSB_NewBridgeClassObject(..., TYPE.StrictObjectRef);
            return 0;
        }

        private int _js_finalizer()
        {
            return 0;
        }

        public void Bind(QuickJS.Binding.TypeRegister register)
        {
            var ns = register.CreateNamespace();
            // ns.CreateClass()
            ns.Close();
        }
    }
}
