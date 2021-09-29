using UnityEngine;
using QuickJS;
using QuickJS.Native;
using QuickJS.Binding;

[JSType]
public class SampleBehaviour : MonoBehaviour
{
    private TypedScriptPromise<string> _p;

    /// <summary>
    /// Define a primitive JSCFunction in C# by yourself, this function will be exported to JS directly without any wrapping glue code. 
    /// !CAUTION! improper operation ref count of JSValue may crash the engine
    /// </summary>
    [JSCFunction("(a: string, b: number): void")]
    public static JSValue PrimitiveCall(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
    {
        // do something you want here
        return JSApi.JS_UNDEFINED;
    }

    public TypedScriptPromise<string> SimpleWait(ScriptContext ctx, int t)
    {
        if (_p != null)
        {
            return null;
        }

        _p = new TypedScriptPromise<string>(ctx);
        return _p;
    }

    public AnyScriptPromise AnotherWait(ScriptContext ctx, int t)
    {
        var p = new AnyScriptPromise(ctx);
        StartCoroutine(_WaitForResolve(() => p.Resolve()));
        return p;
    }

    private System.Collections.IEnumerator _WaitForResolve(System.Action p)
    {
        yield return new WaitForSeconds(3f);
        p();
    }

    void OnGUI()
    {
        var p = _p;
        if (p == null)
        {
            return;
        }

        if (GUILayout.Button("Resolve"))
        {
            _p = null;
            p.Resolve("You clicked 'Resolve' (this is a string from C#)");
        }

        if (GUILayout.Button("Reject"))
        {
            _p = null;
            p.Reject("You clicked 'Reject' (this is a string from C#)");
        }
    }
}