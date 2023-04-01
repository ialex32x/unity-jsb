using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuickJS.Utils;

namespace QuickJS.Extra
{
    using UnityEngine;
    using QuickJS;
    using QuickJS.Errors;
    using QuickJS.IO;
    using QuickJS.Native;
    using QuickJS.Binding;

    public class XMLHttpRequest : Values, IDisposable, Utils.IObjectCollectionEntry
    {
        private class ResponseArgs
        {
            public XMLHttpRequest request;
            public string error;
        }

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

        private Utils.ObjectCollection.Handle _handle;
        private JSValue _jsThis; // dangeous reference holder (no ref count)
        private JSContext _jsContext; // dangeous reference holder

        private JSValue _onerror;
        private JSValue _onreadystatechange;

        private ReadyState _state;
        private bool _bAsync;
        private string _method;
        private int _timeout;
        private string _requestUriString;
        private HttpStatusCode _code;
        private string _reseponseText;

        private void _Transfer(JSContext ctx, JSValue value)
        {
            _jsContext = ctx;
            _jsThis = value;
            _onreadystatechange = JSApi.JS_UNDEFINED;
            _onerror = JSApi.JS_UNDEFINED;

            var runtime = ScriptEngine.GetRuntime(ctx);
            runtime.AddManagedObject(this, out _handle);
        }

        #region IObjectCollectionEntry implementation
        public void OnCollectionReleased()
        {
            Destroy();
        }
        #endregion

        private void Destroy()
        {
            if (_jsThis.IsUndefined())
            {
                return;
            }
            var runtime = ScriptEngine.GetRuntime(_jsContext);
            if (runtime == null)
            {
                return;
            }
            var jsThis = _jsThis;
            _jsThis = JSApi.JS_UNDEFINED;
            var cache = runtime.GetObjectCache();
            cache.RemoveObject(JSApi.JSB_FreePayload(_jsContext, jsThis));
            _state = ReadyState.UNSENT;
            _code = 0;
            runtime.RemoveManagedObject(_handle);
            JSApi.JS_FreeValue(_jsContext, _onreadystatechange);
            _onreadystatechange = JSApi.JS_UNDEFINED;

            JSApi.JS_FreeValue(_jsContext, _onerror);
            _onerror = JSApi.JS_UNDEFINED;

            _jsContext = JSContext.Null;
        }

        // = OnJSFinalize
        public void Dispose()
        {
            Destroy();
        }

        private void OnReadyStateChange()
        {
            if (!_jsContext.IsValid() || JSApi.JS_IsFunction(_jsContext, _onreadystatechange) != 1)
            {
                return;
            }
            var ret = JSApi.JS_Call(_jsContext, _onreadystatechange, JSApi.JS_UNDEFINED);
            JSApi.JS_FreeValue(_jsContext, ret);
        }

        private unsafe void OnError(string error)
        {
            if (!_jsContext.IsValid() || JSApi.JS_IsFunction(_jsContext, _onerror) != 1)
            {
                return;
            }
            var argv = stackalloc[] { Values.js_push_primitive(_jsContext, error) };
            var ret = JSApi.JS_Call(_jsContext, _onerror, JSApi.JS_UNDEFINED, 1, argv);
            if (ret.IsException())
            {
                var ex = _jsContext.GetExceptionString();
                Diagnostics.Logger.Default.Error(ex);
            }
            else
            {
                JSApi.JS_FreeValue(_jsContext, ret);
            }
            JSApi.JS_FreeValue(_jsContext, argv[0]);
        }

        private void Open(string requestUriString, string method, bool bAsync)
        {
            if (_state != ReadyState.UNSENT)
            {
                throw new InvalidOperationException();
            }
            _bAsync = bAsync;
            _method = method;
            _requestUriString = requestUriString;
            _state = ReadyState.OPENED;
            OnReadyStateChange();
        }

        private void Send()
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

            if (_bAsync)
            {
                //TODO: 替换做法
                new Thread(_SendAsync).Start();
            }
            else
            {
                _SendAsync();
            }
        }

        //TODO: 处理线程安全问题
        private void _SendAsync()
        {
            string error = null;
            HttpWebRequest request = null;
            try
            {
                var uri = new Uri(_requestUriString);
                request = WebRequest.CreateHttp(uri);
                request.Method = _method;
                request.Timeout = _timeout;

                var rsp = request.GetResponse();
                if (_state != ReadyState.LOADING)
                {
                    return;
                }

                var reader = new StreamReader(rsp.GetResponseStream());
                _reseponseText = reader.ReadToEnd();
                if (_state != ReadyState.LOADING)
                {
                    return;
                }
                _code = ((HttpWebResponse)rsp).StatusCode;
            }
            catch (Exception ex)
            {
                if (_state != ReadyState.LOADING)
                {
                    return;
                }
                error = ex.ToString();
            }
            finally
            {
                try { request?.Abort(); } catch (Exception) { }
            }

            var runtime = ScriptEngine.GetRuntime(_jsContext);
            runtime?.EnqueueAction(OnResponseCallback, new ResponseArgs()
            {
                request = this,
                error = error,
            });
        }

        private static void OnResponseCallback(ScriptRuntime runtime, object cbArgs, JSValue cbValue)
        {
            if (!runtime.isValid || !runtime.isRunning)
            {
                return;
            }
            var args = cbArgs as ResponseArgs;
            if (!args.request._jsThis.IsUndefined() && args.request._state != ReadyState.UNSENT)
            {
                args.request._state = ReadyState.DONE;
                if (args.error != null)
                {
                    args.request.OnError(args.error);
                }
                else
                {
                    args.request.OnReadyStateChange();
                }
            }
        }

        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        private static JSValue js_constructor(JSContext ctx, JSValue new_target, int argc, JSValue[] argv, int magic)
        {
            var o = new XMLHttpRequest();
            var val = NewBridgeClassObject(ctx, new_target, o, magic, true);
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
                return ctx.ThrowException(exception);
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

                return JSApi.JS_NewInt32(ctx, self._timeout);
            }
            catch (Exception exception)
            {
                return ctx.ThrowException(exception);
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
                self._timeout = timeout;
                return JSApi.JS_UNDEFINED;
            }
            catch (Exception exception)
            {
                return ctx.ThrowException(exception);
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
                return ctx.ThrowException(exception);
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
                return JSApi.JS_DupValue(ctx, self._onreadystatechange);
            }
            catch (Exception exception)
            {
                return ctx.ThrowException(exception);
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
                JSApi.JS_FreeValue(ctx, self._onreadystatechange);
                self._onreadystatechange = JSApi.JS_DupValue(ctx, val);
                return JSApi.JS_UNDEFINED;
            }
            catch (Exception exception)
            {
                return ctx.ThrowException(exception);
            }
        }

        [MonoPInvokeCallback(typeof(JSGetterCFunction))]
        private static JSValue js_get_onerror(JSContext ctx, JSValue this_obj)
        {
            try
            {
                XMLHttpRequest self;
                if (!js_get_classvalue(ctx, this_obj, out self))
                {
                    throw new ThisBoundException();
                }
                return JSApi.JS_DupValue(ctx, self._onerror);
            }
            catch (Exception exception)
            {
                return ctx.ThrowException(exception);
            }
        }

        [MonoPInvokeCallback(typeof(JSSetterCFunction))]
        private static JSValue js_set_onerror(JSContext ctx, JSValue this_obj, JSValue val)
        {
            try
            {
                XMLHttpRequest self;
                if (!js_get_classvalue(ctx, this_obj, out self))
                {
                    throw new ThisBoundException();
                }
                JSApi.JS_FreeValue(ctx, self._onerror);
                self._onerror = JSApi.JS_DupValue(ctx, val);
                return JSApi.JS_UNDEFINED;
            }
            catch (Exception exception)
            {
                return ctx.ThrowException(exception);
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
                return ctx.ThrowException(exception);
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
                return ctx.ThrowException(exception);
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

                return ctx.NewString(self._reseponseText);
            }
            catch (Exception exception)
            {
                return ctx.ThrowException(exception);
            }
        }

        public static void Bind(TypeRegister register, string name)
        {
            var cls = register.CreateGlobalClass(name, typeof(XMLHttpRequest), js_constructor);
            cls.AddMethod(false, "open", js_open, 2);
            cls.AddMethod(false, "send", js_send, 0);
            cls.AddProperty(false, "readyState", js_get_readyState, null);
            cls.AddProperty(false, "status", js_get_status, null);
            cls.AddProperty(false, "responseText", js_get_responseText, null);
            cls.AddProperty(false, "timeout", js_get_timeout, js_set_timeout);
            cls.AddProperty(false, "onreadystatechange", js_get_onreadystatechange, js_set_onreadystatechange);
            cls.AddProperty(false, "onerror", js_get_onerror, js_set_onerror);
        }
    }
}
