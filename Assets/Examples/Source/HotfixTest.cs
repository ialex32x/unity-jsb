using System;
using UnityEngine;
using QuickJS;

[JSHotfix]
public class HotfixTest
{
    public static jsb._QuickJSDelegates._HotfixDelegate0 _JSFIX_R_Foo = null;

    private int value = 12;

    public int Foo(int x)
    {
        if (_JSFIX_R_Foo != null)
        {
            return _JSFIX_R_Foo(this, x);
        }
        Debug.LogFormat("HotfixTest Original Foo Method Impl Return {0}", x);
        return x;
    }
}

[JSHotfix]
[JSType]
public class HotfixTest2
{
    public static jsb._QuickJSDelegates._HotfixDelegate1 _JSFIX_R_Foo = null;

    private int value = 12;

    public int Foo(int x)
    {
        if (_JSFIX_R_Foo != null)
        {
            return _JSFIX_R_Foo(this, x);
        }
        Debug.LogFormat("HotfixTest2 Original Foo Method Impl Return {0}", x);
        return x;
    }
}
