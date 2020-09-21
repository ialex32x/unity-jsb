using QuickJS;
using System;

namespace jsb
{
    [JSType]
    public class ValueTest
    {
        //TODO: 参数 string 考虑一种可选的 index string 方式传递, 针对重复大字符串进行对象池优化传递

        // 未完成
        [return: JSUseStringCache]
        public static string Foo([JSUseStringCache] string v)
        {
            return v;
        }
    }
}
