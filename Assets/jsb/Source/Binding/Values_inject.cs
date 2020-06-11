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
        // inject MonoBehaviour.Constructor
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static JSValue _js_mono_behaviour_constructor(JSContext ctx, JSValue new_target)
        {
            var proto = JSApi.JS_GetProperty(ctx, new_target, JSApi.JS_ATOM_prototype);
            if (!proto.IsException())
            {
                var val = JSApi.JS_NewObjectProtoClass(ctx, proto, JSApi.JSB_GetBridgeClassID());
                JSApi.JS_FreeValue(ctx, proto);
                return val;
            }

            return proto;
        }
        
        // inject GameObject.AddComponent(Type);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static JSValue _js_game_object_add_component(JSContext ctx, JSValue ctor, GameObject gameObject, Type type)
        {
            if (JSApi.JS_IsConstructor(ctx, ctor) == 1)
            {
                var header = JSApi.jsb_get_payload_header(ctor);
                if (header.type_id == BridgeObjectType.None) // it's a plain js value
                {
                    if (type == typeof(MonoBehaviour))
                    {
                        var bridge = gameObject.AddComponent<ScriptBridge>();
                        var cache = ScriptEngine.GetObjectCache(ctx);
                        var object_id = cache.AddObject(bridge);
                        var val = JSApi.jsb_construct_bridge_object(ctx, ctor, object_id);
                        if (val.IsException())
                        {
                            cache.RemoveObject(object_id);
                        }
                        else
                        {
                            bridge.SetBridge(ctx, val, ctor);
                            // JSApi.JSB_SetBridgeType(ctx, val, type_id);
                        }

                        return val;
                    }
                }
            }

            return JSApi.JS_UNDEFINED;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static JSValue _js_game_object_get_component(JSContext ctx, JSValue ctor, GameObject gameObject, Type type)
        {
            if (JSApi.JS_IsConstructor(ctx, ctor) == 1)
            {
                var header = JSApi.jsb_get_payload_header(ctor);
                if (header.type_id == BridgeObjectType.None) // it's a plain js value
                {
                    if (type == typeof(MonoBehaviour))
                    {
                        var allBridges = gameObject.GetComponents<ScriptBridge>();
                        for (int i = 0, size = allBridges.Length; i < size; i++)
                        {
                            var bridge = allBridges[i];
                            var instanceOf = bridge.IsInstanceOf(ctor);
                            if (instanceOf == 1)
                            {
                                return bridge.CloneValue();
                            }

                            if (instanceOf == -1)
                            {
                                ctx.print_exception();
                            }
                        }

                        return JSApi.JS_NULL; // or return an empty array?
                    }
                }
            }

            return JSApi.JS_UNDEFINED;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static JSValue _js_game_object_get_components(JSContext ctx, JSValue ctor, GameObject gameObject, Type type)
        {
            if (JSApi.JS_IsConstructor(ctx, ctor) == 1)
            {
                var header = JSApi.jsb_get_payload_header(ctor);
                if (header.type_id == BridgeObjectType.None) // it's a plain js value
                {
                    if (type == typeof(MonoBehaviour))
                    {
                        var array = JSApi.JS_NewArray(ctx);
                        var length = 0;
                        var allBridges = gameObject.GetComponents<ScriptBridge>();
                        for (int i = 0, size = allBridges.Length; i < size; i++)
                        {
                            var bridge = allBridges[i];
                            var instanceOf = bridge.IsInstanceOf(ctor);
                            if (instanceOf == 1)
                            {
                                JSApi.JS_SetPropertyUint32(ctx, array, (uint) length, bridge.CloneValue());
                                length++;
                            }

                            if (instanceOf == -1)
                            {
                                ctx.print_exception();
                            }
                        }

                        return array; // or return an empty array?
                    }
                }
            }

            return JSApi.JS_UNDEFINED;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static JSValue _js_game_object_get_components(JSContext ctx, JSValue ctor, GameObject gameObject, Type type, List<Component> results)
        {
            if (JSApi.JS_IsConstructor(ctx, ctor) == 1)
            {
                var header = JSApi.jsb_get_payload_header(ctor);
                if (header.type_id == BridgeObjectType.None) // it's a plain js value
                {
                    if (type == typeof(MonoBehaviour))
                    {
                        var allBridges = gameObject.GetComponents<ScriptBridge>();
                        for (int i = 0, size = allBridges.Length; i < size; i++)
                        {
                            var bridge = allBridges[i];
                            var instanceOf = bridge.IsInstanceOf(ctor);
                            if (instanceOf == 1)
                            {
                                results.Add(bridge);
                            }

                            if (instanceOf == -1)
                            {
                                ctx.print_exception();
                            }
                        }

                        return JSApi.JS_NULL; 
                    }
                }
            }

            return JSApi.JS_UNDEFINED;
        }
    }
}