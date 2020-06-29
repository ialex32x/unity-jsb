#if UNITY_STANDALONE_WIN
// Special: _QuickJSDelegates
// Unity: 2019.1.9f1
using System;
using System.Collections.Generic;

namespace jsb
{
    using QuickJS;
    using QuickJS.Binding;
    using QuickJS.Native;

    public partial class _QuickJSDelegates : Values
    {
        [QuickJS.JSDelegateAttribute(typeof(System.Func<HotfixTest, int, int>))]
        public static int _QuickJSDelegates4(QuickJS.ScriptDelegate fn, HotfixTest self, int arg) {
            var ctx = fn.ctx;
            var argv = new JSValue[1];
            argv[0] = js_push_primitive(ctx, arg);
            if (argv[0].IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            var this_obj = js_push_classvalue(ctx, self);
            var rval = fn.Invoke(ctx, this_obj, 1, argv);
            JSApi.JS_FreeValue(ctx, this_obj); // this_obj is temp 
            int ret0;
            var succ = js_get_primitive(ctx, rval, out ret0);
            JSApi.JS_FreeValue(ctx, argv[0]);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            JSApi.JS_FreeValue(ctx, rval);
            if (succ)
            {
                return ret0;
            }
            else
            {
                throw new Exception("js exception caught");
            }
        }
    }
}
#endif