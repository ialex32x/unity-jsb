using System;
using System.Collections.Generic;
using AOT;

namespace QuickJS.Binding
{
    using UnityEngine;
    using Native;

    public partial class Values
    {
        // 通用析构函数
        [MonoPInvokeCallback(typeof(JSClassFinalizer))]
        protected static void class_dtor(JSRuntime rt, JSValue val)
        {
            var runtime = ScriptEngine.GetRuntime(rt);
            var class_id = runtime._def_class_id;
            var payload = JSApi.JSB_FreePayloadRT(rt, val, class_id);
            var objectCache = runtime.GetObjectCache();
            
            if (objectCache != null)
            {
                objectCache.RemoveObject(payload.header.object_id);
            }
        }

        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        protected static JSValue class_private_ctor(JSContext ctx, JSValue new_target, int argc, JSValue[] argv, int magic)
        {
            JSApi.JS_ThrowInternalError(ctx, "cant call constructor on this type");
            return JSApi.JS_UNDEFINED;
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        protected static void struct_dtor(JSRuntime rt, JSValue val)
        {
            var runtime = ScriptEngine.GetRuntime(rt);
            var class_id = runtime._def_struct_id;
            JSApi.JSB_FreePayloadRT(rt, val, class_id);
        }

        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        protected static JSValue struct_private_ctor(JSContext ctx, JSValue this_val, int argc, JSValue[] argv, int magic)
        {
            JSApi.JS_ThrowInternalError(ctx, "cant call constructor on this type");
            return JSApi.JS_UNDEFINED;
        }
    }
}