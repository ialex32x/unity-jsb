using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

namespace QuickJS.Binding
{
    using UnityEngine;

    // 处理常规值, class, struct
    public partial class Values
    {
        public static bool duk_get_primitive(IntPtr ctx, int idx, out IntPtr o)
        {
            object o_t;
            var ret = duk_get_object(ctx, idx, out o_t);
            o = (IntPtr)o_t;
            return ret;
        }

        public static bool duk_get_primitive_array(IntPtr ctx, int idx, out IntPtr[] o)
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                var nidx = DuktapeDLL.duk_normalize_index(ctx, idx);
                o = new IntPtr[length];
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    IntPtr e;
                    duk_get_primitive(ctx, -1, out e);
                    o[i] = e;
                    DuktapeDLL.duk_pop(ctx);
                }
                return true;
            }
            duk_get_classvalue<IntPtr[]>(ctx, idx, out o);
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out bool o)
        {
            o = DuktapeDLL.duk_get_boolean(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive_array(IntPtr ctx, int idx, out bool[] o)
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                var nidx = DuktapeDLL.duk_normalize_index(ctx, idx);
                o = new bool[length];
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    bool e;
                    e = DuktapeDLL.duk_get_boolean(ctx, -1); //duk_get_primitive(ctx, -1, out e);
                    o[i] = e;
                    DuktapeDLL.duk_pop(ctx);
                }
                return true;
            }
            duk_get_classvalue<bool[]>(ctx, idx, out o);
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out sbyte o)
        {
            o = (sbyte)DuktapeDLL.duk_get_int(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive_array(IntPtr ctx, int idx, out sbyte[] o)
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                var nidx = DuktapeDLL.duk_normalize_index(ctx, idx);
                o = new sbyte[length];
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    sbyte e;
                    e = (sbyte)DuktapeDLL.duk_get_int(ctx, -1); // duk_get_primitive(ctx, -1, out e);
                    o[i] = e;
                    DuktapeDLL.duk_pop(ctx);
                }
                return true;
            }
            duk_get_classvalue<sbyte[]>(ctx, idx, out o);
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out byte o)
        {
            o = (byte)DuktapeDLL.duk_get_int(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive_array(IntPtr ctx, int idx, out byte[] o)
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                var nidx = DuktapeDLL.duk_normalize_index(ctx, idx);
                o = new byte[length];
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    byte e;
                    e = (byte)DuktapeDLL.duk_get_int(ctx, -1); // duk_get_primitive(ctx, -1, out e);
                    o[i] = e;
                    DuktapeDLL.duk_pop(ctx);
                }
                return true;
            }
            if (DuktapeDLL.duk_is_buffer_data(ctx, idx))
            {
                uint length;
                var pointer = DuktapeDLL.duk_unity_get_buffer_data(ctx, idx, out length);
                o = new byte[length];
                Marshal.Copy(pointer, o, 0, (int)length);
                return true;
            }
            duk_get_classvalue<byte[]>(ctx, idx, out o);
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out char o)
        {
            o = (char)DuktapeDLL.duk_get_int(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive_array(IntPtr ctx, int idx, out char[] o)
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                var nidx = DuktapeDLL.duk_normalize_index(ctx, idx);
                o = new char[length];
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    char e;
                    e = (char)DuktapeDLL.duk_get_int(ctx, -1); // duk_get_primitive(ctx, -1, out e);
                    o[i] = e;
                    DuktapeDLL.duk_pop(ctx);
                }
                return true;
            }
            duk_get_classvalue<char[]>(ctx, idx, out o);
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out string o)
        {
            o = DuktapeDLL.duk_get_string(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive_array(IntPtr ctx, int idx, out string[] o)
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                var nidx = DuktapeDLL.duk_normalize_index(ctx, idx);
                o = new string[length];
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    string e;
                    e = DuktapeDLL.duk_get_string(ctx, -1); // duk_get_primitive(ctx, -1, out e);
                    o[i] = e;
                    DuktapeDLL.duk_pop(ctx);
                }
                return true;
            }
            duk_get_classvalue<string[]>(ctx, idx, out o);
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out short o)
        {
            o = (short)DuktapeDLL.duk_get_int(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive_array(IntPtr ctx, int idx, out short[] o)
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                var nidx = DuktapeDLL.duk_normalize_index(ctx, idx);
                o = new short[length];
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    short e;
                    e = (short)DuktapeDLL.duk_get_int(ctx, -1); // duk_get_primitive(ctx, -1, out e);
                    o[i] = e;
                    DuktapeDLL.duk_pop(ctx);
                }
                return true;
            }
            duk_get_classvalue<short[]>(ctx, idx, out o);
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out ushort o)
        {
            o = (ushort)DuktapeDLL.duk_get_int(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive_array(IntPtr ctx, int idx, out ushort[] o)
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                var nidx = DuktapeDLL.duk_normalize_index(ctx, idx);
                o = new ushort[length];
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    ushort e;
                    e = (ushort)DuktapeDLL.duk_get_int(ctx, -1); // duk_get_primitive(ctx, -1, out e);
                    o[i] = e;
                    DuktapeDLL.duk_pop(ctx);
                }
                return true;
            }
            duk_get_classvalue<ushort[]>(ctx, idx, out o);
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out int o)
        {
            o = DuktapeDLL.duk_get_int(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive_array(IntPtr ctx, int idx, out int[] o)
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                var nidx = DuktapeDLL.duk_normalize_index(ctx, idx);
                o = new int[length];
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    int e;
                    e = DuktapeDLL.duk_get_int(ctx, -1); // duk_get_primitive(ctx, -1, out e);
                    o[i] = e;
                    DuktapeDLL.duk_pop(ctx);
                }
                return true;
            }
            duk_get_classvalue<int[]>(ctx, idx, out o);
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out uint o)
        {
            o = DuktapeDLL.duk_get_uint(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive_array(IntPtr ctx, int idx, out uint[] o)
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                var nidx = DuktapeDLL.duk_normalize_index(ctx, idx);
                o = new uint[length];
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    uint e;
                    e = DuktapeDLL.duk_get_uint(ctx, -1); // duk_get_primitive(ctx, -1, out e); 
                    o[i] = e;
                    DuktapeDLL.duk_pop(ctx);
                }
                return true;
            }
            duk_get_classvalue<uint[]>(ctx, idx, out o);
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out long o)
        {
            o = (long)DuktapeDLL.duk_get_number(ctx, idx); // no check, dangerous
            return true;
        }

        public static bool duk_get_primitive_array(IntPtr ctx, int idx, out long[] o)
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                var nidx = DuktapeDLL.duk_normalize_index(ctx, idx);
                o = new long[length];
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    long e;
                    e = (long)DuktapeDLL.duk_get_number(ctx, -1); // duk_get_primitive(ctx, -1, out e);
                    o[i] = e;
                    DuktapeDLL.duk_pop(ctx);
                }
                return true;
            }
            duk_get_classvalue<long[]>(ctx, idx, out o);
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out ulong o)
        {
            o = (ulong)DuktapeDLL.duk_get_number(ctx, idx); // no check, dangerous
            return true;
        }

        public static bool duk_get_primitive_array(IntPtr ctx, int idx, out ulong[] o)
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                var nidx = DuktapeDLL.duk_normalize_index(ctx, idx);
                o = new ulong[length];
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    ulong e;
                    e = (ulong)DuktapeDLL.duk_get_number(ctx, -1); // duk_get_primitive(ctx, -1, out e);
                    o[i] = e;
                    DuktapeDLL.duk_pop(ctx);
                }
                return true;
            }
            duk_get_classvalue<ulong[]>(ctx, idx, out o);
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out float o)
        {
            o = (float)DuktapeDLL.duk_get_number(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive_array(IntPtr ctx, int idx, out float[] o)
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                var nidx = DuktapeDLL.duk_normalize_index(ctx, idx);
                o = new float[length];
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    float e;
                    e = (float)DuktapeDLL.duk_get_number(ctx, -1); // duk_get_primitive(ctx, -1, out e);
                    o[i] = e;
                    DuktapeDLL.duk_pop(ctx);
                }
                return true;
            }
            duk_get_classvalue<float[]>(ctx, idx, out o);
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out double o)
        {
            o = DuktapeDLL.duk_get_number(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive_array(IntPtr ctx, int idx, out double[] o)
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                var nidx = DuktapeDLL.duk_normalize_index(ctx, idx);
                o = new double[length];
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    double e;
                    e = DuktapeDLL.duk_get_number(ctx, -1); // duk_get_primitive(ctx, -1, out e);
                    o[i] = e;
                    DuktapeDLL.duk_pop(ctx);
                }
                return true;
            }
            duk_get_classvalue<double[]>(ctx, idx, out o);
            return true;
        }

        public static bool duk_get_structvalue(IntPtr ctx, int idx, out LayerMask o)
        {
            o = (LayerMask)DuktapeDLL.duk_get_int(ctx, idx);
            return true;
        }

        public static bool duk_get_structvalue(IntPtr ctx, int idx, out Color o)
        {
            float r, g, b, a;
            var ret = DuktapeDLL.duk_unity_get4f(ctx, idx, out r, out g, out b, out a);
            o = new Color(r, g, b, a);
            return ret;
        }

        public static bool duk_get_structvalue(IntPtr ctx, int idx, out Color32 o)
        {
            int r, g, b, a;
            var ret = DuktapeDLL.duk_unity_get4i(ctx, idx, out r, out g, out b, out a);
            o = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
            return ret;
        }

        public static bool duk_get_structvalue(IntPtr ctx, int idx, out Vector2 o)
        {
            float x, y;
            var ret = DuktapeDLL.duk_unity_get2f(ctx, idx, out x, out y);
            o = new Vector2(x, y);
            return ret;
        }

        public static bool duk_get_structvalue(IntPtr ctx, int idx, out Vector2Int o)
        {
            int x, y;
            var ret = DuktapeDLL.duk_unity_get2i(ctx, idx, out x, out y);
            o = new Vector2Int(x, y);
            return ret;
        }

        public static bool duk_get_structvalue(IntPtr ctx, int idx, out Vector3 o)
        {
            float x, y, z;
            var ret = DuktapeDLL.duk_unity_get3f(ctx, idx, out x, out y, out z);
            o = new Vector3(x, y, z);
            return ret;
        }

        public static bool duk_get_structvalue(IntPtr ctx, int idx, out Vector3Int o)
        {
            int x, y, z;
            var ret = DuktapeDLL.duk_unity_get3i(ctx, idx, out x, out y, out z);
            o = new Vector3Int(x, y, z);
            return ret;
        }

        public static bool duk_get_structvalue(IntPtr ctx, int idx, out Vector4 o)
        {
            float x, y, z, w;
            var ret = DuktapeDLL.duk_unity_get4f(ctx, idx, out x, out y, out z, out w);
            o = new Vector4(x, y, z, w);
            return ret;
        }

        public static bool duk_get_structvalue(IntPtr ctx, int idx, out Quaternion o)
        {
            float x, y, z, w;
            var ret = DuktapeDLL.duk_unity_get4f(ctx, idx, out x, out y, out z, out w);
            o = new Quaternion(x, y, z, w);
            return ret;
        }

        // public static bool duk_get_structvalue(IntPtr ctx, int idx, out Matrix4x4 o)
        // {
        //     var ret = DuktapeDLL.duk_unity_get16f(ctx, idx, ...);
        //     o = new Matrix4x4(...);
        //     return ret;
        // }

        // fallthrough
        public static bool duk_get_structvalue<T>(IntPtr ctx, int idx, out T o)
        where T : struct
        {
            object o_t;
            var ret = duk_get_object(ctx, idx, out o_t);
            o = (T)o_t;
            return ret;
        }

        public static bool duk_get_structvalue<T>(IntPtr ctx, int idx, out T? o)
        where T : struct
        {
            object o_t;
            var ret = duk_get_object(ctx, idx, out o_t);
            o = (T)o_t;
            return ret;
        }

        public static bool duk_get_structvalue_array<T>(IntPtr ctx, int idx, out T[] o)
        where T : struct
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                o = new T[length];
                idx = DuktapeDLL.duk_normalize_index(ctx, idx);
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    T e;
                    if (duk_get_structvalue(ctx, -1, out e))
                    {
                        o[i] = e;
                    }
                }
                return true;
            }
            o = null;
            return false;
        }

        public static bool duk_get_structvalue_array<T>(IntPtr ctx, int idx, out T?[] o)
        where T : struct
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                o = new T?[length];
                idx = DuktapeDLL.duk_normalize_index(ctx, idx);
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    T? e;
                    if (duk_get_structvalue(ctx, -1, out e))
                    {
                        o[i] = e;
                    }
                }
                return true;
            }
            o = null;
            return false;
        }

        // not value type (except string/array)
        public static bool duk_get_classvalue<T>(IntPtr ctx, int idx, out T o)
        where T : class
        {
            object o_t;
            if (duk_get_cached_object(ctx, idx, out o_t))
            {
                o = o_t as T;
                if (o_t != null && o == null)
                {
                    throw new InvalidCastException(string.Format("{0} type mismatch {1}", o_t.GetType(), typeof(T)));
                    // return false;
                }
                return true;
            }
            var jsType = DuktapeDLL.duk_get_type(ctx, idx);
            throw new InvalidCastException(string.Format("{0} type mismatch {1}", jsType, typeof(T)));
        }

        public static bool duk_get_classvalue(IntPtr ctx, int idx, out DuktapeObject o)
        {
            object obj;
            if (duk_get_cached_object(ctx, idx, out obj))
            {
                if (obj is DuktapeObject)
                {
                    o = (DuktapeObject)obj;
                    return true;
                }
            }
            if (DuktapeDLL.duk_is_object(ctx, idx))
            {
                DuktapeDLL.duk_dup(ctx, idx);
                var ptr = DuktapeDLL.duk_get_heapptr(ctx, -1);
                var refid = DuktapeDLL.duk_unity_ref(ctx);
                o = new DuktapeObject(ctx, refid, ptr);
                return true;
            }
            o = null;
            return false;
        }

        public static bool duk_get_classvalue(IntPtr ctx, int idx, out QuickJS.IO.ByteBuffer o)
        {
            object obj;
            if (duk_get_cached_object(ctx, idx, out obj))
            {
                if (obj is QuickJS.IO.ByteBuffer)
                {
                    o = (QuickJS.IO.ByteBuffer)obj;
                    return true;
                }
            }
            if (DuktapeDLL.duk_is_buffer_data(ctx, idx))
            {
                var allocator = DuktapeVM.GetVM(ctx).GetByteBufferAllocator();
                if (allocator != null)
                {
                    uint length;
                    var pointer = DuktapeDLL.duk_unity_get_buffer_data(ctx, idx, out length);
                    o = allocator.Alloc((int)length);
                    allocator.AutoRelease(o);
                    o.WriteBytes(pointer, (int)length);
                    return true;
                }
            }
            o = null;
            return false;
        }

        public static bool duk_get_classvalue(IntPtr ctx, int idx, out DuktapeArray o)
        {
            object obj;
            if (duk_get_cached_object(ctx, idx, out obj))
            {
                if (obj is DuktapeArray)
                {
                    o = (DuktapeArray)obj;
                    return true;
                }
            }
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                DuktapeDLL.duk_dup(ctx, idx);
                var ptr = DuktapeDLL.duk_get_heapptr(ctx, -1);
                var refid = DuktapeDLL.duk_unity_ref(ctx);
                o = new DuktapeArray(ctx, refid, ptr);
                return true;
            }
            o = null;
            return false;
        }

        public static bool duk_get_classvalue(IntPtr ctx, int idx, out DuktapeFunction o)
        {
            if (DuktapeDLL.duk_is_function(ctx, idx))
            {
                object obj;
                if (duk_get_cached_object(ctx, idx, out obj))
                {
                    if (obj is DuktapeFunction)
                    {
                        o = (DuktapeFunction)obj;
                        return true;
                    }
                }

                DuktapeDLL.duk_dup(ctx, idx);
                var ptr = DuktapeDLL.duk_get_heapptr(ctx, -1);
                var refid = DuktapeDLL.duk_unity_ref(ctx);
                o = new DuktapeFunction(ctx, refid, ptr);
                return true;
            }

            o = null;
            return false;
        }
        public static bool duk_get_cached_object(IntPtr ctx, int idx, out object o)
        {
            int id;
            if (DuktapeDLL.duk_unity_get_refid(ctx, idx, out id))
            {
                return DuktapeVM.GetObjectCache(ctx).TryGetObject(id, out o);
            }
            //TODO: if o is Delegate, try get from delegate cache list
            o = null;
            return false;
        }

        // 只处理 JS_OBJECT
        public static bool duk_get_object(IntPtr ctx, int idx, out object o)
        {
            var jstype = DuktapeDLL.duk_get_type(ctx, idx);
            if (jstype == duk_type_t.DUK_TYPE_OBJECT)
            {
                return duk_get_cached_object(ctx, idx, out o);
            }
            // Debug.LogFormat("duk_get_object({0})", jstype);
            //     case duk_type_t.DUK_TYPE_STRING:
            //         o = DuktapeDLL.duk_get_string(ctx, idx);
            //         return true;
            //     default: break;
            // }
            // 其他类型不存在对象映射
            o = null;
            return false;
        }

        public static bool duk_get_var(IntPtr ctx, int idx, out object o)
        {
            var jstype = DuktapeDLL.duk_get_type(ctx, idx);

            switch (jstype)
            {
                case duk_type_t.DUK_TYPE_BOOLEAN: /* ECMAScript boolean: 0 or 1 */
                    {
                        o = DuktapeDLL.duk_get_boolean(ctx, idx);
                        return true;
                    }
                case duk_type_t.DUK_TYPE_NUMBER: /* ECMAScript number: double */
                    {
                        o = DuktapeDLL.duk_get_number(ctx, idx);
                        return true;
                    }
                case duk_type_t.DUK_TYPE_STRING: /* ECMAScript string: CESU-8 / extended UTF-8 encoded */
                    {
                        o = DuktapeDLL.duk_get_string(ctx, idx);
                        return true;
                    }
                case duk_type_t.DUK_TYPE_OBJECT: /* ECMAScript object: includes objects, arrays, functions, threads */
                    {
                        return duk_get_cached_object(ctx, idx, out o);
                    }
                case duk_type_t.DUK_TYPE_BUFFER: /* fixed or dynamic, garbage collected byte buffer */
                    {
                        uint length;
                        var pointer = DuktapeDLL.duk_unity_get_buffer_data(ctx, idx, out length);
                        var buffer = new byte[length];
                        o = buffer;
                        Marshal.Copy(pointer, buffer, 0, (int)length);
                        return true;
                    }
                case duk_type_t.DUK_TYPE_POINTER:    /* raw void pointer */
                case duk_type_t.DUK_TYPE_LIGHTFUNC:    /* lightweight function pointer */
                    throw new NotImplementedException();
                case duk_type_t.DUK_TYPE_NONE:    /* no value, e.g. invalid index */
                    o = null;
                    return false;
                case duk_type_t.DUK_TYPE_UNDEFINED:    /* ECMAScript undefined */
                case duk_type_t.DUK_TYPE_NULL:    /* ECMAScript null */
                    o = null;
                    return true;
            }

            o = null;
            return false;
        }

        // public static bool duk_get_object(IntPtr ctx, int idx, out object o)
        // {
        //     if (DuktapeDLL.duk_is_null_or_undefined(ctx, idx)) // or check for object?
        //     {
        //         o = null;
        //         return true;
        //     }
        //     var jstype = DuktapeDLL.duk_get_type(ctx, idx);
        //     Debug.LogFormat("duk_get_object({0})", jstype);
        //     switch (jstype)
        //     {
        //         case duk_type_t.DUK_TYPE_STRING:
        //             o = DuktapeDLL.duk_get_string(ctx, idx);
        //             return true;
        //         default: break;
        //     }
        //     return duk_get_cached_object(ctx, idx, out o);
        // }

        public static bool duk_get_classvalue_array<T>(IntPtr ctx, int idx, out T[] o)
        where T : class
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                o = new T[length];
                idx = DuktapeDLL.duk_normalize_index(ctx, idx);
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    T e;
                    if (duk_get_classvalue(ctx, -1, out e))
                    {
                        o[i] = e;
                    }
                }
                return true;
            }
            o = null;
            return false;
        }

        public static bool duk_get_enumvalue<T>(IntPtr ctx, int idx, out T o)
        where T : Enum
        {
            int v;
            var ret = duk_get_primitive(ctx, idx, out v);
            o = (T)Enum.ToObject(typeof(T), v);
            return ret;
        }

        public static bool duk_get_enumvalue_array<T>(IntPtr ctx, int idx, out T[] o)
        where T : Enum
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                var nidx = DuktapeDLL.duk_normalize_index(ctx, idx);
                o = new T[length];
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    T e;
                    duk_get_enumvalue(ctx, -1, out e);
                    o[i] = e;
                    DuktapeDLL.duk_pop(ctx);
                }
                return true;
            }
            duk_get_classvalue<T[]>(ctx, idx, out o);
            return true;
        }
    }
}
