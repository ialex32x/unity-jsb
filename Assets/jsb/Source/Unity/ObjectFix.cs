#if !JSB_UNITYLESS
#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace QuickJS.Unity
{
    using Native;
    using Binding;
    using UnityEngine;

    public static class ObjectFix
    {
        public static JSValue js_push_as_array<T>(JSContext ctx, T[] objects)
        where T : Object
        {
            if (objects == null)
            {
                return JSApi.JS_NULL;
            }
            
            var array = JSApi.JS_NewArray(ctx);
            var len = objects.Length;

            for (var i = 0; i < len; ++i)
            {
                var obj = objects[i];

                JSApi.JS_SetPropertyUint32(ctx, array, (uint)i, Values.js_push_classvalue(ctx, obj));
            }
            return array;
        }
    }
}

#endif // !JSB_UNITYLESS
#endif // UNITY_EDITOR