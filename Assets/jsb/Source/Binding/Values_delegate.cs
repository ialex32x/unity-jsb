using System;
using System.Runtime.CompilerServices;
using QuickJS.Native;

namespace QuickJS.Binding
{
    // 处理委托的绑定
    public partial class Values
    {
        public static JSValue js_new_event(JSContext ctx, object this_obj, JSCFunction adder, JSCFunction remover)
        {
            var context = ScriptEngine.GetContext(ctx);
            var ret = NewBridgeClassObject(ctx, this_obj);
            var adderFunc = JSApi.JSB_NewCFunction(ctx, adder, context.GetAtom("on"), 1, JSCFunctionEnum.JS_CFUNC_generic, 0);
            JSApi.JS_SetProperty(ctx, ret, context.GetAtom("on"), adderFunc);
            var removerFunc = JSApi.JSB_NewCFunction(ctx, remover, context.GetAtom("off"), 1, JSCFunctionEnum.JS_CFUNC_generic, 0);
            JSApi.JS_SetProperty(ctx, ret, context.GetAtom("off"), removerFunc);
            return ret;
        }

        // // 创建一个委托绑定
        // // add/remove 只在 可读&可写 有效
        // // get/set 对应 可读/可写
        // public static JSValue js_new_delegate(JSContext ctx, object this_obj, JSCFunction adder, JSCFunction remover, JSCFunction setter, JSCFunction getter)
        // {
        //     var context = ScriptEngine.GetContext(ctx);
        //     var ret = NewBridgeClassObject(ctx, this_obj);
        //     if (adder != null)
        //     {
        //         var adderFunc = JSApi.JSB_NewCFunction(ctx, adder, context.GetAtom("on"), 1, JSCFunctionEnum.JS_CFUNC_generic, 0);
        //         JSApi.JS_SetProperty(ctx, ret, context.GetAtom("on"), adderFunc);
        //     }
        //     if (remover != null)
        //     {
        //         var removerFunc = JSApi.JSB_NewCFunction(ctx, remover, context.GetAtom("off"), 1, JSCFunctionEnum.JS_CFUNC_generic, 0);
        //         JSApi.JS_SetProperty(ctx, ret, context.GetAtom("off"), removerFunc);
        //     }
        //     return ret;
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue js_push_delegate(JSContext ctx, Delegate o)
        {
            if (o == null)
            {
                return JSApi.JS_NULL;
            }

            var dDelegate = o.Target as ScriptDelegate;
            if (dDelegate != null)
            {
                return JSApi.JS_DupValue(ctx, dDelegate);
            }

            // fallback
            return js_push_object(ctx, (object)o);
        }

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

        // 从 JSValue 反推 Delegate
        // 不约束委托类型 (因此也不会自动创建委托, 不存在已有映射时, 将失败)
        public static bool js_get_delegate_unsafe(JSContext ctx, JSValue val, out Delegate o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }

            if (JSApi.JS_IsFunction(ctx, val) == 1)
            {
                ScriptDelegate fn;
                var cache = ScriptEngine.GetObjectCache(ctx);

                if (cache.TryGetDelegate(val, out fn))
                {
                    o = fn.Any();
                    return o != null;
                }
                else
                {
                    o = null;
                    return false;
                }
            }

            // 检查 val 是否是一个委托对象 wrapped object
            if (JSApi.JS_IsObject(val))
            {
                return js_get_classvalue<Delegate>(ctx, val, out o);
            }

            o = null;
            return false;        }

        // 从 JSValue 反推 Delegate
        // JSValue 可能是一个 js function, cs delegate (js object)
        public static bool js_get_delegate(JSContext ctx, JSValue val, Type delegateType, out Delegate o)
        {
            if (val.IsNullish())
            {
                o = null;
                return true;
            }

            if (JSApi.JS_IsFunction(ctx, val) == 1)
            {
                ScriptDelegate fn;
                var cache = ScriptEngine.GetObjectCache(ctx);

                if (cache.TryGetDelegate(val, out fn))
                {
                    // 已经存在映射关系, 找出符合预期类型的委托
                    o = fn.Match(delegateType);
                    if (o == null)
                    {
                        // 存在 JSValue => Delegate 的多重映射
                        var types = ScriptEngine.GetTypeDB(ctx);
                        var func = types.GetDelegateFunc(delegateType);
                        o = Delegate.CreateDelegate(delegateType, fn, func, false);
                        if (o != null)
                        {
                            fn.Add(o);
                        }
                    }
                    return o != null;
                }
                else
                {
                    // 建立新的映射关系
                    var context = ScriptEngine.GetContext(ctx);
                    var types = context.GetTypeDB();
                    var func = types.GetDelegateFunc(delegateType);

                    if (func == null)
                    {
                        o = null;
                        return false;
                    }

                    fn = new ScriptDelegate(context, val);
                    o = Delegate.CreateDelegate(delegateType, fn, func, false);
                    if (o != null)
                    {
                        fn.Add(o);
                    }

                    return o != null;
                }
            }

            // 检查 val 是否是一个委托对象 wrapped object
            if (JSApi.JS_IsObject(val))
            {
                if (js_get_classvalue<Delegate>(ctx, val, out o))
                {
                    return o == null || o.GetType() == delegateType;
                }
            }

            o = null;
            return false;
        }
    }
}
