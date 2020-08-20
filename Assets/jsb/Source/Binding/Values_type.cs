using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickJS.Binding
{
    using Native;
    using UnityEngine;
    using Utils;

    // 处理类型
    public partial class Values
    {
        public static bool js_get_type(JSContext ctx, JSValue jsValue, out Type o)
        {
            if (JSApi.JS_IsString(jsValue))
            {
                var name = JSApi.GetString(ctx, jsValue);
                o = TypeDB.GetType(name);
                return o != null;
            }
            else
            {
                var context = ScriptEngine.GetContext(ctx);
                var type_id = JSApi.JSB_GetBridgeType(ctx, jsValue, context.GetAtom(Values.KeyForCSharpTypeID));
                if (type_id >= 0)
                {
                    var types = context.GetTypeDB();
                    o = types.GetType(type_id);
                    // Debug.Log($"get type from exported registry {o}:{typeid}");
                    return o != null;
                }
                else
                {
                    var header = JSApi.jsb_get_payload_header(jsValue);
                    switch (header.type_id)
                    {
                        case BridgeObjectType.TypeRef:
                        {
                            var types = context.GetTypeDB();
                            o = types.GetType(header.value);
                            // Debug.Log($"get type from exported registry {o}:{typeid}");
                            return o != null;
                        }
                        case BridgeObjectType.ObjectRef:
                        {
                            var cache = context.GetObjectCache();
                            object obj;
                            cache.TryGetObject(header.value, out obj);
                            o = obj.GetType();
                            return o != null;
                        }
                    }
                }
            }
            
            o = null;
            return false;
        }

        public static bool js_get_type_array(JSContext ctx, JSValue val, out Type[] o)
        {
            if (JSApi.JS_IsArray(ctx, val) == 1)
            {
                var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                if (JSApi.JS_IsException(lengthVal))
                {
                    throw new Exception(ctx.GetExceptionString());
                }
                uint length;
                JSApi.JSB_ToUint32(ctx, out length, lengthVal);
                JSApi.JS_FreeValue(ctx, lengthVal);
                o = new Type[length];
                for (uint i = 0; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    Type e;
                    js_get_type(ctx, eVal, out e);
                    o[i] = e;
                    JSApi.JS_FreeValue(ctx, eVal);
                }
                return true;
            }
            
            // fallthrough
            return js_get_classvalue<Type[]>(ctx, val, out o);
        }
    }
}
