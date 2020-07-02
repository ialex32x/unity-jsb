using System;
using UnityEngine;
using QuickJS;

[JSHotfix]
public class HotfixTest
{
    private int value = 12;

    public int Foo(int x)
    {
        Debug.LogFormat("HotfixTest Original Foo Method Impl Return {0}", x);
        return x;
    }
}

[JSHotfix]
[JSType]
public class HotfixTest2
{
    private int value = 12;

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
