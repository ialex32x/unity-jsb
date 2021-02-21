using QuickJS;
using QuickJS.Native;

namespace Example
{
    using UnityEngine;

    public class BasicRun : MonoBehaviour
    {
        void Awake()
        {
            var rt = JSApi.JS_NewRuntime();
            var ctx = JSApi.JS_NewContext(rt);

            JSApi.JS_AddIntrinsicOperators(ctx);
            var obj = JSApi.JS_NewObject(ctx);
            JSApi.JS_FreeValue(ctx, obj);

            JSApi.JS_FreeContext(ctx);
            JSApi.JS_FreeRuntime(rt);
        }
    }
}
