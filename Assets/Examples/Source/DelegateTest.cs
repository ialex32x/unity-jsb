using QuickJS;
using System;

namespace jsb
{
    //TODO: 暂时不支持
    //TODO: 先调整 ref/out 在脚本中的约定, 从返回值约定改为参数Ref<T>/Out<T>包装约定
    //TODO: 然后提供此支持
    public delegate void UnsupportedAtPresent_Callback(out int v);

    [JSType]
    public class DelegateTest
    {
        public UnsupportedAtPresent_Callback unsupported;
        public Action actionFieldRW;
        public readonly Action actionFieldR;
        public Action actionPropG { get; }
        public Action actionPropS { set { } }
        public Action actionPropGS { get; set; }

        public static Action actionFieldRW_s;
        public static readonly Action actionFieldR_s;
        public static Action actionPropG_s { get; }
        public static Action actionPropS_s { set { } }
        public static Action actionPropGS_s { get; set; }

        public class NotExportedClass
        {
            public string value = "未导出类的访问测试";
            public static string value2 = "未导出类的访问测试2";

            public int Add(int a, int b)
            {
                return a + b;
            }
        }

        [JSType]
        public class InnerTest
        {
            public const string hello = "hello";
        }

        public Action onAction;
        public void AddAction()
        {
            onAction += () =>
             {
                 UnityEngine.Debug.Log("这是在 C# 中添加到委托上的一个 C# Action");
             };
        }
        public Action<string, float, int> onActionWithArgs;
        public Func<int, int> onFunc;

        public event Action<int> onEvent;
        public static event Action<int> onStaticEvent;

        public void DipatchEvent(int v)
        {
            onEvent?.Invoke(v);
        }

        public static void DipatchStaticEvent(int v)
        {
            onStaticEvent?.Invoke(v);
        }

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

        public static void CallHotfixTest()
        {
            var h1 = new HotfixTest();
            UnityEngine.Debug.LogFormat("HotfixTest1: {0}", h1.Foo(12));

            var h2 = new HotfixTest2();
            UnityEngine.Debug.LogFormat("HotfixTest2: {0}", h2.Foo(12));
        }

        public static InnerTest[] GetArray()
        {
            return null;
        }
    }
}
