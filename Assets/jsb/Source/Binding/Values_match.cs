using System;
using System.Collections.Generic;
using AOT;

namespace QuickJS.Binding
{
    using UnityEngine;

    // 处理类型匹配
    public partial class Values
    {
        protected static HashSet<Type> _assignableFromArray = new HashSet<Type>();

        static DuktapeBinding()
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
            // _assignableFromArray.Add(typeof(Matrix4x4));
        }

        protected static bool duk_match_type(IntPtr ctx, int idx, Type type)
        {
            if (type == null)
            {
                return true;
            }
            if (type == typeof(object))
            {
                return true;
            }
            if (type == typeof(Type))
            {
                Type otype;
                return duk_get_type(ctx, idx, out otype); // 只要求匹配 Type 本身, 不比较具体 Type
                // return otype == type;
            }
            var jstype = DuktapeDLL.duk_get_type(ctx, idx);
            switch (jstype)
            {
                // case duk_type_t.DUK_TYPE_NONE:
                case duk_type_t.DUK_TYPE_OBJECT: // objects, arrays, functions, threads
                    if (DuktapeDLL.duk_is_array(ctx, idx))
                    {
                        if (!type.IsArray && !_assignableFromArray.Contains(type))
                        {
                            return false;
                        }
                    }
                    else if (DuktapeDLL.duk_is_function(ctx, idx))
                    {
                        //TODO: 完善处理 delegate 
                        return type == typeof(DuktapeFunction) || type.BaseType == typeof(MulticastDelegate);
                    }
                    else if (DuktapeDLL.duk_is_thread(ctx, idx))
                    {
                        return false;
                    }

                    int refid;
                    if (duk_get_native_refid(ctx, idx, out refid))
                    {
                        var cache = DuktapeVM.GetObjectCache(ctx);
                        return cache.MatchObjectType(refid, type);
                    }

                    int typeid;
                    if (DuktapeDLL.duk_unity_get_type_refid(ctx, idx, out typeid))
                    {
                        var vm = DuktapeVM.GetVM(ctx);
                        var eType = vm.GetExportedType(typeid);
                        if (eType != null)
                        {
                            // Debug.LogFormat("match type? {0} {1} {2}", eType, type, typeid); 
                            return eType == type;
                        }
                        // Debug.LogFormat("match type {0} with typeid {1}", type, typeid);
                    }
                    return type.IsSubclassOf(typeof(DuktapeValue));
                case duk_type_t.DUK_TYPE_NUMBER:
                    return type.IsPrimitive || type.IsEnum;
                case duk_type_t.DUK_TYPE_STRING:
                    return type == typeof(string);
                case duk_type_t.DUK_TYPE_UNDEFINED:
                case duk_type_t.DUK_TYPE_NULL:
                    return !type.IsValueType && !type.IsPrimitive;
                case duk_type_t.DUK_TYPE_BOOLEAN:
                    return type == typeof(bool);
                case duk_type_t.DUK_TYPE_BUFFER:
                    return type == typeof(byte[]) || type == typeof(sbyte[]) /* || type == typeof(DuktapeBuffer) */;
                case duk_type_t.DUK_TYPE_POINTER:
                // return type == typeof(DuktapePointer);
                case duk_type_t.DUK_TYPE_LIGHTFUNC:
                default:
                    return false;
            }
        }

        // 检查变参参数
        // offset: 从偏移处开始为变参
        protected static bool duk_match_param_types(IntPtr ctx, int offset, int nargs, Type type)
        {
            for (var i = offset; i < nargs; i++)
            {
                if (!duk_match_type(ctx, i, type))
                {
                    return false;
                }
            }
            return true;
        }

        protected static bool duk_match_types(IntPtr ctx, int nargs, Type t0)
        {
            return duk_match_type(ctx, 0, t0);
        }

        protected static bool duk_match_types(IntPtr ctx, int nargs, Type t0, Type t1)
        {
            return duk_match_type(ctx, 0, t0) && duk_match_type(ctx, 1, t1);
        }

        protected static bool duk_match_types(IntPtr ctx, int nargs, Type t0, Type t1, Type t2)
        {
            return duk_match_type(ctx, 0, t0) && duk_match_type(ctx, 1, t1) && duk_match_type(ctx, 2, t2);
        }

        protected static bool duk_match_types(IntPtr ctx, int nargs, Type t0, Type t1, Type t2, Type t3)
        {
            return duk_match_type(ctx, 0, t0) && duk_match_type(ctx, 1, t1) && duk_match_type(ctx, 2, t2) && duk_match_type(ctx, 3, t3);
        }

        protected static bool duk_match_types(IntPtr ctx, int nargs, Type t0, Type t1, Type t2, Type t3, Type t4)
        {
            return duk_match_type(ctx, 0, t0) && duk_match_type(ctx, 1, t1) && duk_match_type(ctx, 2, t2) && duk_match_type(ctx, 3, t3) && duk_match_type(ctx, 4, t4);
        }

        protected static bool duk_match_types(IntPtr ctx, int nargs, Type t0, Type t1, Type t2, Type t3, Type t4, Type t5)
        {
            return duk_match_type(ctx, 0, t0) && duk_match_type(ctx, 1, t1) && duk_match_type(ctx, 2, t2) && duk_match_type(ctx, 3, t3) && duk_match_type(ctx, 4, t4) && duk_match_type(ctx, 5, t5);
        }

        protected static bool duk_match_types(IntPtr ctx, int nargs, Type t0, Type t1, Type t2, Type t3, Type t4, Type t5, Type t6)
        {
            return duk_match_type(ctx, 0, t0) && duk_match_type(ctx, 1, t1) && duk_match_type(ctx, 2, t2) && duk_match_type(ctx, 3, t3) && duk_match_type(ctx, 4, t4) && duk_match_type(ctx, 5, t5) && duk_match_type(ctx, 6, t6);
        }

        protected static bool duk_match_types(IntPtr ctx, int nargs, params Type[] types)
        {
            for (int i = 0, size = types.Length; i < size; i++)
            {
                if (!duk_match_type(ctx, i, types[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
