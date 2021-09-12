using QuickJS;
using System;

namespace Example
{
    [JSType]
    public class ValueTest
    {
        //TODO: 按 JSUseStringCache 属性配置使缓存行为可选功能未完成

        public static void TakeStringWithCache([JSUseStringCache] string v)
        {
        }

        public static void TakeString(string v)
        {
        }

        // 未完成
        [return: JSUseStringCache]
        public static string Foo([JSUseStringCache] string v)
        {
            return v;
        }

        public static bool CheckArgs(int a, out float b, ref string c)
        {
            b = 1f;
            return true;
        }

        public static bool CheckArgs(int a, out float b, ref UnityEngine.GameObject c)
        {
            b = 1f;
            return true;
        }

        public static void MakeTrouble()
        {
            throw new InvalidOperationException();
        }

        public static int[] values1 = new int[6] { 0, 1, 2, 3, 5, 6 };

        public static int[,] values2 = new int[2, 3] {
            { 0, 1, 2 },
            { 3, 4, 5 },
        };
    }

    /** 
     * 可以通过 .AddRequiredDefines(...) 为导出的绑定代码添加条件编译
     * 以便在编译条件发生变化时无需重新生成绑定代码
     * 详见 CustomBinding.cs 中的示意代码, 以及对应的生成代码
     */
#if CUSTOM_DEF_FOO && UNITY_EDITOR
    public class FOO
    {
#if CUSTOM_DEF_PROP
        public string propValue {get; set;}
#endif
        
        public static string value = "FOO";
    }
#endif

#if CUSTOM_DEF_BAR
    public class BAR
    {
        public static string value = "BAR";
    }
#endif
}
