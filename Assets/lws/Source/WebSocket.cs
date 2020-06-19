using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using AOT;
using System.Text;

namespace WebSockets
{
    using QuickJS;
    using QuickJS.IO;
    using QuickJS.Native;
    using QuickJS.Binding;

    /*
    constructor:
        new WebSocket(url, [protocol]);
            url: 要连接的URL
            protocol: 一个协议字符串或者一个包含协议字符串的数组。
    property:
        x binaryType 
        bufferedAmount
        x protocol
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
        x 'message'
    method: 
        send
        close
        x addEventListener('message', func)
    */
    public class WebSocket : Values, IScriptFinalize
    {
        private enum ReadyState
        {
            CONNECTING = 0,
            OPEN = 1,
            CLOSING = 2,
            CLOSED = 3,

            _CONSTRUCTED = -1,
            _DNS = -2,
        }
        private struct Packet
        {
            public bool is_binary;
            public ByteBuffer buffer;

            public Packet(bool is_binary, ByteBuffer buffer)
            {
                this.is_binary = is_binary;
                this.buffer = buffer;
            }

            public void Release()
            {
                if (buffer != null)
                {
                    buffer.Release();
                    buffer = null;
                }
            }
        }

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
        private ReadyState _readyState;
        private int _bufferedAmount;
        private bool _is_closing;
        private bool _is_servicing;
        private bool _is_polling;
        private bool _is_context_destroying;
        private bool _is_context_destroyed;
        private Queue<Packet> _pending = new Queue<Packet>();

        private ByteBuffer _buffer;

        private JSValue _jsThis; // dangeous reference holder (no ref count)
        private JSContext _jsContext; // dangeous reference holder

        private string _url;
        private string _protocol;
        private string[] _protocols;

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

        private void SetReadyState(ReadyState readyState)
        {
            _readyState = readyState;
        }

        private void SetClose()
        {
            if (_wsi.IsValid())
            {
                _is_closing = true;
                SetReadyState(ReadyState.CLOSING);
                WSApi.lws_callback_on_writable(_wsi);
                _wsi = lws.Null;
            }
            else
            {
                SetReadyState(ReadyState.CLOSED);
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

            SetReadyState(ReadyState.CLOSED);
            _is_context_destroyed = true;
            if (_context.IsValid())
            {
                WSApi.lws_context_destroy(_context);
                _context = lws_context.Null;
            }

            while (_pending.Count > 0)
            {
                var packet = _pending.Dequeue();
                packet.Release();
            }
            _bufferedAmount = 0;

            if (_buffer != null)
            {
                _buffer.Release();
                _buffer = null;
            }
            var runtime = ScriptEngine.GetRuntime(_jsContext);
            if (runtime != null)
            {
                runtime.OnUpdate -= Update;
            }
        }

        private void OnWrite()
        {
            if (_pending.Count > 0)
            {
                var packet = _pending.Dequeue();
                var protocol = packet.is_binary ? lws_write_protocol.LWS_WRITE_BINARY : lws_write_protocol.LWS_WRITE_TEXT;
                var len = packet.buffer.writerIndex - WSApi.LWS_PRE;

                unsafe
                {
                    fixed (byte* buf = packet.buffer.data)
                    {
                        WSApi.lws_write(_wsi, &buf[WSApi.LWS_PRE], len, protocol);
                    }
                }

                _bufferedAmount -= len;
                packet.Release();
                if (_pending.Count > 0)
                {
                    WSApi.lws_callback_on_writable(_wsi);
                }
            }
        }

        private void Update()
        {
            switch (_readyState)
            {
                case ReadyState.OPEN:
                case ReadyState.CLOSING:
                case ReadyState.CONNECTING:
                    if (!_context.IsValid())
                    {
                        return;
                    }

                    _is_polling = true;
                    do
                    {
                        _is_servicing = false;
                        WSApi.lws_service(_context, 0);
                    } while (_is_servicing);
                    _is_polling = false;
                    break;
                case ReadyState._CONSTRUCTED:
                    Connect();
                    break;
            }

            if (_is_context_destroying)
            {
                Destroy();
            }
        }

        private void OnClose()
        {
            SetReadyState(ReadyState.CLOSED);
            CallScript("onclose", JSApi.JS_UNDEFINED);
        }

        // 已建立连接
        private void OnConnect()
        {
            SetReadyState(ReadyState.OPEN);
            CallScript("onopen", JSApi.JS_UNDEFINED);
        }

        private void OnError()
        {
            CallScript("onerror", JSApi.JS_UNDEFINED);
        }

        private void OnCloseRequest(IntPtr @in, size_t len)
        {
            int code;
            string reason;
            if (TryParseReason(@in, len, out code, out reason))
            {
                var val = JSApi.JS_NewInt32(_jsContext, code);
                CallScript("oncloserequest", val);
                JSApi.JS_FreeValue(_jsContext, val);
            }
        }

        // return -1 if error
        private int OnReceive(IntPtr @in, size_t len)
        {
            if (WSApi.lws_is_first_fragment(_wsi) == 1)
            {
                _buffer.writerIndex = 0;
            }
            _buffer.WriteBytes(@in, len);

            if (WSApi.lws_is_final_fragment(_wsi) == 1)
            {
                var is_binary = WSApi.lws_frame_is_binary(_wsi) == 1;
                if (is_binary)
                {
                    unsafe
                    {
                        fixed (byte* ptr = _buffer.data)
                        {
                            var val = JSApi.JS_NewArrayBufferCopy(_jsContext, ptr, _buffer.writerIndex);
                            CallScript("onmessage", val);
                            JSApi.JS_FreeValue(_jsContext, val);
                        }
                    }
                }
                else
                {
                    unsafe
                    {
                        _buffer.WriteByte(0); // make it null terminated
                        fixed (byte* ptr = _buffer.data)
                        {
                            var val = JSApi.JS_NewString(_jsContext, ptr);
                            CallScript("onmessage", val);
                            JSApi.JS_FreeValue(_jsContext, val);
                        }
                    }
                }
            }

            return 0;
        }

        private void CallScript(string eventName, JSValue eventArg)
        {
            if (eventArg.IsException())
            {
                _jsContext.print_exception();
                return;
            }
            var scriptContext = ScriptEngine.GetContext(_jsContext);
            if (scriptContext != null)
            {
                var eventFunc = JSApi.JS_GetProperty(_jsContext, _jsThis, scriptContext.GetAtom(eventName));
                if (JSApi.JS_IsFunction(_jsContext, eventFunc) != 1)
                {
                    if (eventFunc.IsException())
                    {
                        _jsContext.print_exception();
                    }
                    JSApi.JS_FreeValue(_jsContext, eventFunc);
                    return;
                }

                var rval = JSApi.JS_Call(_jsContext, eventFunc, _jsThis, 1, new JSValue[] { eventArg });
                if (rval.IsException())
                {
                    _jsContext.print_exception();
                }
                JSApi.JS_FreeValue(_jsContext, rval);
                JSApi.JS_FreeValue(_jsContext, eventFunc);
            }
        }

        // buffer: buffer for recv
        private WebSocket(ByteBuffer buffer, string url, List<string> protocols)
        {
            _url = url;
            _buffer = buffer;
            _protocols = protocols != null ? protocols.ToArray() : new string[] { "" };
            SetReadyState(ReadyState._CONSTRUCTED);
        }

        private void _Transfer(JSContext ctx, JSValue value)
        {
            _jsContext = ctx;
            _jsThis = value;
            var context = ScriptEngine.GetContext(ctx);
            var runtime = context.GetRuntime();

            runtime.OnUpdate += Update;
            JSApi.JS_SetProperty(ctx, value, context.GetAtom("onopen"), JSApi.JS_NULL);
            JSApi.JS_SetProperty(ctx, value, context.GetAtom("onclose"), JSApi.JS_NULL);
            JSApi.JS_SetProperty(ctx, value, context.GetAtom("onerror"), JSApi.JS_NULL);
            JSApi.JS_SetProperty(ctx, value, context.GetAtom("onmessage"), JSApi.JS_NULL);
            JSApi.JS_SetProperty(ctx, value, context.GetAtom("url"), JSApi.JS_NewString(ctx, _url));
        }

        private async void Connect()
        {
            if (_readyState != ReadyState._CONSTRUCTED)
            {
                return;
            }
            SetReadyState(ReadyState._DNS);
            var uri = new Uri(_url);
            var ssl_type = uri.Scheme == "ws" ? ulws_ssl_type.ULWS_DEFAULT : ulws_ssl_type.ULWS_USE_SSL_ALLOW_SELFSIGNED;
            var protocol_names = QuickJS.Utils.TextUtils.GetNullTerminatedBytes(string.Join(",", _protocols));
            var path = QuickJS.Utils.TextUtils.GetNullTerminatedBytes(uri.AbsolutePath);
            var host = QuickJS.Utils.TextUtils.GetNullTerminatedBytes(uri.DnsSafeHost);
            var port = uri.Port;
            switch (uri.HostNameType)
            {
                case UriHostNameType.IPv4:
                case UriHostNameType.IPv6:
                    {
                        var address = QuickJS.Utils.TextUtils.GetNullTerminatedBytes(uri.DnsSafeHost);
                        unsafe
                        {
                            fixed (byte* protocol_names_ptr = protocol_names)
                            fixed (byte* host_ptr = host)
                            fixed (byte* address_ptr = address)
                            fixed (byte* path_ptr = path)
                            {
                                WSApi.ulws_connect(_context, protocol_names_ptr, ssl_type, host_ptr, address_ptr, path_ptr, port);
                            }
                        }
                    }
                    break;
                default:
                    {
                        var entry = await Dns.GetHostEntryAsync(uri.DnsSafeHost);
                        if (_readyState != ReadyState._DNS)
                        {
                            // already closed
                            return;
                        }
                        try
                        {
                            var ipAddress = Select(entry.AddressList);
                            var address = QuickJS.Utils.TextUtils.GetNullTerminatedBytes(ipAddress.ToString());
                            unsafe
                            {
                                fixed (byte* protocol_names_ptr = protocol_names)
                                fixed (byte* host_ptr = host)
                                fixed (byte* address_ptr = address)
                                fixed (byte* path_ptr = path)
                                {
                                    WSApi.ulws_connect(_context, protocol_names_ptr, ssl_type, host_ptr, address_ptr, path_ptr, port);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            SetReadyState(ReadyState.CLOSED);
                            OnError();
                        }
                    }
                    break;
            }
        }

        private IPAddress Select(IPAddress[] list)
        {
            for (int i = 0, len = list.Length; i < len; i++)
            {
                var ipAddress = list[i];
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork || i == len - 1)
                {
                    return ipAddress;
                }
            }
            throw new ArgumentOutOfRangeException("no IPAddress available");
        }

        public void OnJSFinalize()
        {
            _jsContext = JSContext.Null;
            _jsThis = JSApi.JS_UNDEFINED;
            Destroy();
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
                var url = JSApi.GetString(ctx, argv[0]);
                var buffer = ScriptEngine.AllocByteBuffer(ctx);
                // var protocols = new List<string>();
                var o = new WebSocket(buffer, url, null);
                var val = NewBridgeClassObject(ctx, new_target, o, magic);
                o._Transfer(ctx, val);
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

        [MonoPInvokeCallback(typeof(JSGetterCFunction))]
        private static JSValue _js_bufferedAmount(JSContext ctx, JSValue this_obj)
        {
            try
            {
                WebSocket self;
                if (!js_get_classvalue(ctx, this_obj, out self))
                {
                    throw new ThisBoundException();
                }
                return JSApi.JS_NewInt32(ctx, self._bufferedAmount);
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        [MonoPInvokeCallback(typeof(JSGetterCFunction))]
        private static JSValue _js_readyState(JSContext ctx, JSValue this_obj)
        {
            try
            {
                WebSocket self;
                if (!js_get_classvalue(ctx, this_obj, out self))
                {
                    throw new ThisBoundException();
                }
                return JSApi.JS_NewInt32(ctx, (int)self._readyState);
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        private static JSValue _js_send(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            try
            {
                WebSocket self;
                if (!js_get_classvalue(ctx, this_obj, out self))
                {
                    throw new ThisBoundException();
                }
                if (argc == 0)
                {
                    throw new ParameterException("data", typeof(string), 0);
                }
                if (argv[0].IsString())
                {
                    // send text data
                    size_t psize;
                    var pointer = JSApi.JS_ToCStringLen(ctx, out psize, argv[0]);
                    if (pointer != IntPtr.Zero && psize > 0)
                    {
                        var buffer = ScriptEngine.AllocByteBuffer(ctx, psize + WSApi.LWS_PRE);
                        if (buffer != null)
                        {
                            buffer.WriteBytes(WSApi.LWS_PRE);
                            buffer.WriteBytes(pointer, psize);
                            self._pending.Enqueue(new Packet(false, buffer));
                            self._bufferedAmount += psize;
                            WSApi.lws_callback_on_writable(self._wsi);
                        }
                        else
                        {
                            JSApi.JS_FreeCString(ctx, pointer);
                            return JSApi.JS_ThrowInternalError(ctx, "buf alloc failed");
                        }
                    }
                    JSApi.JS_FreeCString(ctx, pointer);
                }
                else
                {
                    size_t psize;
                    var pointer = JSApi.JS_GetArrayBuffer(ctx, out psize, argv[0]);
                    if (pointer != IntPtr.Zero && psize > 0)
                    {
                        var buffer = ScriptEngine.AllocByteBuffer(ctx, psize + WSApi.LWS_PRE);
                        if (buffer != null)
                        {
                            buffer.WriteBytes(WSApi.LWS_PRE);
                            buffer.WriteBytes(pointer, psize);
                            self._pending.Enqueue(new Packet(false, buffer));
                            self._bufferedAmount += psize;
                            WSApi.lws_callback_on_writable(self._wsi);
                        }
                        else
                        {
                            return JSApi.JS_ThrowInternalError(ctx, "buf alloc failed");
                        }
                    }
                    else
                    {
                        return JSApi.JS_ThrowInternalError(ctx, "unknown buf type");
                    }
                }

                return JSApi.JS_UNDEFINED;
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        public static void Bind(TypeRegister register)
        {
            var ns = register.CreateNamespace();
            var cls = ns.CreateClass("WebSocket", typeof(WebSocket), _js_constructor);
            cls.AddMethod(false, "close", _js_close);
            cls.AddMethod(false, "send", _js_send);
            cls.AddProperty(false, "readyState", _js_readyState, null);
            cls.AddProperty(false, "bufferedAmount", _js_bufferedAmount, null);
            cls.Close();
            ns.Close();
        }
    }
}
