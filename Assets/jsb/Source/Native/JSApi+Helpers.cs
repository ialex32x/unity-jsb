using System;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

namespace QuickJS.Native
{
    public partial class JSApi
    {
        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        public static JSValue class_private_ctor(JSContext ctx, JSValue new_target, int argc, JSValue[] argv, int magic)
        {
            JSApi.JS_ThrowInternalError(ctx, "cant call constructor on this type");
            return JSApi.JS_UNDEFINED;
        }

        // 通用析构函数
        [MonoPInvokeCallback(typeof(JSClassFinalizer))]
        public static void class_finalizer(JSRuntime rt, JSValue val)
        {
            var runtime = ScriptEngine.GetRuntime(rt);
            var header = JSApi.JSB_FreePayloadRT(rt, val);
            if (header.type_id == BridgeObjectType.ObjectRef)
            {
                var objectCache = runtime.GetObjectCache();

                if (objectCache != null)
                {
                    objectCache.RemoveObject(header.value);
                }
            }
        }

        public static string GetString(JSContext ctx, JSValue val)
        {
            size_t len;
            var pstr = JSApi.JS_ToCStringLen(ctx, out len, val);
            if (pstr == IntPtr.Zero)
            {
                return null;
            }

            var str = JSApi.GetString(pstr, len);
            JSApi.JS_FreeCString(ctx, pstr);
            return str;
        }

        public static string GetString(IntPtr ptr, int len)
        {
            var str = Marshal.PtrToStringAnsi(ptr, len);
            if (str == null)
            {
                var buffer = new byte[len];
                Marshal.Copy(ptr, buffer, 0, len);
                return Encoding.UTF8.GetString(buffer);
            }

            return str;
        }
    }
}