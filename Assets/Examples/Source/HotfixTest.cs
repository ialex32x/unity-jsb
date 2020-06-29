using System;
using UnityEngine;
using QuickJS;

[JSHotfix]
public class HotfixTest
{
    public static Func<HotfixTest, int, int> _JSFIX_R_Foo = null;

    private int value = 12;

    public int Foo(int x)
    {
        if (_JSFIX_R_Foo != null)
        {
            return _JSFIX_R_Foo(this, x);
        }
        Debug.LogFormat("Original Foo Method Impl Return {0}", x);
        return x;
    }
}
