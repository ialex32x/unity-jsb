
// 测试: 不导出基类的情况
public class NoNamespaceBase
{
}

[QuickJS.JSType]
public class NoNamespaceClass : NoNamespaceBase
{
    public static void Moo()
    {
    }

    public static byte[] MakeBytes()
    {
        return System.Text.Encoding.UTF8.GetBytes("sdfasdf");
    }

    public static byte[] TestBytes(byte[] test)
    {
        return test;
    }

    public static void TestOut(out int x, int y, out int z)
    {
        x = y;
        z = y;
    }

    public static void TestRefOut(ref int g, out int x, int y, out int z)
    {
        x = y * g;
        z = y + g;
        g = x + z;
    }

    public static int? TestNullable(int? x, int? y)
    {
        return (x ?? 0) + (y ?? 0);
    }

    public static float? TestNullable(UnityEngine.Vector2? xy)
    {
        return xy != null ? null : (float?)((UnityEngine.Vector2)xy).magnitude;
    }

    public static float? TestNullable(UnityEngine.Vector2? xy, ref float? g)
    {
        g = UnityEngine.Random.value;
        return xy != null ? null : (float?)((UnityEngine.Vector2)xy).magnitude;
    }
}
