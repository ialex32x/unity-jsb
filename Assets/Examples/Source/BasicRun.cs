using QuickJS;
using QuickJS.Native;
using System.Collections;

namespace Example
{
    using UnityEngine;

    public class BasicRun : MonoBehaviour
    {
        public bool execRuntime;

        unsafe void Start()
        {
            if (execRuntime)
            {
                var rt = JSApi.JS_NewRuntime();
                JSMemoryUsage s;
                JSApi.JS_ComputeMemoryUsage(rt, &s);
                Debug.Log($"test {rt}: {s.malloc_count} {s.malloc_size} {s.malloc_limit}");
                var ctx = JSApi.JS_NewContext(rt);
                Debug.Log("test:" + ctx);

                // // // JSApi.JS_AddIntrinsicOperators(ctx);
                // // var obj = JSApi.JS_NewObject(ctx);
                // // JSApi.JS_FreeValue(ctx, obj);

                JSApi.JS_FreeContext(ctx);
                JSApi.JS_FreeRuntime(rt);
                Debug.Log("it's a good day");
            }
        }
    }
}
