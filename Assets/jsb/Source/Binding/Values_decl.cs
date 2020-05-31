using System;
using System.Collections.Generic;
using AOT;

namespace QuickJS.Binding
{
    using UnityEngine;
    using Native;

    public partial class Values
    {
        // 通用析构函数
        [MonoPInvokeCallback(typeof(JSApi.JSCFunction))]
        protected static JSValue class_dtor(JSRuntime rt, JSValue val)
        {
            int id;
            if (DuktapeDLL.duk_unity_get_refid(ctx, 0, out id))
            {
                // Debug.LogErrorFormat("remove refid {0}", id);
                DuktapeVM.GetObjectCache(ctx)?.RemoveObject(id);
            }
            return 0;
        }

        [MonoPInvokeCallback(typeof(JSApi.JSCFunction))]
        protected static JSValue class_private_ctor(JSContext ctx, JSValue this_val, int argc, JSValue[] argv)
        {
            return DuktapeDLL.duk_generic_error(ctx, "cant call constructor on this type");
        }

        [MonoPInvokeCallback(typeof(JSApi.JSCFunction))]
        protected static int class_dummy_ctor(JSContext ctx)
        {
            return 0;
        }

        [MonoPInvokeCallback(typeof(JSApi.JSCFunction))]
        protected static JSValue struct_dtor(JSRuntime rt, JSValue val)
        {
            int id;
            if (DuktapeDLL.duk_unity_get_refid(ctx, 0, out id))
            {
                // Debug.LogErrorFormat("remove refid {0}", id);
                DuktapeVM.GetObjectCache(ctx)?.RemoveObject(id);
            }
            return 0;
        }

        [MonoPInvokeCallback(typeof(JSApi.JSCFunction))]
        protected static JSValue struct_private_ctor(JSContext ctx, JSValue this_val, int argc, JSValue[] argv)
        {
            return DuktapeDLL.duk_generic_error(ctx, "cant call constructor on this type");
        }

        [MonoPInvokeCallback(typeof(JSApi.JSCFunction))]
        protected static JSValue struct_dummy_ctor(JSContext ctx, JSValue this_val, int argc, JSValue[] argv)
        {
            return 0;
        }

        public static bool duk_retrive_object(JSContext ctx, string el0)
        {
            DuktapeDLL.duk_dup_top(ctx);
            if (!DuktapeDLL.duk_get_prop_string(ctx, -1, el0)) // [parent, el0]
            {
                DuktapeDLL.duk_remove(ctx, -2);
                return false;
            }
            DuktapeDLL.duk_remove(ctx, -2);
            return true;
        }

        public static bool duk_retrive_object(JSContext ctx, string el0, string el1)
        {
            DuktapeDLL.duk_dup_top(ctx);
            if (!DuktapeDLL.duk_get_prop_string(ctx, -1, el0)) // [parent, el0]
            {
                DuktapeDLL.duk_remove(ctx, -2);
                return false;
            }
            DuktapeDLL.duk_remove(ctx, -2);
            if (!DuktapeDLL.duk_get_prop_string(ctx, -1, el1)) // [parent, el1]
            {
                DuktapeDLL.duk_remove(ctx, -2);
                return false;
            }
            DuktapeDLL.duk_remove(ctx, -2);
            return true;
        }

        public static bool duk_retrive_object(JSContext ctx, params string[] els)
        {
            DuktapeDLL.duk_dup_top(ctx);
            for (int i = 1, size = els.Length; i < size; i++)
            {
                var el = els[i];
                if (!DuktapeDLL.duk_get_prop_string(ctx, -1, el)) // [parent, el]
                {
                    DuktapeDLL.duk_remove(ctx, -2);
                    return false;
                }
                DuktapeDLL.duk_remove(ctx, -2);
            }
            return true;
        }

        // 无命名空间, 直接外围对象作为容器 (通常是global)
        public static void duk_begin_namespace(JSContext ctx) // [parent]
        {
            // Debug.LogFormat("begin namespace {0}", DuktapeDLL.duk_get_top(ctx));
            DuktapeDLL.duk_dup_top(ctx); // [parent, parent]
            // ScriptRuntime.GetContext(ctx).GetGlobalObject();
        }

        public static void duk_begin_namespace(JSContext ctx, string el) // [parent]
        {
            // Debug.LogFormat("begin namespace {0}", DuktapeDLL.duk_get_top(ctx));
            if (!DuktapeDLL.duk_get_prop_string(ctx, -1, el)) // [parent, el]
            {
                DuktapeDLL.duk_pop(ctx); // [parent]
                DuktapeDLL.duk_push_object(ctx); // [parent, new_object]
                DuktapeDLL.duk_dup_top(ctx); // [parent, new_object]
                DuktapeDLL.duk_put_prop_string(ctx, -3, el); // [parent, el]
            }
        }

        public static void duk_begin_namespace(JSContext ctx, string el1, string el2) // [parent]
        {
            duk_begin_namespace(ctx, el1); // [parent, el1]
            duk_begin_namespace(ctx, el2); // [parent, el1, el2]
            DuktapeDLL.duk_remove(ctx, -2); // [parent, el2]
        }

        public static void duk_begin_namespace(JSContext ctx, string el1, string el2, string el3) // [parent]
        {
            duk_begin_namespace(ctx, el1); // [parent, el1]
            duk_begin_namespace(ctx, el2); // [parent, el1, el2]
            DuktapeDLL.duk_remove(ctx, -2); // [parent, el2]
            duk_begin_namespace(ctx, el3); // [parent, el2, el3]
            DuktapeDLL.duk_remove(ctx, -2); // [parent, el3]
        }

        // return [parent, el]
        public static void duk_begin_namespace(JSContext ctx, params string[] els) // [parent]
        {
            duk_begin_namespace(ctx, els[0]); // [parent, el0]
            for (int i = 1, size = els.Length; i < size; i++)
            {
                var el = els[i];
                duk_begin_namespace(ctx, el); // [parent, eli-1, eli]
                DuktapeDLL.duk_remove(ctx, -2); // [parent, eli]
            }
        }

        public static void duk_end_namespace(JSContext ctx)
        {
            DuktapeDLL.duk_pop(ctx);
            // Debug.LogFormat("end namespace {0}", DuktapeDLL.duk_get_top(ctx));
        }

        protected static void duk_begin_object(JSContext ctx, string objectname, Type type)
        {
            DuktapeDLL.duk_push_object(ctx);
            DuktapeDLL.duk_dup(ctx, -1);
            // DuktapeDLL.duk_dup(ctx, -1);
            // DuktapeVM.GetVM(ctx).AddExported(type, new DuktapeFunction(ctx, DuktapeDLL.duk_unity_ref(ctx)));
            DuktapeDLL.duk_put_prop_string(ctx, -2, objectname);
        }

        protected static void duk_end_object(JSContext ctx)
        {
            DuktapeDLL.duk_pop(ctx);
        }

        protected static void duk_begin_special(JSContext ctx, string name)
        {
            DuktapeDLL.duk_push_c_function(ctx, object_private_ctor, 0); // ctor
            DuktapeDLL.duk_dup(ctx, -1);
            DuktapeDLL.duk_dup(ctx, -1);
            var ptr = DuktapeDLL.duk_get_heapptr(ctx, -1);
            var typeValue = new DuktapeFunction(ctx, DuktapeDLL.duk_unity_ref(ctx), ptr); // ctor, ctor
            DuktapeVM.GetVM(ctx).AddSpecial(name, typeValue);
            DuktapeDLL.duk_put_prop_string(ctx, -3, name); // ctor
            DuktapeDLL.duk_push_object(ctx); // ctor, prototype
            DuktapeDLL.duk_dup_top(ctx); // ctor, prototype, prototype
            DuktapeDLL.duk_put_prop_string(ctx, -3, "prototype"); // ctor, prototype
        }

        protected static void duk_end_special(JSContext ctx)
        {
            DuktapeDLL.duk_pop_2(ctx); // remove [ctor, prototype]
        }

        protected static void duk_begin_class(JSContext ctx, string typename, Type type, DuktapeDLL.duk_c_function ctor)
        {
            // Debug.LogFormat("begin class {0}", DuktapeDLL.duk_get_top(ctx));
            DuktapeDLL.duk_push_c_function(ctx, ctor, DuktapeDLL.DUK_VARARGS); // [ctor]
            DuktapeDLL.duk_dup(ctx, -1); // [ctor ctor]
            // Debug.LogFormat("begin check {0}", DuktapeDLL.duk_get_top(ctx));
            DuktapeDLL.duk_dup(ctx, -1); // [ctor ctor ctor]
            var ptr = DuktapeDLL.duk_get_heapptr(ctx, -1); 
            var typeid = DuktapeVM.GetVM(ctx).AddExportedType(type, new DuktapeFunction(ctx, DuktapeDLL.duk_unity_ref(ctx), ptr));
            DuktapeDLL.duk_unity_set_type_refid(ctx, -1, typeid); // constructor_function.!type == typeid
            // Debug.LogFormat("end check {0}", DuktapeDLL.duk_get_top(ctx));
            DuktapeDLL.duk_put_prop_string(ctx, -3, typename);
            DuktapeDLL.duk_push_object(ctx); // [ctor, prototype]
            DuktapeDLL.duk_dup_top(ctx); // [ctor, prototype, prototype]
            DuktapeDLL.duk_unity_set_type_refid(ctx, -1, typeid); // prototype.!type == typeid
            DuktapeDLL.duk_push_c_function(ctx, object_dtor, 1);
            DuktapeDLL.duk_set_finalizer(ctx, -3);  // set prototype finalizer
            DuktapeDLL.duk_put_prop_string(ctx, -3, "prototype"); // [ctor, prototype]
        }

        protected static void duk_end_class(JSContext ctx)
        {
            DuktapeDLL.duk_pop_2(ctx); // remove [ctor, prototype]
            // Debug.LogFormat("end class {0}", DuktapeDLL.duk_get_top(ctx));
        }

        protected static void duk_begin_enum(JSContext ctx, string typename, Type type)
        {
            duk_begin_class(ctx, typename, type, object_private_ctor);
        }

        protected static void duk_end_enum(JSContext ctx)
        {
            duk_end_class(ctx);
        }

        protected static void duk_add_method(JSContext ctx, string name, DuktapeDLL.duk_c_function func, int idx)
        {
            idx = DuktapeDLL.duk_normalize_index(ctx, idx);
            DuktapeDLL.duk_push_c_function(ctx, func, DuktapeDLL.DUK_VARARGS);
            DuktapeDLL.duk_put_prop_string(ctx, idx, name);
        }

        protected static void duk_add_field(JSContext ctx, string name, DuktapeDLL.duk_c_function getter, DuktapeDLL.duk_c_function setter, int idx)
        {
            // js 层面field与property绑定代码结构完全一致
            duk_add_property(ctx, name, getter, setter, idx);
        }

        protected static void duk_add_event(JSContext ctx, string name, DuktapeDLL.duk_c_function add_op, DuktapeDLL.duk_c_function remove_op, int idx)
        {
            idx = DuktapeDLL.duk_normalize_index(ctx, idx);

            DuktapeDLL.duk_push_object(ctx);
            int refid;
            if (DuktapeDLL.duk_unity_get_refid(ctx, idx, out refid)) // 直接在 event object 上复制主对象的引用id
            {
                DuktapeDLL.duk_unity_set_refid(ctx, -1, refid);
            }
            DuktapeDLL.duk_push_c_function(ctx, add_op, 1);
            DuktapeDLL.duk_put_prop_string(ctx, -2, "on");
            DuktapeDLL.duk_push_c_function(ctx, remove_op, 1);
            DuktapeDLL.duk_put_prop_string(ctx, -2, "off");
            DuktapeDLL.duk_put_prop_string(ctx, idx, name);
        }

        protected static void duk_add_event_instanced(JSContext ctx, string name, DuktapeDLL.duk_c_function add_op, DuktapeDLL.duk_c_function remove_op, int idx)
        {
            idx = DuktapeDLL.duk_normalize_index(ctx, idx);

            DuktapeDLL.duk_push_object(ctx); // [evtobj]
            int refid;
            if (DuktapeDLL.duk_unity_get_refid(ctx, idx, out refid)) // 直接在 event object 上复制主对象的引用id
            {
                DuktapeDLL.duk_unity_set_refid(ctx, -1, refid);
            }
            DuktapeDLL.duk_push_string(ctx, name); // [evtobj, name]
            DuktapeDLL.duk_dup(ctx, -2); // [evtobj, name, evtobj]
            DuktapeDLL.duk_push_c_function(ctx, add_op, 1);
            DuktapeDLL.duk_put_prop_string(ctx, -2, "on");
            DuktapeDLL.duk_push_c_function(ctx, remove_op, 1);
            DuktapeDLL.duk_put_prop_string(ctx, -2, "off");
            // [evtobj, name, evtobj]
            DuktapeDLL.duk_def_prop(ctx, idx, DuktapeDLL.DUK_DEFPROP_HAVE_VALUE
                                            | DuktapeDLL.DUK_DEFPROP_SET_ENUMERABLE
                                            | DuktapeDLL.DUK_DEFPROP_CLEAR_CONFIGURABLE);
            // [evtobj]
        }

        protected static void duk_add_property(JSContext ctx, string name, DuktapeDLL.duk_c_function getter, DuktapeDLL.duk_c_function setter, int idx)
        {
            // [ctor, prototype]
            idx = DuktapeDLL.duk_normalize_index(ctx, idx);
            var flags = 0U;
            DuktapeDLL.duk_push_string(ctx, name);
            if (getter != null)
            {
                flags |= DuktapeDLL.DUK_DEFPROP_HAVE_GETTER;
                DuktapeDLL.duk_push_c_function(ctx, getter, 0);
            }
            if (setter != null)
            {
                flags |= DuktapeDLL.DUK_DEFPROP_HAVE_SETTER;
                DuktapeDLL.duk_push_c_function(ctx, setter, 1);
            }
            // [ctor, prototype, name, ?getter, ?setter]
            DuktapeDLL.duk_def_prop(ctx, idx, flags
                                            | DuktapeDLL.DUK_DEFPROP_SET_ENUMERABLE
                                            | DuktapeDLL.DUK_DEFPROP_CLEAR_CONFIGURABLE);
        }

        protected static void duk_add_const(JSContext ctx, string name, bool v, int idx)
        {
            idx = DuktapeDLL.duk_normalize_index(ctx, idx);
            DuktapeDLL.duk_push_string(ctx, name);
            DuktapeDLL.duk_push_boolean(ctx, v);
            DuktapeDLL.duk_def_prop(ctx, idx, DuktapeDLL.DUK_DEFPROP_SET_ENUMERABLE
                                            | DuktapeDLL.DUK_DEFPROP_CLEAR_CONFIGURABLE
                                            | DuktapeDLL.DUK_DEFPROP_HAVE_VALUE
                                            | DuktapeDLL.DUK_DEFPROP_CLEAR_WRITABLE);
        }

        protected static void duk_add_const(JSContext ctx, string name, char v, int idx)
        {
            duk_add_const(ctx, name, (int)v, idx);
        }

        protected static void duk_add_const(JSContext ctx, string name, byte v, int idx)
        {
            duk_add_const(ctx, name, (int)v, idx);
        }

        protected static void duk_add_const(JSContext ctx, string name, sbyte v, int idx)
        {
            duk_add_const(ctx, name, (int)v, idx);
        }

        protected static void duk_add_const(JSContext ctx, string name, short v, int idx)
        {
            duk_add_const(ctx, name, (int)v, idx);
        }

        protected static void duk_add_const(JSContext ctx, string name, ushort v, int idx)
        {
            duk_add_const(ctx, name, (int)v, idx);
        }

        protected static void duk_add_const(JSContext ctx, string name, int v, int idx)
        {
            idx = DuktapeDLL.duk_normalize_index(ctx, idx);
            DuktapeDLL.duk_push_string(ctx, name);
            DuktapeDLL.duk_push_int(ctx, v);
            DuktapeDLL.duk_def_prop(ctx, idx, DuktapeDLL.DUK_DEFPROP_SET_ENUMERABLE
                                            | DuktapeDLL.DUK_DEFPROP_CLEAR_CONFIGURABLE
                                            | DuktapeDLL.DUK_DEFPROP_HAVE_VALUE
                                            | DuktapeDLL.DUK_DEFPROP_CLEAR_WRITABLE);
        }

        // always static
        protected static void duk_add_const(JSContext ctx, string name, uint v, int idx)
        {
            idx = DuktapeDLL.duk_normalize_index(ctx, idx);
            DuktapeDLL.duk_push_string(ctx, name);
            DuktapeDLL.duk_push_uint(ctx, v);
            DuktapeDLL.duk_def_prop(ctx, idx, DuktapeDLL.DUK_DEFPROP_SET_ENUMERABLE
                                            | DuktapeDLL.DUK_DEFPROP_CLEAR_CONFIGURABLE
                                            | DuktapeDLL.DUK_DEFPROP_HAVE_VALUE
                                            | DuktapeDLL.DUK_DEFPROP_CLEAR_WRITABLE);
        }

        // always static
        protected static void duk_add_const(JSContext ctx, string name, double v, int idx)
        {
            idx = DuktapeDLL.duk_normalize_index(ctx, idx);
            DuktapeDLL.duk_push_string(ctx, name);
            DuktapeDLL.duk_push_number(ctx, v);
            DuktapeDLL.duk_def_prop(ctx, idx, DuktapeDLL.DUK_DEFPROP_SET_ENUMERABLE
                                            | DuktapeDLL.DUK_DEFPROP_CLEAR_CONFIGURABLE
                                            | DuktapeDLL.DUK_DEFPROP_HAVE_VALUE
                                            | DuktapeDLL.DUK_DEFPROP_CLEAR_WRITABLE);
        }

        protected static void duk_add_const(JSContext ctx, string name, float v, int idx)
        {
            duk_add_const(ctx, name, (double)v, idx);
        }

        // always static
        protected static void duk_add_const(JSContext ctx, string name, string v, int idx)
        {
            idx = DuktapeDLL.duk_normalize_index(ctx, idx);
            DuktapeDLL.duk_push_string(ctx, name);
            DuktapeDLL.duk_push_string(ctx, v);
            DuktapeDLL.duk_def_prop(ctx, idx, DuktapeDLL.DUK_DEFPROP_SET_ENUMERABLE
                                            | DuktapeDLL.DUK_DEFPROP_CLEAR_CONFIGURABLE
                                            | DuktapeDLL.DUK_DEFPROP_HAVE_VALUE
                                            | DuktapeDLL.DUK_DEFPROP_CLEAR_WRITABLE);
        }
    }
}
