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
