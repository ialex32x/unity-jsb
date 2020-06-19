using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

namespace WebSockets
{
    using QuickJS;
    using QuickJS.Native;
    using QuickJS.Binding;

    /*
constructor:
    new WebSocket(url, [protocol]);
        url: 要连接的URL
        protocol: 一个协议字符串或者一个包含协议字符串的数组。
property:
    binaryType 
    bufferedAmount
    protocol
    url
    readyState
        0 (WebSocket.CONNECTING)
            正在链接中
        1 (WebSocket.OPEN)
            已经链接并且可以通讯
        2 (WebSocket.CLOSING)
            连接正在关闭
        3 (WebSocket.CLOSED)
            连接已关闭或者没有链接成功 
    onopen(event)
    onmessage(event)
    onerror(event)
    onclose(event)
event:
    'message'
method: 
    send
    close
    + addEventListener('message', func)
    */
    public class WebSocket : Values, IScriptFinalize
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

        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        private static JSValue _js_constructor(JSContext ctx, JSValue new_target, int argc, JSValue[] argv, int magic)
        {
            try
            {
                if (argc < 1 || !argv[0].IsString())
                {
                    throw new ParameterException("url", typeof(string), 0);
                }
                if (argc > 1 && !argv[1].IsString() && JSApi.JS_IsArray(ctx, argv[1]) != 1)
                {
                    throw new ParameterException("protocol", typeof(string), 1);
                }
                var url = JSApi.GetString(ctx, argv[1]);
                // var protocols = new List<string>();
                var o = new WebSocket(url, null);
                var val = NewBridgeClassObject(ctx, new_target, o, magic);
                return val;
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        private static JSValue _js_close(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            try
            {
                WebSocket self;
                if (!js_get_classvalue(ctx, this_obj, out self))
                {
                    throw new ThisBoundException();
                }
                self.SetClose();
                return JSApi.JS_UNDEFINED;
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        private WebSocket(string url, List<string> protocols)
        {
            //TODO: resolve and connect
        }

        public void OnJSFinalize()
        {
            //TODO: on js value finalized
        }

        public static void Bind(TypeRegister register)
        {
            var ns = register.CreateNamespace();
            var cls = ns.CreateClass("WebSocket", typeof(WebSocket), _js_constructor);
            cls.AddMethod(false, "close", _js_close);
            // cls.AddMethod(false, "send", _js_send);
            cls.Close();
            ns.Close();
        }
    }
}
