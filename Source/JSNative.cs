using System;

namespace QuickJS
{
    using Native;
    using JSValueConst = Native.JSValue;
    using JS_BOOL = Int32;

    /// <summary>
    /// A thin layer wrapping the raw JSApi which depends on some methods from jsb.core module.
    /// the JSApi will be isolated from jsb.core module later.
    /// </summary>
    public static class JSNative
    {
        public static unsafe JSValue NewString(this JSContext ctx, string str)
        {
            if (str == null)
            {
                return JSApi.JS_NULL;
            }

            if (str.Length == 0)
            {
                return JSApi.JSB_NewEmptyString(ctx);
            }

            var bytes = Utils.TextUtils.GetBytes(str);
            fixed (byte* buf = bytes)
            {
                return JSApi.JS_NewStringLen(ctx, buf, bytes.Length);
            }
        }

        /// <summary>
        /// Get a cstring allocated on the heap (will not be automatically collected by GC)
        /// </summary>
        public static unsafe IntPtr NewCString(this JSContext ctx, string str)
        {
            var bytes = Utils.TextUtils.GetNullTerminatedBytes(str);
            fixed (byte* ptr = bytes)
            {
                return JSApi.js_strndup(ctx, ptr, bytes.Length - 1);
            }
        }

        public static unsafe JSValue ThrowException(this JSContext ctx, Exception exception)
        {
            return ThrowInternalError(ctx, exception.ToString());
        }

        public static unsafe JSValue ThrowTypeError(this JSContext ctx, string message)
        {
            var bytes = Utils.TextUtils.GetNullTerminatedBytes(message);
            fixed (byte* msg = bytes)
            {
                return JSApi.JSB_ThrowTypeError(ctx, msg);
            }
        }

        public static unsafe JSValue ThrowInternalError(this JSContext ctx, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return JSApi.JSB_ThrowInternalError(ctx, (byte*)0);
            }

            var bytes = Utils.TextUtils.GetBytes(message);
            fixed (byte* buf = bytes)
            {
                return JSApi.JSB_ThrowError(ctx, buf, bytes.Length);
            }
        }

        public static unsafe JSValue ThrowRangeError(this JSContext ctx, string message)
        {
            var bytes = Utils.TextUtils.GetNullTerminatedBytes(message);
            fixed (byte* msg = bytes)
            {
                return JSApi.JSB_ThrowRangeError(ctx, msg);
            }
        }

        public static unsafe JSValue ThrowReferenceError(this JSContext ctx, string message)
        {
            var bytes = Utils.TextUtils.GetNullTerminatedBytes(message);
            fixed (byte* msg = bytes)
            {
                return JSApi.JSB_ThrowReferenceError(ctx, msg);
            }
        }

        [MonoPInvokeCallback(typeof(JSHostPromiseRejectionTracker))]
        public static void PromiseRejectionTracker(JSContext ctx, JSValueConst promise, JSValueConst reason, JS_BOOL is_handled, IntPtr opaque)
        {
            if (is_handled != 1)
            {
                var logger = ScriptEngine.GetLogger(ctx);
                if (logger != null)
                {
                    var reasonStr = JSApi.GetString(ctx, reason);
                    var is_error = JSApi.JS_IsError(ctx, reason);

                    do
                    {
                        if (is_error == 1)
                        {
                            var val = JSApi.JS_GetPropertyStr(ctx, reason, "stack");
                            if (!JSApi.JS_IsUndefined(val))
                            {
                                var stack = JSApi.GetString(ctx, val);
                                JSApi.JS_FreeValue(ctx, val);
                                logger.Write(Utils.LogLevel.Error, "Unhandled promise rejection: {0}\n{1}", reasonStr, stack);
                                return;
                            }
                            JSApi.JS_FreeValue(ctx, val);
                        }
                        logger.Write(Utils.LogLevel.Error, "Unhandled promise rejection: {0}", reasonStr);
                    } while (false);
                }
            }
        }
    }
}