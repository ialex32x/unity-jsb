
[QuickJS.JSType]
public class NoNamespaceClass
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
}
