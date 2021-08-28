using System;
using System.Runtime.InteropServices;
using System.Text;

namespace QuickJS.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct JSContext
    {
        public static readonly JSContext Null;

        private unsafe void* _ptr;

        public unsafe bool IsValid()
        {
            return _ptr != (void*)0;
        }

        public unsafe bool IsContext(JSContext c)
        {
            return _ptr == c._ptr;
        }

        public void print_exception(Utils.LogLevel logLevel = Utils.LogLevel.Error, string title = "")
        {
            var logger = ScriptEngine.GetLogger(this);
            print_exception(this, logger, logLevel, title);
        }

        public static void print_exception(JSContext ctx, Utils.LogLevel logLevel = Utils.LogLevel.Error, string title = "")
        {
            var logger = ScriptEngine.GetLogger(ctx);
            print_exception(ctx, logger, logLevel, title);
        }

        public static void print_exception(JSContext ctx, Utils.IScriptLogger logger, Utils.LogLevel logLevel, string title)
        {
            var ex = JSApi.JS_GetException(ctx);

            try
            {
                if (logger != null)
                {
                    var err_fileName = JSApi.JS_GetProperty(ctx, ex, JSApi.JS_ATOM_fileName);
                    var err_lineNumber = JSApi.JS_GetProperty(ctx, ex, JSApi.JS_ATOM_lineNumber);
                    var err_message = JSApi.JS_GetProperty(ctx, ex, JSApi.JS_ATOM_message);
                    var err_stack = JSApi.JS_GetProperty(ctx, ex, JSApi.JS_ATOM_stack);

                    try
                    {
                        var fileName = err_fileName.IsNullish() ? "native" : JSApi.GetString(ctx, err_fileName);
                        var lineNumber = err_lineNumber.IsNullish() ? null : JSApi.GetString(ctx, err_lineNumber);
                        var message = JSApi.GetString(ctx, err_message);
                        var stack = JSApi.GetString(ctx, err_stack);

                        if (string.IsNullOrEmpty(lineNumber))
                        {
                            if (string.IsNullOrEmpty(stack))
                            {
                                logger.Write(logLevel, "[{0}] {1} {2}",
                                    fileName, title, message);
                            }
                            else
                            {
                                logger.Write(logLevel, "[{0}] {1} {2}\nJavascript stack:\n{3}",
                                    fileName, title, message, stack);
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(stack))
                            {
                                logger.Write(logLevel, "[{0}:{1}] {2} {3}",
                                fileName, lineNumber, title, message);
                            }
                            else
                            {
                                logger.Write(logLevel, "[{0}:{1}] {2} {3}\nJavascript stack:\n{4}",
                                    fileName, lineNumber, title, message, stack);
                            }
                        }
                    }
                    finally
                    {
                        JSApi.JS_FreeValue(ctx, err_fileName);
                        JSApi.JS_FreeValue(ctx, err_lineNumber);
                        JSApi.JS_FreeValue(ctx, err_message);
                        JSApi.JS_FreeValue(ctx, err_stack);
                    }
                }
            }
            finally
            {
                JSApi.JS_FreeValue(ctx, ex);
            }
        }

        // 在外部已确定存在异常
        public string GetExceptionString()
        {
            var ex = JSApi.JS_GetException(this);
            var err_fileName = JSApi.JS_GetProperty(this, ex, JSApi.JS_ATOM_fileName);
            var err_lineNumber = JSApi.JS_GetProperty(this, ex, JSApi.JS_ATOM_lineNumber);
            var err_message = JSApi.JS_GetProperty(this, ex, JSApi.JS_ATOM_message);
            var err_stack = JSApi.JS_GetProperty(this, ex, JSApi.JS_ATOM_stack);

            try
            {
                var fileName = JSApi.GetString(this, err_fileName);
                var lineNumber = JSApi.GetString(this, err_lineNumber);
                var message = JSApi.GetString(this, err_message);
                var stack = JSApi.GetString(this, err_stack);
                var exceptionString = string.Format("[JS] {0}:{1} {2}\n{3}", fileName, lineNumber, message, stack);
                
                return exceptionString;
            }
            finally
            {

                JSApi.JS_FreeValue(this, err_fileName);
                JSApi.JS_FreeValue(this, err_lineNumber);
                JSApi.JS_FreeValue(this, err_message);
                JSApi.JS_FreeValue(this, err_stack);
                JSApi.JS_FreeValue(this, ex);
            }
        }

        public void SetProperty(JSValue this_obj, string name, JSCFunction fn, int length = 0)
        {
            JSApi.JS_SetPropertyStr(this, this_obj, name, JSApi.JS_NewCFunction(this, fn, name, length));
        }

        public override unsafe int GetHashCode()
        {
            return (int)_ptr;
        }

        public override unsafe bool Equals(object obj)
        {
            if (obj is JSContext)
            {
                var t = (JSContext)obj;
                return t._ptr == _ptr;
            }

            return false;
        }

        public static unsafe bool operator ==(JSContext a, JSContext b)
        {
            return a._ptr == b._ptr;
        }

        public static bool operator !=(JSContext a, JSContext b)
        {
            return !(a == b);
        }
    }
}
