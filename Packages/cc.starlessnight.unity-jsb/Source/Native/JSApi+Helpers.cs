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
                                logger.Write(Utils.LogLevel.Error, "Unhandled promise rejection: {0}\n{1}", reasonStr, stack);
                                return;
                            }
                            JS_FreeValue(ctx, val);
                        }
                        logger.Write(Utils.LogLevel.Error, "Unhandled promise rejection: {0}", reasonStr);
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
            var header = JSApi.JSB_FreePayloadRT(rt, val);
            if (header.type_id == BridgeObjectType.ObjectRef)
            {
                var objectCache = ScriptEngine.GetObjectCache(rt);
                if (objectCache != null)
                {
                    try
                    {
                        objectCache.RemoveObject(header.value);
                    }
                    catch (Exception exception)
                    {
                        ScriptEngine.GetLogger(rt)?.WriteException(exception);
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
                    return ctx.ThrowException(exception);
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
                    return ctx.ThrowException(exception);
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
                    return ctx.ThrowException(exception);
                }
            }

            return JSApi.JS_ThrowInternalError(ctx, "dynamic field not found");
        }

        public static string GetString(JSContext ctx, JSAtom atom)
        {
            var strValue = JSApi.JS_AtomToString(ctx, atom);
            var str = strValue.IsString() ? GetString(ctx, strValue) : null;
            JSApi.JS_FreeValue(ctx, strValue);
            return str;
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

        public static string GetNonNullString(JSContext ctx, JSValue val)
        {
            size_t len;
            var pstr = JSApi.JS_ToCStringLen(ctx, out len, val);
            if (pstr == IntPtr.Zero)
            {
                return string.Empty;
            }

            try
            {
                return JSApi.GetString(ctx, pstr, len) ?? string.Empty;
            }
            finally
            {
                JSApi.JS_FreeCString(ctx, pstr);
            }
        }

        public static unsafe void MemoryCopy(void* source, void* destination, long destinationSizeInBytes, long sourceBytesToCopy)
        {
#if JSB_COMPATIBLE
            if (sourceBytesToCopy > destinationSizeInBytes)
            {
                throw new ArgumentOutOfRangeException();
            }

            var pSource = (byte*)source;
            var pDestination = (byte*)destination;

            for (int i = 0; i < sourceBytesToCopy; ++i)
            {
                pDestination[i] = pSource[i];
            }
#else
            Buffer.MemoryCopy(source, destination, destinationSizeInBytes, sourceBytesToCopy);
#endif
        }

        public static unsafe string GetString(JSContext ctx, IntPtr ptr, int len)
        {
            if (len > 0)
            {
                var str = Marshal.PtrToStringAnsi(ptr, len);
                if (str == null)
                {
#if JSB_COMPATIBLE
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

        public static unsafe string FindKeyOfProperty(JSContext ctx, JSValue this_obj, JSValue prop_value)
        {
            uint plen;
            JSPropertyEnum* ptab;
            if (JSApi.JS_GetOwnPropertyNames(ctx, out ptab, out plen, this_obj, JSGPNFlags.JS_GPN_STRING_MASK) < 0)
            {
                // failed
                return null;
            }

            try
            {
                for (var i = 0; i < plen; i++)
                {
                    var prop = JSApi.JS_GetProperty(ctx, this_obj, ptab[i].atom);
                    if (prop == prop_value)
                    {
                        JSApi.JS_FreeValue(ctx, prop);
                        return JSApi.GetString(ctx, ptab[i].atom);
                    }
                    JSApi.JS_FreeValue(ctx, prop);
                }
            }
            finally
            {
                for (var i = 0; i < plen; i++)
                {
                    JSApi.JS_FreeAtom(ctx, ptab[i].atom);
                }
            }
            return null;
        }

        public static unsafe bool ForEachProperty(JSContext ctx, JSValue this_obj, Func<JSAtom, JSValue, bool> callback)
        {
            JSPropertyEnum* ptab;
            uint plen;
            if (JSApi.JS_GetOwnPropertyNames(ctx, out ptab, out plen, this_obj, JSGPNFlags.JS_GPN_STRING_MASK) < 0)
            {
                // failed
                return false;
            }

            var stop = false;
            for (var i = 0; i < plen; i++)
            {
                var prop = JSApi.JS_GetProperty(ctx, this_obj, ptab[i].atom);
                try
                {
                    if (callback(ptab[i].atom, prop))
                    {
                        stop = true;
                        break;
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    JSApi.JS_FreeValue(ctx, prop);
                }
            }

            for (var i = 0; i < plen; i++)
            {
                JSApi.JS_FreeAtom(ctx, ptab[i].atom);
            }
            return stop;
        }
    }
}