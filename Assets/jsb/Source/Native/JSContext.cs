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

        public void print_exception(LogLevel logLevel = LogLevel.Error, string title = "")
        {
            var logger = ScriptEngine.GetLogger(this);
            print_exception(logger, logLevel, title);
        }

        public void print_exception(IScriptLogger logger, LogLevel logLevel, string title)
        {
            var ex = JSApi.JS_GetException(this);

            if (logger != null)
            {
                var err_fileName = JSApi.JS_GetProperty(this, ex, JSApi.JS_ATOM_fileName);
                var err_lineNumber = JSApi.JS_GetProperty(this, ex, JSApi.JS_ATOM_lineNumber);
                var err_message = JSApi.JS_GetProperty(this, ex, JSApi.JS_ATOM_message);
                var err_stack = JSApi.JS_GetProperty(this, ex, JSApi.JS_ATOM_stack);

                var fileName = err_fileName.IsNullish() ? "native" : GetString(err_fileName);
                var lineNumber = err_lineNumber.IsNullish() ? null : GetString(err_lineNumber);
                var message = GetString(err_message);
                var stack = GetString(err_stack);

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

                JSApi.JS_FreeValue(this, err_fileName);
                JSApi.JS_FreeValue(this, err_lineNumber);
                JSApi.JS_FreeValue(this, err_message);
                JSApi.JS_FreeValue(this, err_stack);
            }

            JSApi.JS_FreeValue(this, ex);
        }

        // 在外部已确定存在异常
        public string GetExceptionString()
        {
            var ex = JSApi.JS_GetException(this);
            var err_fileName = JSApi.JS_GetProperty(this, ex, JSApi.JS_ATOM_fileName);
            var err_lineNumber = JSApi.JS_GetProperty(this, ex, JSApi.JS_ATOM_lineNumber);
            var err_message = JSApi.JS_GetProperty(this, ex, JSApi.JS_ATOM_message);
            var err_stack = JSApi.JS_GetProperty(this, ex, JSApi.JS_ATOM_stack);

            var fileName = this.GetString(err_fileName);
            var lineNumber = this.GetString(err_lineNumber);
            var message = this.GetString(err_message);
            var stack = this.GetString(err_stack);

            var exceptionString = string.Format("[JS] {0}:{1} {2}\n{3}", fileName, lineNumber, message, stack);

            JSApi.JS_FreeValue(this, err_fileName);
            JSApi.JS_FreeValue(this, err_lineNumber);
            JSApi.JS_FreeValue(this, err_message);
            JSApi.JS_FreeValue(this, err_stack);
            JSApi.JS_FreeValue(this, ex);

            return exceptionString;
        }

        public unsafe string GetString(JSValue jsValue)
        {
            size_t len;
            var ptr = JSApi.JS_ToCStringLen(this, out len, jsValue);
            if (ptr != IntPtr.Zero)
            {
                try
                {
                    if (len > 0)
                    {
                        var str_ = Encoding.UTF8.GetString((byte*)ptr.ToPointer(), len);
                        return str_;
                    }
                }
                finally
                {
                    JSApi.JS_FreeCString(this, ptr);
                }
            }

            return null;
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
