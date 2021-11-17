using System;

namespace QuickJS
{
    using Native;

    public static class JSNative
    {
        public static unsafe JSValue ThrowException(this JSContext ctx, Exception exception)
        {
            return JSApi.JS_ThrowInternalError(ctx, exception.ToString());
        }
    }
}