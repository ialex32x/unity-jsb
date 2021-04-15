using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace QuickJS.Native
{
    using JSValueConst = JSValue;
    using JS_BOOL = Int32;

    public partial class JSApi
    {
        [MonoPInvokeCallback(typeof(JSHostPromiseRejectionTracker))]
        public static void PromiseRejectionTracker(JSContext ctx, JSValueConst promise, JSValueConst reason, JS_BOOL is_handled, IntPtr opaque)
        {
            if (is_handled != 1)
            {
                var logger = ScriptEngine.GetLogger(ctx);
                if (logger != null)
                {
                    var reasonStr = GetString(ctx, reason);
                    var is_error = JS_IsError(ctx, reason);

                    do
                    {
                        if (is_error == 1)
                        {
                            var val = JS_GetPropertyStr(ctx, reason, "stack");
                            if (!JS_IsUndefined(val))
                            {
                                var stack = GetString(ctx, val);
                                JS_FreeValue(ctx, val);
                                logger.Write(LogLevel.Error, "Unhandled promise rejection: {0}\n{1}", reasonStr, stack);
                                return;
                            }
                            JS_FreeValue(ctx, val);
                        }
                        logger.Write(LogLevel.Error, "Unhandled promise rejection: {0}", reasonStr);
                    } while (false);
                }
            }
        }

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
                    try
                    {
                        objectCache.RemoveObject(header.value, out obj);
                    }
                    catch (Exception exception)
                    {
                        runtime.GetLogger()?.WriteException(exception);
                    }
                }
            }
        }

        // 用于中转动态注册的反射调用
        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        public static JSValue _DynamicOperatorInvoke(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv, int magic)
        {
            throw new NotImplementedException();
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

        public static string GetString(JSContext ctx, JSValue val)
        {
            size_t len;
            var pstr = JSApi.JS_ToCStringLen(ctx, out len, val);
            if (pstr == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                return JSApi.GetString(ctx, pstr, len);
            }
            finally
            {
                JSApi.JS_FreeCString(ctx, pstr);
            }
        }

        public static unsafe string GetString(JSContext ctx, IntPtr ptr, int len)
        {
            if (len > 0)
            {
                var str = Marshal.PtrToStringAnsi(ptr, len);
                if (str == null)
                {
#if JSB_MARSHAL_STRING
                    var buffer = new byte[len];
                    Marshal.Copy(ptr, buffer, 0, len);
                    return Encoding.UTF8.GetString(buffer);
#else
                    var pointer = (byte*)(void*)ptr;
                    return Encoding.UTF8.GetString(pointer, len);
#endif
                }

                return str;
            }

            return null;
        }
    }
}