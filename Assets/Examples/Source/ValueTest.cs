using QuickJS;
using System;

namespace jsb
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
    }
}
