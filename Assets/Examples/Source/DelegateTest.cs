using QuickJS;
using System;

namespace jsb
{
    [JSType]
    public class DelegateTest
    {
        public class NotExportedClass
        {
            public string value = "未导出类的访问测试";
        }

        [JSType]
        public class InnerTest
        {
            public const string hello = "hello";
        }

        public Action onAction;
        public Action<string, float, int> onActionWithArgs;
        public Func<int, int> onFunc;

        public void CallAction()
        {
            onAction?.Invoke();
        }

        public static NotExportedClass GetNotExportedClass()
        {
            return new NotExportedClass();
        }

        public void CallActionWithArgs(string a1, float a2, int a3)
        {
            onActionWithArgs?.Invoke(a1, a2, a3);
        }

        public int CallFunc(int a1)
        {
            return onFunc == null ? a1 : onFunc.Invoke(a1);
        }
    }
}
