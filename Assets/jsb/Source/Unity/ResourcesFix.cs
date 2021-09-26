#if !JSB_UNITYLESS
#if UNITY_EDITOR
using System;

namespace QuickJS.Unity
{
    using Native;
    using Binding;
    using UnityEngine;

    public static class ResourcesFix
    {
        [MonoPInvokeCallbackAttribute(typeof(JSCFunction))]
        public static JSValue BindStatic_FindObjectsOfTypeAll(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            try
            {
                if (argc == 1)
                {
                    System.Type arg_type;
                    if (!Values.js_get_classvalue(ctx, argv[0], out arg_type))
                    {
                        throw new ParameterException(typeof(Resources), "FindObjectsOfTypeAll", typeof(System.Type), 0);
                    }
                    var inject = js_find_objects_of_type_all(ctx, argv[0], arg_type);
                    if (!inject.IsUndefined())
                    {
                        return inject;
                    }
                    var ret = Resources.FindObjectsOfTypeAll(arg_type);
                    return ObjectFix.js_push_as_array(ctx, ret);
                }

                throw new NotImplementedException();
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        public static JSValue js_find_objects_of_type_all(JSContext ctx, JSValue ctor, Type type)
        {
            if (JSApi.JS_IsConstructor(ctx, ctor) == 1)
            {
                var header = JSApi.jsb_get_payload_header(ctor);

                //TODO how to distinctly determine whether ctor is script-defined type
                if (header.type_id == BridgeObjectType.None) // it's a plain js value
                {
                    if (type == typeof(Object) || type.IsSubclassOf(typeof(Object)))
                    {
                        var objects = Resources.FindObjectsOfTypeAll(type);
                        var len = objects.Length;
                        var array = JSApi.JS_NewArray(ctx);
                        var arrayLength = 0;

                        for (var i = 0; i < len; ++i)
                        {
                            var obj = objects[i];

                            if (typeof(IScriptInstancedObject).IsAssignableFrom(obj.GetType()))
                            {
                                var si = (IScriptInstancedObject)obj;
                                if (si.IsInstanceOf(ctor) == 1)
                                {
                                    JSApi.JS_SetPropertyUint32(ctx, array, (uint)arrayLength, si.CloneValue());
                                    arrayLength++;
                                }
                                continue;
                            }
                        }

                        return array;
                    }
                }
            }

            return JSApi.JS_UNDEFINED;
        }
    }
}

#endif // !JSB_UNITYLESS
#endif // UNITY_EDITOR