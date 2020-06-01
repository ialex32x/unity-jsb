using System;
using System.Collections.Generic;
using AOT;
using QuickJS.Utils;

namespace QuickJS.Binding
{
    using UnityEngine;
    using Native;

    public partial class Values
    {
        protected static JSValue BeginClass(TypeRegister register, ScriptContext ctx, JSValue ns, string typename, Type type, JSCFunctionMagic ctor)
        {
            var runtime = ctx.GetRuntime();
            var class_id = runtime._def_class_id;
            var proto_val = ctx.NewObject();
            var type_id = register.Add(type, proto_val);
            var ctor_val =
                JSApi.JS_NewCFunctionMagic(ctx, ctor, typename, 0, JSCFunctionEnum.JS_CFUNC_constructor_magic, type_id);
            JSApi.JS_SetConstructor(ctx, ctor_val, proto_val);
            JSApi.JS_SetClassProto(ctx, class_id, proto_val);
            JSApi.JS_DefinePropertyValueStr(ctx, ns, typename, ctor_val,
                JSPropFlags.JS_PROP_ENUMERABLE | JSPropFlags.JS_PROP_CONFIGURABLE);
            JSApi.JS_DupValue(ctx, proto_val);
            return proto_val;
        }

        protected static void EndClass(ScriptContext ctx, JSValue proto)
        {
            JSApi.JS_FreeValue(ctx, proto);
        }

        protected static JSValue BeginEnum(TypeRegister register, ScriptContext ctx, JSValue ns, string typename, Type type)
        {
            return BeginClass(register, ctx, ns, typename, type, class_private_ctor);
        }
        
        protected static void EndEnum(ScriptContext ctx, JSValue proto)
        {
            EndClass(ctx, proto);
        }

        // protected static void duk_begin_object(JSContext ctx, string objectname, Type type)
        // {
        //     DuktapeDLL.duk_push_object(ctx);
        //     DuktapeDLL.duk_dup(ctx, -1);
        //     // DuktapeDLL.duk_dup(ctx, -1);
        //     // DuktapeVM.GetVM(ctx).AddExported(type, new DuktapeFunction(ctx, DuktapeDLL.duk_unity_ref(ctx)));
        //     DuktapeDLL.duk_put_prop_string(ctx, -2, objectname);
        // }
        //
        // protected static void duk_end_object(JSContext ctx)
        // {
        //     DuktapeDLL.duk_pop(ctx);
        // }

        // protected static void duk_begin_special(JSContext ctx, string name)
        // {
        //     DuktapeDLL.duk_push_c_function(ctx, object_private_ctor, 0); // ctor
        //     DuktapeDLL.duk_dup(ctx, -1);
        //     DuktapeDLL.duk_dup(ctx, -1);
        //     var ptr = DuktapeDLL.duk_get_heapptr(ctx, -1);
        //     var typeValue = new DuktapeFunction(ctx, DuktapeDLL.duk_unity_ref(ctx), ptr); // ctor, ctor
        //     DuktapeVM.GetVM(ctx).AddSpecial(name, typeValue);
        //     DuktapeDLL.duk_put_prop_string(ctx, -3, name); // ctor
        //     DuktapeDLL.duk_push_object(ctx); // ctor, prototype
        //     DuktapeDLL.duk_dup_top(ctx); // ctor, prototype, prototype
        //     DuktapeDLL.duk_put_prop_string(ctx, -3, "prototype"); // ctor, prototype
        // }
        //
        // protected static void duk_end_special(JSContext ctx)
        // {
        //     DuktapeDLL.duk_pop_2(ctx); // remove [ctor, prototype]
        // }

        protected static void AddMethod(ScriptContext ctx, JSValue this_obj, string name, JSCFunctionMagic func, int length)
        {
            var magic = 0;
            var cfun = JSApi.JS_NewCFunctionMagic(ctx, func, name, length, JSCFunctionEnum.JS_CFUNC_generic_magic, magic);
            JSApi.JS_DefinePropertyValueStr(ctx, this_obj, name, cfun, JSPropFlags.JS_PROP_C_W_E);
        }

        protected static void AddField(ScriptContext ctx, JSValue this_obj, string name, JSCFunctionMagic getter, JSCFunctionMagic setter)
        {
            // js 层面field与property绑定代码结构完全一致
            AddProperty(ctx, this_obj, name, getter, setter);
        }

        protected static void AddProperty(ScriptContext ctx, JSValue this_obj, string name, JSCFunctionMagic getter, JSCFunctionMagic setter)
        {
            // [ctor, prototype]
            var getterVal = JSApi.JS_UNDEFINED;
            var setterVal = JSApi.JS_UNDEFINED;
            var flags = JSPropFlags.JS_PROP_HAS_CONFIGURABLE | JSPropFlags.JS_PROP_HAS_ENUMERABLE;
            if (getter != null)
            {
                flags |= JSPropFlags.JS_PROP_HAS_GET;
                getterVal = JSApi.JS_NewCFunctionMagic(ctx, getter, name, 0, JSCFunctionEnum.JS_CFUNC_getter_magic, 0);
            }
            if (setter != null)
            {
                flags |= JSPropFlags.JS_PROP_HAS_SET;
                setterVal = JSApi.JS_NewCFunctionMagic(ctx, setter, name, 0, JSCFunctionEnum.JS_CFUNC_setter_magic, 0);
            }
            // [ctor, prototype, name, ?getter, ?setter]
            var atom = JSApi.JS_NewAtom(ctx, name);
            JSApi.JS_DefineProperty(ctx, this_obj, atom, JSApi.JS_UNDEFINED, getterVal, setterVal, flags);
            JSApi.JS_FreeAtom(ctx, atom);
        }

        protected static void AddConstValue(ScriptContext ctx, JSValue this_obj, string name, bool v)
        {
            var val = JSApi.JS_NewBool(ctx, v);
            JSApi.JS_DefinePropertyValueStr(ctx, this_obj, name, val, JSPropFlags.CONST_VALUE);
        }

        protected static void AddConstValue(ScriptContext ctx, JSValue this_obj, string name, char v)
        {
            var val = JSApi.JS_NewInt32(ctx, v);
            JSApi.JS_DefinePropertyValueStr(ctx, this_obj, name, val, JSPropFlags.CONST_VALUE);
        }

        protected static void AddConstValue(ScriptContext ctx, JSValue this_obj, string name, byte v)
        {
            var val = JSApi.JS_NewInt32(ctx, v);
            JSApi.JS_DefinePropertyValueStr(ctx, this_obj, name, val, JSPropFlags.CONST_VALUE);
        }

        protected static void AddConstValue(ScriptContext ctx, JSValue this_obj, string name, sbyte v)
        {
            var val = JSApi.JS_NewInt32(ctx, v);
            JSApi.JS_DefinePropertyValueStr(ctx, this_obj, name, val, JSPropFlags.CONST_VALUE);
        }

        protected static void AddConstValue(ScriptContext ctx, JSValue this_obj, string name, short v)
        {
            var val = JSApi.JS_NewInt32(ctx, v);
            JSApi.JS_DefinePropertyValueStr(ctx, this_obj, name, val, JSPropFlags.CONST_VALUE);
        }

        protected static void AddConstValue(ScriptContext ctx, JSValue this_obj, string name, ushort v)
        {
            var val = JSApi.JS_NewInt32(ctx, v);
            JSApi.JS_DefinePropertyValueStr(ctx, this_obj, name, val, JSPropFlags.CONST_VALUE);
        }

        protected static void AddConstValue(ScriptContext ctx, JSValue this_obj, string name, int v)
        {
            var val = JSApi.JS_NewInt32(ctx, v);
            JSApi.JS_DefinePropertyValueStr(ctx, this_obj, name, val, JSPropFlags.CONST_VALUE);
        }

        // always static
        protected static void AddConstValue(ScriptContext ctx, JSValue this_obj, string name, uint v)
        {
            var val = JSApi.JS_NewUint32(ctx, v);
            JSApi.JS_DefinePropertyValueStr(ctx, this_obj, name, val, JSPropFlags.CONST_VALUE);
        }

        // always static
        protected static void AddConstValue(ScriptContext ctx, JSValue this_obj, string name, double v)
        {
            var val = JSApi.JS_NewFloat64(ctx, v);
            JSApi.JS_DefinePropertyValueStr(ctx, this_obj, name, val, JSPropFlags.CONST_VALUE);
        }

        protected static void AddConstValue(ScriptContext ctx, JSValue this_obj, string name, float v)
        {
            var val = JSApi.JS_NewFloat64(ctx, v);
            JSApi.JS_DefinePropertyValueStr(ctx, this_obj, name, val, JSPropFlags.CONST_VALUE);
        }

        // always static
        protected static void AddConstValue(ScriptContext ctx, JSValue this_obj, string name, string v)
        {
            var val = JSApi.JS_NewString(ctx, v);
            JSApi.JS_DefinePropertyValueStr(ctx, this_obj, name, val, JSPropFlags.CONST_VALUE);
        }

        // protected static void duk_add_event(JSContext ctx, string name, DuktapeDLL.duk_c_function add_op, DuktapeDLL.duk_c_function remove_op, int idx)
        // {
        //     idx = DuktapeDLL.duk_normalize_index(ctx, idx);
        //
        //     DuktapeDLL.duk_push_object(ctx);
        //     int refid;
        //     if (DuktapeDLL.duk_unity_get_refid(ctx, idx, out refid)) // 直接在 event object 上复制主对象的引用id
        //     {
        //         DuktapeDLL.duk_unity_set_refid(ctx, -1, refid);
        //     }
        //     DuktapeDLL.duk_push_c_function(ctx, add_op, 1);
        //     DuktapeDLL.duk_put_prop_string(ctx, -2, "on");
        //     DuktapeDLL.duk_push_c_function(ctx, remove_op, 1);
        //     DuktapeDLL.duk_put_prop_string(ctx, -2, "off");
        //     DuktapeDLL.duk_put_prop_string(ctx, idx, name);
        // }

        // protected static void duk_add_event_instanced(JSContext ctx, string name, DuktapeDLL.duk_c_function add_op, DuktapeDLL.duk_c_function remove_op, int idx)
        // {
        //     idx = DuktapeDLL.duk_normalize_index(ctx, idx);
        //
        //     DuktapeDLL.duk_push_object(ctx); // [evtobj]
        //     int refid;
        //     if (DuktapeDLL.duk_unity_get_refid(ctx, idx, out refid)) // 直接在 event object 上复制主对象的引用id
        //     {
        //         DuktapeDLL.duk_unity_set_refid(ctx, -1, refid);
        //     }
        //     DuktapeDLL.duk_push_string(ctx, name); // [evtobj, name]
        //     DuktapeDLL.duk_dup(ctx, -2); // [evtobj, name, evtobj]
        //     DuktapeDLL.duk_push_c_function(ctx, add_op, 1);
        //     DuktapeDLL.duk_put_prop_string(ctx, -2, "on");
        //     DuktapeDLL.duk_push_c_function(ctx, remove_op, 1);
        //     DuktapeDLL.duk_put_prop_string(ctx, -2, "off");
        //     // [evtobj, name, evtobj]
        //     DuktapeDLL.duk_def_prop(ctx, idx, DuktapeDLL.DUK_DEFPROP_HAVE_VALUE
        //                                     | DuktapeDLL.DUK_DEFPROP_SET_ENUMERABLE
        //                                     | DuktapeDLL.DUK_DEFPROP_CLEAR_CONFIGURABLE);
        //     // [evtobj]
        // }
    }
}
