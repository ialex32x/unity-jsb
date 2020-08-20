using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections;
using System.Collections.Generic;

namespace QuickJS.Extra
{
    using UnityEngine;
    using QuickJS;
    using QuickJS.IO;
    using QuickJS.Native;
    using QuickJS.Binding;

    public class XMLHttpRequest : Values, IScriptFinalize
    {
        private enum ReadyState
        {
            /// <summary>
            /// 代理被创建，但尚未调用 open() 方法。
            /// </summary>
            UNSENT = 0,
            /// <summary>
            /// open() 方法已经被调用。
            /// </summary>
            OPENED = 1,
            /// <summary>
            /// send() 方法已经被调用，并且头部和状态已经可获得。 
            /// </summary>
            HEADERS_RECEIVED = 2,
            /// <summary>
            /// 下载中； responseText 属性已经包含部分数据。
            /// </summary>
            LOADING = 3,
            /// <summary>
            /// 下载操作已完成。
            /// </summary>
            DONE = 4,
        }

        private JSValue _jsThis; // dangeous reference holder (no ref count)
        private JSContext _jsContext; // dangeous reference holder
        private JSValue _onreadychange; 

        private HttpWebRequest _request;
        private ReadyState _state;
        private HttpStatusCode _code;
        private string _reseponseText;

        private void _Transfer(JSContext ctx, JSValue value)
        {
            _jsContext = ctx;
            _jsThis = value;
            _onreadychange = JSApi.JS_UNDEFINED;

            var context = ScriptEngine.GetContext(ctx);
            var runtime = context.GetRuntime();

            runtime.OnDestroy += Destroy;
        }

        private void Destroy(ScriptRuntime runtime)
        {
            Destroy();
        }

        private void Destroy()
        {
            var callback = _onreadychange;
            if (!callback.IsNullish())
            {
                _onreadychange = JSApi.JS_UNDEFINED;
                JSApi.JS_FreeValue(_jsContext, callback);
            }
            _state = ReadyState.UNSENT;
            _code = 0;
            _request = null;
        }

        public void OnJSFinalize()
        {
            var callback = _onreadychange;
            if (!callback.IsNullish())
            {
                _onreadychange = JSApi.JS_UNDEFINED;
                JSApi.JS_FreeValue(_jsContext, callback);
            }
            _jsContext = JSContext.Null;
            _jsThis = JSApi.JS_UNDEFINED;
            Destroy();
        }

        private void OnReadyStateChange()
        {
            if (!_jsContext.IsValid() || JSApi.JS_IsFunction(_jsContext, _onreadychange) != 1)
            {
                return;
            }
            var ret = JSApi.JS_Call(_jsContext, _onreadychange, JSApi.JS_UNDEFINED);
            JSApi.JS_FreeValue(_jsContext, ret);
        }

        private void Open(string requestUriString, string method, bool bAsync)
        {
            if (_state != ReadyState.UNSENT)
            {
                throw new InvalidOperationException();
            }
            var uri = new Uri(requestUriString);
            _state = ReadyState.OPENED;
            _request = WebRequest.CreateHttp(uri);
            _request.Method = method;
            OnReadyStateChange();
        }

        private async void Send()
        {
            if (_state != ReadyState.OPENED)
            {
                throw new InvalidOperationException();
            }
            _state = ReadyState.LOADING;
            OnReadyStateChange();
            if (_state != ReadyState.LOADING)
            {
                return;
            }
            var rsp = await _request.GetResponseAsync() as HttpWebResponse;
            if (_state != ReadyState.LOADING)
            {
                return;
            }
            var reader = new StreamReader(rsp.GetResponseStream());
            _reseponseText = await reader.ReadToEndAsync();
            if (_state != ReadyState.LOADING)
            {
                return;
            }
            _state = ReadyState.DONE;
            _code = rsp.StatusCode;
            OnReadyStateChange();
        }

        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        private static JSValue js_constructor(JSContext ctx, JSValue new_target, int argc, JSValue[] argv, int magic)
        {
            var o = new XMLHttpRequest();
            var val = NewBridgeClassObject(ctx, new_target, o, magic);
            o._Transfer(ctx, val);
            return val;
        }

        // xhr.open(method: string, url: string, async: boolean = true);
        [MonoPInvokeCallback(typeof(JSCFunction))]
        private static JSValue js_open(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            try
            {
                XMLHttpRequest self;
                if (!js_get_classvalue(ctx, this_obj, out self))
                {
                    throw new ThisBoundException();
                }
                if (argc < 2)
                {
                    throw new InvalidDataException();
                }
                if (!argv[0].IsString() || !argv[0].IsString())
                {
                    throw new InvalidDataException();
                }
                var method = JSApi.GetString(ctx, argv[0]);
                var uri = JSApi.GetString(ctx, argv[1]);
                self.Open(uri, method, true);
                return JSApi.JS_UNDEFINED;
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        // xhr.timeout = 2000; # unsigned long, milliseconds
        [MonoPInvokeCallback(typeof(JSGetterCFunction))]
        private static JSValue js_get_timeout(JSContext ctx, JSValue this_obj)
        {
            try
            {
                XMLHttpRequest self;
                if (!js_get_classvalue(ctx, this_obj, out self))
                {
                    throw new ThisBoundException();
                }

                if (self._state != ReadyState.OPENED)
                {
                    throw new InvalidOperationException();
                }

                return JSApi.JS_NewInt32(ctx, self._request.Timeout);
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        [MonoPInvokeCallback(typeof(JSSetterCFunction))]
        private static JSValue js_set_timeout(JSContext ctx, JSValue this_obj, JSValue val)
        {
            try
            {
                XMLHttpRequest self;
                if (!js_get_classvalue(ctx, this_obj, out self))
                {
                    throw new ThisBoundException();
                }
                if (!val.IsNumber())
                {
                    throw new InvalidDataException();
                }

                if (self._state != ReadyState.OPENED)
                {
                    throw new InvalidOperationException();
                }
                int timeout;
                JSApi.JS_ToInt32(ctx, out timeout, val);
                self._request.Timeout = timeout;
                return JSApi.JS_UNDEFINED;
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        // xhr.send(data?: FormData); 
        [MonoPInvokeCallback(typeof(JSCFunction))]
        private static JSValue js_send(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            try
            {
                XMLHttpRequest self;
                if (!js_get_classvalue(ctx, this_obj, out self))
                {
                    throw new ThisBoundException();
                }
                self.Send();
                return JSApi.JS_UNDEFINED;
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        // xhr.onreadystatechange = function () { ... };
        [MonoPInvokeCallback(typeof(JSGetterCFunction))]
        private static JSValue js_get_onreadystatechange(JSContext ctx, JSValue this_obj)
        {
            try
            {
                XMLHttpRequest self;
                if (!js_get_classvalue(ctx, this_obj, out self))
                {
                    throw new ThisBoundException();
                }
                return JSApi.JS_DupValue(ctx, self._onreadychange);
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        [MonoPInvokeCallback(typeof(JSSetterCFunction))]
        private static JSValue js_set_onreadystatechange(JSContext ctx, JSValue this_obj, JSValue val)
        {
            try
            {
                XMLHttpRequest self;
                if (!js_get_classvalue(ctx, this_obj, out self))
                {
                    throw new ThisBoundException();
                }
                JSApi.JS_FreeValue(ctx, self._onreadychange);
                self._onreadychange = JSApi.JS_DupValue(ctx, val);
                return JSApi.JS_UNDEFINED;
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        // xhr.readyState
        // 0 	UNSENT 	代理被创建，但尚未调用 open() 方法。
        // 1 	OPENED 	open() 方法已经被调用。
        // 2 	HEADERS_RECEIVED 	send() 方法已经被调用，并且头部和状态已经可获得。
        // 3 	LOADING 	下载中； responseText 属性已经包含部分数据。
        // 4 	DONE 	下载操作已完成。
        [MonoPInvokeCallback(typeof(JSGetterCFunction))]
        private static JSValue js_get_readyState(JSContext ctx, JSValue this_obj)
        {
            try
            {
                XMLHttpRequest self;
                if (!js_get_classvalue(ctx, this_obj, out self))
                {
                    throw new ThisBoundException();
                }
                return JSApi.JS_NewInt32(ctx, (int)self._state);
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        // xhr.status 
        [MonoPInvokeCallback(typeof(JSGetterCFunction))]
        private static JSValue js_get_status(JSContext ctx, JSValue this_obj)
        {
            try
            {
                XMLHttpRequest self;
                if (!js_get_classvalue(ctx, this_obj, out self))
                {
                    throw new ThisBoundException();
                }
                return JSApi.JSB_NewInt64(ctx, (int)self._code);
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        // xhr.responseText
        [MonoPInvokeCallback(typeof(JSGetterCFunction))]
        private static JSValue js_get_responseText(JSContext ctx, JSValue this_obj)
        {
            try
            {
                XMLHttpRequest self;
                if (!js_get_classvalue(ctx, this_obj, out self))
                {
                    throw new ThisBoundException();
                }

                return JSApi.JS_NewString(ctx, self._reseponseText);
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        public static void Bind(TypeRegister register)
        {
            var ns = register.CreateNamespace();
            var cls = ns.CreateClass("XMLHttpRequest", typeof(XMLHttpRequest), js_constructor);
            cls.AddMethod(false, "open", js_open, 2);
            cls.AddMethod(false, "send", js_send, 0);
            cls.AddProperty(false, "readyState", js_get_readyState, null);
            cls.AddProperty(false, "status", js_get_status, null);
            cls.AddProperty(false, "responseText", js_get_responseText, null);
            cls.AddProperty(false, "timeout", js_get_timeout, js_set_timeout);
            cls.AddProperty(false, "onreadystatechange", js_get_onreadystatechange, js_set_onreadystatechange);
            cls.Close();
            ns.Close();
        }
    }
}
