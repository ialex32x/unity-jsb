using System;

namespace jsb
{
    using UnityEngine;

    public class Sample:MonoBehaviour
    {
        void Awake()
        {
            Debug.Log(unchecked((ulong)-11));
            Debug.Log(unchecked((ulong)-1));
            var rt = JSBridgeDLL.JS_NewRuntime();
            var ctx = JSBridgeDLL.JS_NewContext(rt);

            var jsval = JSBridgeDLL.JS_Eval(ctx, "2+2", "eval");
            int rval;
            JSBridgeDLL.JS_ToInt32(ctx, out rval, jsval);
            Debug.LogFormat("2+2={0}", rval);
            JSBridgeDLL.JS_FreeContext(ctx);
            JSBridgeDLL.JS_FreeRuntime(rt);
        }
    }
}