using System.Diagnostics;

namespace QuickJS.Diagnostics
{
    public static class Assert
    {
        /// <summary>
        /// 发布期断言, 在定义为 JSB_RELEASE 时仅报错, 不暂停, 否则等价于 Debug 断言 
        /// </summary>
        [Conditional("JSB_DEBUG")]
        [Conditional("JSB_RELEASE")]
        public static void Never(string message = "")
        {
            var stackTrace = new StackTrace(1, true);
            var text = "[ASSERT_FAILED][NEVER] ";

            if (!string.IsNullOrEmpty(message))
            {
                text += message + "\n";
            }
            text += stackTrace.ToString();

#if JSB_DEBUG
            Logger.Default.Fatal(text);
#else 
            Logger.Default.Error(text);
#endif
        }

        [Conditional("JSB_DEBUG")]
        [Conditional("JSB_RELEASE")]
        public static void Never(string fmt, params object[] args)
        {
            Never(string.Format(fmt, args));
        }

        /// <summary>
        /// 发布期断言, 在定义为 JSB_RELEASE 时仅报错, 不暂停, 否则等价于 Debug 断言 
        /// </summary>
        [Conditional("JSB_DEBUG")]
        [Conditional("JSB_RELEASE")]
        public static void Release(bool condition, string message = "")
        {
            if (condition)
            {
                return;
            }
            var stackTrace = new StackTrace(1, true);
            var text = "[ASSERT_FAILED][RELEASE] ";

            if (!string.IsNullOrEmpty(message))
            {
                text += message + "\n";
            }
            text += stackTrace.ToString();

#if JSB_DEBUG
            Logger.Default.Fatal(text);
#else 
            Logger.Default.Error(text);
#endif
        }

        [Conditional("JSB_DEBUG")]
        [Conditional("JSB_RELEASE")]
        public static void Release(bool condition, string fmt, params object[] args)
        {
            if (condition)
            {
                return;
            }
            Release(false, string.Format(fmt, args));
        }

        /// <summary>
        /// 调试期断言, 触发时将暂停编辑器运行
        /// </summary>
        [Conditional("JSB_DEBUG")]
        public static void Debug(bool condition, string message = "")
        {
            if (condition)
            {
                return;
            }
            var stackTrace = new StackTrace(1, true);
            var text = "[ASSERT_FAILED][DEBUG] ";

            if (!string.IsNullOrEmpty(message))
            {
                text += message + "\n";
            }
            text += stackTrace.ToString();

            Logger.Default.Fatal(text);
        }

        [Conditional("JSB_DEBUG")]
        public static void Debug(bool condition, string fmt, params object[] args)
        {
            if (condition)
            {
                return;
            }
            Debug(false, string.Format(fmt, args));
        }

        public static bool Ensure(bool condition, string message = "")
        {
            if (condition)
            {
                return true;
            }

#if JSB_DEBUG
            var stackTrace = new StackTrace(1, true);
            var text = "[ASSERT_FAILED] ";

            if (!string.IsNullOrEmpty(message))
            {
                text += message + "\n";
            }
            text += stackTrace.ToString();

            Logger.Default.Fatal(text);
#endif
            return false;
        }

        public static bool Ensure(bool condition, string fmt, params object[] args)
        {
            if (condition)
            {
                return true;
            }
            return Ensure(false, string.Format(fmt, args));
        }
    }
}