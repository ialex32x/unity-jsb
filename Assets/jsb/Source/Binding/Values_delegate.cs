using System;
using System.Collections.Generic;
using AOT;
using QuickJS.Native;

namespace QuickJS.Binding
{
    using UnityEngine;

    // 处理委托的绑定
    public partial class Values
    {
        public static bool js_get_delegate_array<T>(JSContext ctx, JSValue val, out T[] o)
        where T : class
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
                o = new T[length];
                for (uint i = 0; i < length; i++)
                {
                    var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                    T e;
                    js_get_delegate(ctx, eVal, out e);
                    o[i] = e;
                    JSApi.JS_FreeValue(ctx, eVal);
                }
                return true;
            }
            js_get_classvalue<T[]>(ctx, val, out o);
            return true;
        }
        
        public static bool js_get_delegate<T>(JSContext ctx, JSValue val, out T o)
        where T : class
        {
            Delegate d;
            var ret = js_get_delegate(ctx, val, typeof(T), out d);
            o = ret ? d as T : null;
            return ret;
        }

        //TODO: !!! 重写 delegate 映射 提供专用的 JSValue ObjectType, 并通过 index 映射, 脱离 JSValue 本身 (避免不必要的引用管理)
        public static bool js_get_delegate(JSContext ctx, JSValue val, Type delegateType, out Delegate o)
        {
            //TODO: 20200320 !!! 如果 o 不是 jsobject, 且是 Delegate 但不是 ScriptDelegate, 则 ... 处理
            if (val.IsNullish())
            {
                o = null;
                return true;
            }
            
            if (JSApi.JS_IsObject(val) || JSApi.JS_IsFunction(ctx, val) == 1)
            {
                ScriptDelegate fn;
                var cache = ScriptEngine.GetObjectCache(ctx);
                
                if (cache.TryGetDelegate(val, out fn))
                {
                    // Debug.LogWarningFormat("cache hit {0}", heapptr);
                    o = fn.Match(delegateType);
                    return true;
                }

                // 默认赋值操作
                var types = ScriptEngine.GetTypeDB(ctx);
                var func = types.GetDelegateFunc(delegateType);

                if (func == null)
                {
                    o = null;
                    return false;
                }

                fn = new ScriptDelegate(ScriptEngine.GetContext(ctx), val);
                o = Delegate.CreateDelegate(delegateType, fn, func, true);
                fn.Add(o);
        
                // ScriptDelegate 拥有 js 对象的强引用, 此 js 对象无法释放 cache 中的 object, 所以这里用弱引用注册
                // 会出现的问题是, 如果 c# 没有对 ScriptDelegate 的强引用, 那么反复 get_delegate 会重复创建 ScriptDelegate
                // Debug.LogWarningFormat("cache create : {0}", heapptr);
                cache.AddDelegate(val, fn);
                return true;
            } 
            // else if (DuktapeDLL.duk_is_object(ctx, idx))
            // {
            //     return js_get_classvalue<T>(ctx, idx, out o);
            // }
            o = null;
            return false;
        }
    }
}
