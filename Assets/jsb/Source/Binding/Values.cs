using System;
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
