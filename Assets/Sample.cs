namespace jsb
{
    using UnityEngine;

    public class Sample:MonoBehaviour
    {
        void Awake()
        {
            var rt = JSBridgeDLL.JS_NewRuntime();
            var ctx = JSBridgeDLL.JS_NewContext(rt);
            Debug.Log(rt);
            Debug.Log(ctx);
            var val = JSBridgeDLL.JSB_Test(ctx);
            Debug.Log(val);

            var jsval = JSBridgeDLL.JS_Eval(ctx, "2+2");
            int rval;
            JSBridgeDLL.JS_ToInt32(ctx, out rval, jsval);
            Debug.LogFormat("2+2={0}", rval);
            JSBridgeDLL.JS_FreeContext(ctx);
            JSBridgeDLL.JS_FreeRuntime(rt);
        }
    }
}