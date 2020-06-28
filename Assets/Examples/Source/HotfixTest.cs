using System;
using UnityEngine;
using QuickJS;

[JSHotfix]
public class HotfixTest
{
    public static Func<int, int> _JSFIX_R_Foo = null;
    public static Action<int> _JSFIX_B_Foo = null;
    public static Action<int> _JSFIX_A_Foo = null;

    public int Foo(int x)
    {
        if (_JSFIX_R_Foo != null)
        {
            return _JSFIX_R_Foo(x);
        }
        try
        {
            if (_JSFIX_B_Foo != null)
            {
                _JSFIX_B_Foo(x);
            }
            Debug.Log("Original Foo Method Impl");
            return x;
        }
        finally
        {
            if (_JSFIX_A_Foo != null)
            {
                _JSFIX_A_Foo(x);
            }
        }
    }
}
