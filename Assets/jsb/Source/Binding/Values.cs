using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using Native;

    public partial class Values
    {
        public const string KeyForCSharpTypeID = "__csharp_type_id__";
        public const string NamespaceOfStaticBinder = "QuickJS";
        public const string ClassNameOfStaticBinder = "StaticBinder";
        public const string MethodNameOfStaticBinder = "BindAll";

        static Values()
        {
            init_cast_map();
        }

        public static bool IsVarargParameter(ParameterInfo[] parameters)
        {
            return parameters.Length > 0 && parameters[parameters.Length - 1].IsDefined(typeof(ParamArrayAttribute), false);
        }

        /// <summary>
        /// globally find a type with FullName
        /// </summary>
        public static Type FindType(string type_name)
        {
            Type type = null; //Assembly.GetExecutingAssembly().GetType(type_name);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0, count = assemblies.Length; i < count; i++)
            {
                var assembly = assemblies[i];
                if (!assembly.IsDynamic)
                {
                    type = assembly.GetType(type_name);
                    if (type != null)
                    {
                        break;
                    }
                }
            }
            return type;
        }

        public static bool IsContextualType(Type pType)
        {
            return pType == typeof(JSContext) || pType == typeof(JSRuntime)
                || pType == typeof(ScriptContext) || pType == typeof(ScriptRuntime);
        }

        /// <summary>
        /// explicitly push as JSArray
        /// </summary>
        public static unsafe JSValue PushArray(JSContext ctx, object o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }
            if (!(o is Array))
            {
                return JSApi.ThrowException(ctx, new InvalidCastException($"fail to cast type to Array"));
            }
            var arr = (Array)o;
            var length = arr.Length;
            var rval = JSApi.JS_NewArray(ctx);
            try
            {
                for (var i = 0; i < length; i++)
                {
                    var obj = arr.GetValue(i);
                    var elem = Values.js_push_object(ctx, obj);
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

        public static object GetContext(JSContext ctx, Type type)
        {
            if (type == typeof(JSContext))
            {
                return ctx;
            }

            if (type == typeof(JSRuntime))
            {
                return JSApi.JS_GetRuntime(ctx);
            }

            if (type == typeof(ScriptContext))
            {
                return ScriptEngine.GetContext(ctx);
            }

            if (type == typeof(ScriptRuntime))
            {
                return ScriptEngine.GetRuntime(ctx);
            }

            return null;
        }

        public static JSValue FindPrototypeOf<T>(JSContext ctx)
        {
            int type_id;
            var types = ScriptEngine.GetTypeDB(ctx);
            return types.FindChainedPrototypeOf(typeof(T), out type_id);
        }

        public static JSValue FindPrototypeOf(JSContext ctx, Type type)
        {
            int type_id;
            var types = ScriptEngine.GetTypeDB(ctx);
            return types.FindChainedPrototypeOf(type, out type_id);
        }

        protected static bool WriteScriptError(JSContext ctx)
        {
            var logger = ScriptEngine.GetLogger(ctx);
            if (logger != null)
            {
                logger.Write(Utils.LogLevel.Error, ctx.GetExceptionString());
            }
            return false;
        }
    }
}
