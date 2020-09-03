using UnityEngine;
using QuickJS;
using QuickJS.Native;
using QuickJS.Binding;

[JSType]
public class SampleBehaviour : MonoBehaviour
{
    private TypedScriptPromise<string> _p;

    // 可以通过 JSCFunctionAttribute 标记此函数直接接入JS参数
    // 这种情况下, 可以不用 try/catch 包装, 但仍需自己保证 JSValue 的引用平衡, 否则会导致崩溃
    // 必须谨慎使用
    [JSCFunction("(a: string, b: number): void")]
    public static JSValue PrimitiveCall(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
    {
        Debug.Log("直接无包装导出方法");
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
            p.Resolve("执行成功测试, 我是一个C#字符串, 传给JS");
        }

        if (GUILayout.Button("Reject"))
        {
            _p = null;
            p.Reject("执行失败测试");
        }
    }
}