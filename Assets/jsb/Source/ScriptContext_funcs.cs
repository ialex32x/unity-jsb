using System;
using System.Reflection;
using System.Text;
using QuickJS.Binding;
using QuickJS.Native;

namespace QuickJS
{
    public partial class ScriptContext
    {
        private static Type FindType(string type_name)
        {
            Type type = null; //Assembly.GetExecutingAssembly().GetType(type_name);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0, count = assemblies.Length; i < count; i++)
            {
                var assembly = assemblies[i];
                type = assembly.GetType(type_name);
                if (type != null)
                {
                    break;
                }
            }
            return type;
        }

        #region Builtins

        [MonoPInvokeCallback(typeof(JSCFunction))]
        private static JSValue _sleep(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            int pres = 0;
            if (argc > 0)
            {
                if (JSApi.JS_ToInt32(ctx, out pres, argv[0]) != 0)
                {
                    return JSApi.JS_ThrowInternalError(ctx, "invalid parameter: milliseconds");
                }
            }
            if (pres > 0)
            {
                System.Threading.Thread.Sleep(pres);
            }
            return JSApi.JS_UNDEFINED;
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        private static JSValue _gc(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            // var runtime = ScriptEngine.GetRuntime(ctx);
            // runtime.EnqueueAction(new JSAction() { callback = _RunGC });
            var rt = JSApi.JS_GetRuntime(ctx);
            JSApi.JS_RunGC(rt);
            GC.Collect();
            GC.WaitForPendingFinalizers();

            return JSApi.JS_UNDEFINED;
        }

        // private static void _RunGC(ScriptRuntime rt, JSAction value)
        // {
        //     JSApi.JS_RunGC(rt);
        //     GC.Collect();
        //     GC.WaitForPendingFinalizers();
        // }

        [MonoPInvokeCallback(typeof(JSCFunctionMagic))]
        private static JSValue _print(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv, int magic)
        {
            if (argc == 0 && magic >= 0)
            {
                return JSApi.JS_UNDEFINED;
            }

            var runtime = ScriptEngine.GetRuntime(ctx);
            if (runtime == null)
            {
                return JSApi.JS_UNDEFINED;
            }

            int i = 0;

            if (magic == (int)LogLevel.Assert)
            {
                if (JSApi.JS_ToBool(ctx, argv[0]) == 1)
                {
                    return JSApi.JS_UNDEFINED;
                }
                i = 1;
            }

            var logger = runtime.GetLogger();
            if (logger == null)
            {
                return JSApi.JS_UNDEFINED;
            }
            var sb = new StringBuilder();
            size_t str_len;

            for (; i < argc; i++)
            {
                var pstr = JSApi.JS_ToCStringLen(ctx, out str_len, argv[i]);
                if (pstr == IntPtr.Zero)
                {
                    return JSApi.JS_EXCEPTION;
                }

                var str = JSApi.GetString(ctx, pstr, str_len);
                if (str != null)
                {
                    sb.Append(str);
                }

                JSApi.JS_FreeCString(ctx, pstr);
                if (i != argc - 1)
                {
                    sb.Append(' ');
                }
            }

            var logLevel = magic == -1 ? LogLevel.Info : (LogLevel)magic;
            if (magic == -1 || logLevel > LogLevel.Warn || runtime.withStacktrace)
            {
                sb.AppendLine();
                runtime.GetContext(ctx).AppendStacktrace(sb);
            }

            try
            {
                logger.Write(logLevel, sb.ToString());
            }
            catch (Exception)
            {
                // Debug.LogErrorFormat("Logger Exception: {0}\n{1}", exception, exception.StackTrace);
            }

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

            //TODO: use runtime.EvalFile instead

            var context = ScriptEngine.GetContext(ctx);
            var runtime = context.GetRuntime();
            var fileSystem = runtime.GetFileSystem();
            var resolvedPath = runtime.ResolveFilePath("", path);
            if (resolvedPath == null)
            {
                return JSApi.JS_ThrowInternalError(ctx, "file not found");
            }
            var source = fileSystem.ReadAllBytes(resolvedPath);
            return ScriptRuntime.EvalSource(ctx, source, resolvedPath, true);
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

        // 尝试将传入的委托转换为 js function
        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static unsafe JSValue to_js_function(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc >= 1)
            {
                var make_dynamic = argc >= 2 && JSApi.JS_ToBool(ctx, argv[1]) == 1;

                if (JSApi.JS_IsFunction(ctx, argv[0]) == 1)
                {
                    return JSApi.JS_DupValue(ctx, argv[0]);
                }

                if (JSApi.JS_IsObject(argv[0]))
                {
                    Delegate o;
                    if (Values.js_get_delegate_unsafe(ctx, argv[0], out o))
                    {
                        if (o != null)
                        {
                            var sd = o.Target as ScriptDelegate;
                            if (sd != null)
                            {
                                return JSApi.JS_DupValue(ctx, sd);
                            }

                            // 尝试将传入的委托转换为 js function
                            // c# delegate 通过 dynamic method wrapper 产生一个 jsvalue 
                            // 谨慎: 无法再从 function 还原此委托, 两者不会建立关联 (构成强引用循环)
                            // 谨慎: NewDynamicDelegate 会产生一个与 Runtime 相同生命周期的对象, 该对象将持有 Delegate 对象引用
                            if (make_dynamic)
                            {
                                var context = ScriptEngine.GetContext(ctx);
                                var types = context.GetTypeDB();
                                var name = context.GetAtom(o.Method.Name);

                                return types.NewDynamicDelegate(name, o);
                            }
                        }
                    }
                }
            }

            return JSApi.JS_UNDEFINED;
        }

        // 尝试将传入的 function 转换为 cs delegate
        // 暂时只转换 ScriptDelegate
        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static unsafe JSValue to_cs_delegate(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc == 2)
            {
                var type_name = JSApi.GetString(ctx, argv[1]);
                var type = FindType(type_name);
                if (type != null)
                {
                    Delegate d;
                    if (Values.js_get_delegate(ctx, argv[0], type, out d))
                    {
                        return Values.js_push_object(ctx, d);
                    }
                }
            }

            return JSApi.JS_ThrowInternalError(ctx, "invalid parameters");
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
                var co = context.GetCoroutineManager();
                if (co != null)
                {
                    return co.Yield(context, awaitObject);
                }

                return JSApi.JS_ThrowInternalError(ctx, "no async manager");
                // return context.Yield(awaitObject);
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
            var type = FindType(type_name);

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