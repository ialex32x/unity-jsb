using System;
using UnityEngine;
using QuickJS;

#pragma warning disable CS0414

[JSHotfix(JSHotfixFlags.Full)]
public class HotfixTest
{
    private int value = 12;
    private static string static_value = "<私有静态变量>";

    public HotfixTest()
    {
        Debug.LogFormat("[HOTFIX][C#] 构造函数");
    }

    public int Foo(int x)
    {
        Debug.LogFormat("[HOTFIX][C#] HotfixTest.Foo({0})", x);
        return x;
    }

    public string Foo(string x)
    {
        Debug.LogFormat("[HOTFIX][C#] HotfixTest.Foo({0})", x);
        return x;
    }

    public static void SimpleStaticCall()
    {
        Debug.LogWarningFormat("[HOTFIX][C#] HotfixTest.SimpleStaticCall()");
    }

    public static void AnotherStaticCall()
    {
        Debug.LogWarningFormat("[HOTFIX][C#] HotfixTest.AnotherStaticCall()");
    }
}

[JSHotfix]
[JSType]
public class HotfixTest2
{
    private int value = 12;

    // 暂时不支持
    public void CallByRef(out int v)
    {
        v = 1;
    }

    public int Foo(int x)
    {
        Debug.LogFormat("HotfixTest2 Original Foo Method Impl Return {0}", x);
        return x;
    }
}

public class NotExportedClass
{
    private int _value;

    public int value
    {
        get { return _value; }
        set { _value = value; }
    }

    public void Foo()
    {
        Debug.Log("NotExportedClass.Foo");
    }
}

#pragma warning restore CS0414
