using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using QuickJS.Binding;
using QuickJS.Native;
using QuickJS.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QuickJS
{
    public partial class ScriptContext
    {
        #region Builtins

        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        private static JSValue _print(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv, int magic)
        {
            var runtime = ScriptEngine.GetRuntime(ctx);
            if (runtime == null)
            {
                return JSApi.JS_UNDEFINED;
            }
            var logger = runtime.GetLogger();
            if (logger == null)
            {
                return JSApi.JS_UNDEFINED;
            }
            int i;
            var sb = new StringBuilder();
            size_t len;

            for (i = 0; i < argc; i++)
            {
                if (i != 0)
                {
                    sb.Append(' ');
                }

                var pstr = JSApi.JS_ToCStringLen(ctx, out len, argv[i]);
                if (pstr == IntPtr.Zero)
                {
                    return JSApi.JS_EXCEPTION;
                }

                var str = JSApi.GetString(ctx, pstr, len);
                if (str != null)
                {
                    sb.Append(str);
                }

                JSApi.JS_FreeCString(ctx, pstr);
            }

            sb.AppendLine();
            if (runtime.withStacktrace)
            {
                runtime.GetContext(ctx).AppendStacktrace(sb);
            }
            logger.ScriptWrite((LogLevel)magic, sb.ToString());
            return JSApi.JS_UNDEFINED;
        }

        #endregion

        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue _DoFile(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc < 1 || !argv[0].IsString())
            {
                return JSApi.JS_ThrowInternalError(ctx, "path expected");
            }
            var path = JSApi.GetString(ctx, argv[0]);
            if (string.IsNullOrEmpty(path))
            {
                return JSApi.JS_ThrowInternalError(ctx, "invalid path");
            }

            var context = ScriptEngine.GetContext(ctx);
            var runtime = context.GetRuntime();
            var fileSystem = runtime.GetFileSystem();
            var resolver = runtime.GetFileResolver();
            string resolvedPath;
            if (!resolver.ResolvePath(fileSystem, path, out resolvedPath))
            {
                return JSApi.JS_ThrowInternalError(ctx, "file not found");
            }
            var source = fileSystem.ReadAllText(resolvedPath);
            return context.EvalSource(source, resolvedPath);
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue _AddSearchPath(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc < 1 || !argv[0].IsString())
            {
                return JSApi.JS_ThrowInternalError(ctx, "path expected");
            }
            var path = JSApi.GetString(ctx, argv[0]);
            if (string.IsNullOrEmpty(path))
            {
                return JSApi.JS_ThrowInternalError(ctx, "invalid path");
            }

            var runtime = ScriptEngine.GetRuntime(ctx);
            runtime.AddSearchPath(path);
            return JSApi.JS_UNDEFINED;
        }

        // arraybuffer => c# array<byte>
        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static unsafe JSValue to_cs_bytes(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc < 1)
            {
                return JSApi.JS_ThrowInternalError(ctx, "byte[] expected");
            }

            byte[] o;
            if (!Values.js_get_primitive_array(ctx, argv[0], out o))
            {
                return JSApi.JS_ThrowInternalError(ctx, "byte[] expected");
            }

            return Values.js_push_classvalue(ctx, o);
        }

        // c# array<byte> => js arraybuffer
        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static unsafe JSValue to_js_array_buffer(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc < 1)
            {
                return JSApi.JS_ThrowInternalError(ctx, "byte[] expected");
            }

            byte[] o;
            if (!Values.js_get_classvalue(ctx, argv[0], out o))
            {
                return JSApi.JS_ThrowInternalError(ctx, "byte[] expected");
            }
            if (o == null)
            {
                return JSApi.JS_NULL;
            }
            fixed (byte* mem_ptr = o)
            {
                return JSApi.JS_NewArrayBufferCopy(ctx, mem_ptr, o.Length);
            }
        }

        // c# array => js array
        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue to_js_array(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc < 1)
            {
                return JSApi.JS_ThrowInternalError(ctx, "array expected");
            }
            if (JSApi.JS_IsArray(ctx, argv[0]) == 1)
            {
                return JSApi.JS_DupValue(ctx, argv[0]);
            }

            Array o;
            if (!Values.js_get_classvalue<Array>(ctx, argv[0], out o))
            {
                return JSApi.JS_ThrowInternalError(ctx, "array expected");
            }
            if (o == null)
            {
                return JSApi.JS_NULL;
            }
            var len = o.Length;
            var rval = JSApi.JS_NewArray(ctx);
            try
            {
                for (var i = 0; i < len; i++)
                {
                    var obj = o.GetValue(i);
                    var elem = Values.js_push_var(ctx, obj);
                    JSApi.JS_SetPropertyUint32(ctx, rval, (uint)i, elem);
                }
            }
            catch (Exception exception)
            {
                JSApi.JS_FreeValue(ctx, rval);
                return JSApi.ThrowException(ctx, exception);
            }
            return rval;
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue yield_func(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc < 1)
            {
                return JSApi.JS_ThrowInternalError(ctx, "type YieldInstruction or Task expected");
            }
            object awaitObject;
            if (Values.js_get_cached_object(ctx, argv[0], out awaitObject))
            {
                var context = ScriptEngine.GetContext(ctx);
                return context.Yield(awaitObject);
            }

            return JSApi.JS_ThrowInternalError(ctx, "type YieldInstruction or Task expected");
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue js_import_type(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc < 1 || !argv[0].IsString())
            {
                return JSApi.JS_ThrowInternalError(ctx, "type_name expected");
            }

            var type_name = JSApi.GetString(ctx, argv[0]);
            var type = Assembly.GetExecutingAssembly().GetType(type_name);
            if (type == null)
            {
                return JSApi.JS_UNDEFINED;
            }

            var privateAccess = false;
            if (argc > 1 && argv[1].IsBoolean())
            {
                if (JSApi.JS_ToBool(ctx, argv[1]) == 1)
                {
                    privateAccess = true;
                }
            }

            var runtime = ScriptEngine.GetRuntime(ctx);
            var db = runtime.GetTypeDB();
            var proto = db.GetPrototypeOf(type);

            if (privateAccess)
            {
                var dynamicType = db.GetDynamicType(type);

                if (proto.IsNullish())
                {
                    proto = db.GetPrototypeOf(type);
                }

                if (dynamicType != null)
                {
                    dynamicType.OpenPrivateAccess();
                }
            }
            else
            {
                if (proto.IsNullish())
                {
                    db.GetDynamicType(type);
                    proto = db.GetPrototypeOf(type);
                }
            }

            return JSApi.JS_GetProperty(ctx, proto, JSApi.JS_ATOM_constructor);
        }

        //TODO: 临时代码
        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue hotfix_replace_single(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc < 3)
            {
                return JSApi.JS_ThrowInternalError(ctx, "type_name, func_name, func expected");
            }
            if (!argv[0].IsString() || !argv[1].IsString() || JSApi.JS_IsFunction(ctx, argv[2]) != 1)
            {
                return JSApi.JS_ThrowInternalError(ctx, "type_name, func_name expected");
            }

            var type_name = JSApi.GetString(ctx, argv[0]);
            var field_name = JSApi.GetString(ctx, argv[1]);
            var type = Assembly.GetExecutingAssembly().GetType(type_name);
            if (type == null)
            {
                return JSApi.JS_ThrowInternalError(ctx, "no such type");
            }

            var runtime = ScriptEngine.GetRuntime(ctx);
            var db = runtime.GetTypeDB();
            var hotfixBaseName = field_name != ".ctor" ? "_JSFIX_R_" + field_name + "_" : "_JSFIX_RC_ctor_";
            var hotfixSlot = 0;

            do
            {
                var hotfixName = hotfixBaseName + hotfixSlot;
                var field = type.GetField(hotfixName);
                if (field == null)
                {
                    if (hotfixSlot == 0)
                    {
                        return JSApi.JS_ThrowInternalError(ctx, "invalid hotfix point");
                    }
                    break;
                }
                Delegate d;
                if (Values.js_get_delegate(ctx, argv[2], field.FieldType, out d))
                {
                    field.SetValue(null, d);
                }
                ++hotfixSlot;
            } while (true);

            db.GetDynamicType(type, true);
            return JSApi.JS_UNDEFINED;
        }

        //TODO: 临时代码
        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue hotfix_before_single(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc < 3)
            {
                return JSApi.JS_ThrowInternalError(ctx, "type_name, func_name, func expected");
            }
            if (!argv[0].IsString() || !argv[1].IsString() || JSApi.JS_IsFunction(ctx, argv[2]) != 1)
            {
                return JSApi.JS_ThrowInternalError(ctx, "type_name, func_name expected");
            }

            var type_name = JSApi.GetString(ctx, argv[0]);
            var field_name = JSApi.GetString(ctx, argv[1]);
            var type = Assembly.GetExecutingAssembly().GetType(type_name);
            if (type == null)
            {
                return JSApi.JS_ThrowInternalError(ctx, "no such type");
            }

            var runtime = ScriptEngine.GetRuntime(ctx);
            var db = runtime.GetTypeDB();
            var hotfixBaseName = field_name != ".ctor" ? "_JSFIX_B_" + field_name + "_" : "_JSFIX_BC_ctor_";
            var hotfixSlot = 0;

            do
            {
                var hotfixName = hotfixBaseName + hotfixSlot;
                var field = type.GetField(hotfixName);
                if (field == null)
                {
                    if (hotfixSlot == 0)
                    {
                        return JSApi.JS_ThrowInternalError(ctx, "invalid hotfix point");
                    }
                    break;
                }
                Delegate d;
                if (Values.js_get_delegate(ctx, argv[2], field.FieldType, out d))
                {
                    field.SetValue(null, d);
                }
                ++hotfixSlot;
            } while (true);

            db.GetDynamicType(type, true);
            return JSApi.JS_UNDEFINED;
        }
    }
}