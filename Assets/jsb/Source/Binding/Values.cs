using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using Native;
    using UnityEngine;


    public partial class Values
    {
        protected static HashSet<Type> _assignableFromArray = new HashSet<Type>();

        static Values()
        {
            _assignableFromArray.Add(typeof(LayerMask));
            _assignableFromArray.Add(typeof(Color));
            _assignableFromArray.Add(typeof(Color32));
            _assignableFromArray.Add(typeof(Vector2));
            _assignableFromArray.Add(typeof(Vector2Int));
            _assignableFromArray.Add(typeof(Vector3));
            _assignableFromArray.Add(typeof(Vector3Int));
            _assignableFromArray.Add(typeof(Vector4));
            _assignableFromArray.Add(typeof(Quaternion));
            _assignableFromArray.Add(typeof(Matrix4x4));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue FindPrototypeOf(JSContext ctx, Type type, out int type_id)
        {
            var types = ScriptEngine.GetTypeDB(ctx);
            return types.FindPrototypeOf(type, out type_id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue FindPrototypeOf(JSContext ctx, Type type)
        {
            int type_id;
            var types = ScriptEngine.GetTypeDB(ctx);
            return types.FindPrototypeOf(type, out type_id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool js_script_error(JSContext ctx)
        {
            var logger = ScriptEngine.GetLogger(ctx);
            logger.ScriptWrite(LogLevel.Error, ctx.GetExceptionString());
            return false;
        }
    }
}