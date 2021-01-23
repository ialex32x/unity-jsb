using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using Native;

    public partial class Values
    {
        public const string KeyForCSharpTypeID = "__csharp_type_id__";

        private static float[] _matrix_floats_buffer = new float[16];

        static Values()
        {
            init_js_cast_map();
            init_cs_cast_map();
        }

        public static bool IsVarargParameter(ParameterInfo[] parameters)
        {
            return parameters.Length > 0 && parameters[parameters.Length - 1].IsDefined(typeof(ParamArrayAttribute), false);
        }

        public static bool IsContextualType(Type pType)
        {
            return pType == typeof(JSContext) || pType == typeof(JSRuntime)
                || pType == typeof(ScriptContext) || pType == typeof(ScriptRuntime);
        }

        public static object js_get_context(JSContext ctx, Type type)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue FindPrototypeOf(JSContext ctx, Type type)
        {
            int type_id;
            var types = ScriptEngine.GetTypeDB(ctx);
            return types.FindChainedPrototypeOf(type, out type_id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool js_script_error(JSContext ctx)
        {
            var logger = ScriptEngine.GetLogger(ctx);
            if (logger != null)
            {
                logger.Write(LogLevel.Error, ctx.GetExceptionString());
            }
            return false;
        }
    }
}
