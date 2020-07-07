using System;
using System.Runtime.CompilerServices;
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
            return JSApi.JS_ThrowInternalError(ctx, "cant call constructor on this type");
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
                    object obj;
                    if (objectCache.RemoveObject(header.value, out obj))
                    {
                        var jsf = obj as IScriptFinalize;
                        if (jsf != null)
                        {
                            jsf.OnJSFinalize();
                        }
                    }
                }
            }
        }

        // 用于中转动态注册的反射调用
        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        public static JSValue _DynamicMethodInvoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv, int magic)
        {
            var typeDB = ScriptEngine.GetTypeDB(ctx);

            if (typeDB == null)
            {
                return JSApi.JS_ThrowInternalError(ctx, "type db is null");
            }

            var method = typeDB.GetDynamicMethod(magic);
            if (method != null)
            {
                try
                {
                    return method.Invoke(ctx, this_obj, argc, argv);
                }
                catch (Exception exception)
                {
                    return JSApi.ThrowException(ctx, exception);
                }
            }

            return JSApi.JS_ThrowInternalError(ctx, "dynamic method not found");
        }

        // 用于中转动态注册的反射调用
        [MonoPInvokeCallback(typeof(JSGetterCFunctionMagic))]
        public static JSValue _DynamicFieldGetter(JSContext ctx, JSValue this_obj, int magic)
        {
            var typeDB = ScriptEngine.GetTypeDB(ctx);

            if (typeDB == null)
            {
                return JSApi.JS_ThrowInternalError(ctx, "type db is null");
            }

            var field = typeDB.GetDynamicField(magic);
            if (field != null)
            {
                try
                {
                    return field.GetValue(ctx, this_obj);
                }
                catch (Exception exception)
                {
                    return JSApi.ThrowException(ctx, exception);
                }
            }

            return JSApi.JS_ThrowInternalError(ctx, "dynamic field not found");
        }

        // 用于中转动态注册的反射调用
        [MonoPInvokeCallback(typeof(JSSetterCFunctionMagic))]
        public static JSValue _DynamicFieldSetter(JSContext ctx, JSValue this_obj, JSValue val, int magic)
        {
            var typeDB = ScriptEngine.GetTypeDB(ctx);

            if (typeDB == null)
            {
                return JSApi.JS_ThrowInternalError(ctx, "type db is null");
            }

            var field = typeDB.GetDynamicField(magic);
            if (field != null)
            {
                try
                {
                    return field.SetValue(ctx, this_obj, val);
                }
                catch (Exception exception)
                {
                    return JSApi.ThrowException(ctx, exception);
                }
            }

            return JSApi.JS_ThrowInternalError(ctx, "dynamic field not found");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(JSContext ctx, JSValue val)
        {
            size_t len;
            var pstr = JSApi.JS_ToCStringLen(ctx, out len, val);
            if (pstr == IntPtr.Zero)
            {
                return null;
            }

            var str = JSApi.GetString(ctx, pstr, len);
            JSApi.JS_FreeCString(ctx, pstr);
            return str;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe string GetString(JSContext ctx, IntPtr ptr, int len)
        {
            var str = Marshal.PtrToStringAnsi(ptr, len);
            if (str == null)
            {
                var pointer = (byte*)(void*)ptr;
                return Encoding.UTF8.GetString(pointer, len);
            }

            return str;
        }
    }
}