using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

// Default Marshaling for Strings
// https://docs.microsoft.com/en-us/dotnet/framework/interop/default-marshaling-for-strings

// Marshaling a Delegate as a Callback Method
// https://docs.microsoft.com/en-us/dotnet/framework/interop/marshaling-a-delegate-as-a-callback-method

namespace QuickJS.Native
{
    using JSValueConst = JSValue;
    using JS_BOOL = Int32;
    using int32_t = Int32;
    using uint32_t = UInt32;
    using int64_t = Int64;
    using uint64_t = UInt64;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public unsafe delegate IntPtr JSModuleNormalizeFunc(JSContext ctx,
        [MarshalAs(UnmanagedType.LPStr)] string module_base_name, [MarshalAs(UnmanagedType.LPStr)] string module_name,
        IntPtr opaque);

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate JSModuleDef JSModuleLoaderFunc(JSContext ctx, [MarshalAs(UnmanagedType.LPStr)] string module_name,
        IntPtr opaque);

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate void JSClassFinalizer(JSRuntime rt, JSValue val);

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate JSValue JSCFunction(JSContext ctx, JSValueConst this_val, int argc,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
        JSValueConst[] argv);

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public delegate JSValue JSCFunctionMagic(JSContext ctx, JSValueConst this_val, int argc,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
        JSValueConst[] argv, int magic);

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
    public unsafe delegate JSValue JSCFunctionData(JSContext ctx, JSValueConst this_val, int argc,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
        JSValueConst[] argv, int magic, JSValue* func_data);


    public partial class JSApi
    {
#if UNITY_IPHONE && !UNITY_EDITOR
	    const string JSBDLL = "__Internal";
#else
        const string JSBDLL = "libquickjs";
#endif

        public const int JS_TAG_FIRST = -11; /* first negative tag */
        public const int JS_TAG_BIG_DECIMAL = -11;
        public const int JS_TAG_BIG_INT = -10;
        public const int JS_TAG_BIG_FLOAT = -9;
        public const int JS_TAG_SYMBOL = -8;
        public const int JS_TAG_STRING = -7;
        public const int JS_TAG_MODULE = -3; /* used internally */
        public const int JS_TAG_FUNCTION_BYTECODE = -2; /* used internally */
        public const int JS_TAG_OBJECT = -1;
        public const int JS_TAG_INT = 0;
        public const int JS_TAG_BOOL = 1;
        public const int JS_TAG_NULL = 2;
        public const int JS_TAG_UNDEFINED = 3;
        public const int JS_TAG_UNINITIALIZED = 4;
        public const int JS_TAG_CATCH_OFFSET = 5;
        public const int JS_TAG_EXCEPTION = 6;
        public const int JS_TAG_FLOAT64 = 7;

        public static JSValue[] EmptyValues = new JSValue[0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool JS_VALUE_HAS_REF_COUNT(JSValue v)
        {
            return (ulong)v.tag >= unchecked((ulong)JS_TAG_FIRST);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue JS_MKVAL(long tag, int val)
        {
            return new JSValue() { u = new JSValueUnion() { int32 = val }, tag = tag };
        }

        public static readonly JSValue JS_NULL = JS_MKVAL(JS_TAG_NULL, 0);
        public static readonly JSValue JS_UNDEFINED = JS_MKVAL(JS_TAG_UNDEFINED, 0);
        public static readonly JSValue JS_FALSE = JS_MKVAL(JS_TAG_BOOL, 0);
        public static readonly JSValue JS_TRUE = JS_MKVAL(JS_TAG_BOOL, 1);
        public static readonly JSValue JS_EXCEPTION = JS_MKVAL(JS_TAG_EXCEPTION, 0);
        public static readonly JSValue JS_UNINITIALIZED = JS_MKVAL(JS_TAG_UNINITIALIZED, 0);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSRuntime JS_NewRuntime();

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSRuntime JS_GetRuntime(JSContext ctx);

        static JSApi()
        {
            __JSB_Init();
        }

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JS_FreeRuntime(JSRuntime rt);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSContext JS_NewContext(JSRuntime rt);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JS_FreeContext(JSContext ctx);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JS_AddIntrinsicOperators(JSContext ctx);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_GetGlobalObject(JSContext ctx);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_IsInstanceOf(JSContext ctx, JSValueConst val, JSValueConst obj);

        #region property
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_GetPropertyUint32(JSContext ctx, JSValueConst this_obj, uint32_t idx);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_GetPropertyInternal(JSContext ctx, JSValueConst obj, JSAtom prop,
            JSValueConst receiver, JS_BOOL throw_ref_error);

        // 增引用, 需要 FreeValue
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue JS_GetProperty(JSContext ctx, JSValueConst this_obj, JSAtom prop)
        {
            return JS_GetPropertyInternal(ctx, this_obj, prop, this_obj, 0);
        }

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JSB_NewPropertyObjectStr(JSContext ctx, JSValueConst this_obj,
            [MarshalAs(UnmanagedType.LPStr)] string name, JSPropFlags flags);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JSB_NewPropertyObject(JSContext ctx, JSValueConst this_obj,
            JSAtom name, JSPropFlags flags);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_GetPropertyStr(JSContext ctx, JSValueConst this_obj,
            [MarshalAs(UnmanagedType.LPStr)] string prop);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_DefineProperty(JSContext ctx, JSValueConst this_obj,
            JSAtom prop, JSValueConst val,
            JSValueConst getter, JSValueConst setter, JSPropFlags flags);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_DefinePropertyValueStr(JSContext ctx, JSValueConst this_obj,
            [MarshalAs(UnmanagedType.LPStr)] string prop,
            JSValue val, JSPropFlags flags);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_DefinePropertyValue(JSContext ctx, JSValueConst this_obj,
            JSAtom prop, JSValue val, JSPropFlags flags);

        #endregion

        #region error handling

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_Throw(JSContext ctx, JSValue obj);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_GetException(JSContext ctx);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL JS_IsError(JSContext ctx, JSValueConst val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JS_ResetUncatchableError(JSContext ctx);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_NewError(JSContext ctx);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe JSValue JSB_ThrowTypeError(JSContext ctx, byte* msg);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe JSValue JS_ThrowTypeError(JSContext ctx, string message)
        {
            var bytes = Utils.TextUtils.GetNullTerminatedBytes(message);
            fixed (byte* msg = bytes)
            {
                return JSB_ThrowTypeError(ctx, msg);
            }
        }

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe JSValue JSB_ThrowInternalError(JSContext ctx, byte* msg);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe JSValue JS_ThrowInternalError(JSContext ctx, string message)
        {
            var bytes = Utils.TextUtils.GetNullTerminatedBytes(message);
            fixed (byte* msg = bytes)
            {
                return JSB_ThrowInternalError(ctx, msg);
            }
        }

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe JSValue JSB_ThrowRangeError(JSContext ctx, byte* msg);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe JSValue JS_ThrowRangeError(JSContext ctx, string message)
        {
            var bytes = Utils.TextUtils.GetNullTerminatedBytes(message);
            fixed (byte* msg = bytes)
            {
                return JSB_ThrowRangeError(ctx, msg);
            }
        }

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe JSValue JSB_ThrowReferenceError(JSContext ctx, byte* msg);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe JSValue JS_ThrowReferenceError(JSContext ctx, string message)
        {
            var bytes = Utils.TextUtils.GetNullTerminatedBytes(message);
            fixed (byte* msg = bytes)
            {
                return JSB_ThrowReferenceError(ctx, msg);
            }
        }

        // JSValue __js_printf_like(2, 3) JS_ThrowRangeError(JSContext *ctx, const char *fmt, ...);
        // JSValue __js_printf_like(2, 3) JS_ThrowInternalError(JSContext *ctx, const char *fmt, ...);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_ThrowOutOfMemory(JSContext ctx);

        #endregion

        #region new values

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe JSValue JS_NewString(JSContext ctx, byte* str);

        public static unsafe JSValue JS_NewString(JSContext ctx, string str)
        {
            var bytes = Utils.TextUtils.GetNullTerminatedBytes(str);

            fixed (byte* ptr = bytes)
            {
                return JS_NewString(ctx, ptr);
            }
        }

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JSB_NewInt64(JSContext ctx, int64_t val);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue JS_NewBool(JSContext ctx, bool val)
        {
            return val ? JS_TRUE : JS_FALSE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue JS_NewInt32(JSContext ctx, int val)
        {
            return JS_MKVAL(JS_TAG_INT, val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue __JS_NewFloat64(JSContext ctx, double d)
        {
            JSValue v = new JSValue();
            v.tag = JS_TAG_FLOAT64;
            v.u.float64 = d;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue JS_NewUint32(JSContext ctx, uint32_t val)
        {
            JSValue v;
            if (val <= 0x7fffffff)
            {
                v = JS_NewInt32(ctx, (int)val);
            }
            else
            {
                v = __JS_NewFloat64(ctx, val);
            }

            return v;
        }

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "JSB_NewFloat64")]
        public static extern JSValue JS_NewFloat64(JSContext ctx, double d);

        #endregion

        #region atom support

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe JSAtom JS_NewAtomLen(JSContext ctx, byte* str, size_t len);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern JSAtom JS_NewAtom(JSContext ctx, [In, MarshalAs(UnmanagedType.LPStr)] string str);

        // JSAtom JS_NewAtomUInt32(JSContext *ctx, uint32_t n);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSAtom JS_DupAtom(JSContext ctx, JSAtom v);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JS_FreeAtom(JSContext ctx, JSAtom v);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JS_FreeAtomRT(JSRuntime rt, JSAtom v);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_AtomToValue(JSContext ctx, JSAtom atom);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_AtomToString(JSContext ctx, JSAtom atom);

        // const char *JS_AtomToCString(JSContext *ctx, JSAtom atom);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSAtom JS_ValueToAtom(JSContext ctx, JSValueConst val);

        #endregion

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_NewObjectProtoClass(JSContext ctx, JSValueConst proto, JSClassID class_id);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_NewObjectClass(JSContext ctx, int class_id);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_NewObjectProto(JSContext ctx, JSValueConst proto);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_NewObject(JSContext ctx);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL JS_IsFunction(JSContext ctx, JSValueConst val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL JS_IsConstructor(JSContext ctx, JSValueConst val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL JS_SetConstructorBit(JSContext ctx, JSValueConst func_obj, JS_BOOL val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_NewArray(JSContext ctx);

        /// <summary>
        /// return -1 if exception (proxy case) or TRUE/FALSE
        /// </summary>
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_IsArray(JSContext ctx, JSValueConst val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr JS_GetContextOpaque(JSContext ctx);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JS_SetContextOpaque(JSContext ctx, IntPtr opaque);

        // 通过 Atom 命名创建函数
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JSB_NewCFunction(JSContext ctx, IntPtr func, JSAtom atom, int length,
            JSCFunctionEnum cproto, int magic);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue JSB_NewCFunction(JSContext ctx, JSCFunction func, JSAtom atom, int length,
            JSCFunctionEnum cproto, int magic)
        {
            var fn = Marshal.GetFunctionPointerForDelegate(func);
            return JSB_NewCFunction(ctx, fn, atom, length, cproto, magic);
        }

        // 通过 Atom 命名创建函数
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JSB_NewCFunctionMagic(JSContext ctx, IntPtr func, JSAtom atom, int length,
            JSCFunctionEnum cproto, int magic);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue JSB_NewCFunctionMagic(JSContext ctx, JSCFunctionMagic func, JSAtom atom, int length,
            JSCFunctionEnum cproto, int magic)
        {
            var fn = Marshal.GetFunctionPointerForDelegate(func);
            return JSB_NewCFunctionMagic(ctx, fn, atom, length, cproto, magic);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JSValue JS_NewCFunctionMagic(JSContext ctx, JSCFunctionMagic func,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            int length, JSCFunctionEnum cproto, int magic)
        {
            var fn = Marshal.GetFunctionPointerForDelegate(func);
            return JS_NewCFunction2(ctx, fn, name, length, cproto, magic);
        }

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_NewCFunction2(JSContext ctx, IntPtr func,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            int length, JSCFunctionEnum cproto, int magic);

        public static JSValue JS_NewCFunction2(JSContext ctx, JSCFunction func, string name, int length,
            JSCFunctionEnum cproto, int magic)
        {
            var fn = Marshal.GetFunctionPointerForDelegate(func);
            return JS_NewCFunction2(ctx, fn, name, length, cproto, magic);
        }

        public static JSValue JS_NewCFunction(JSContext ctx, JSCFunction func, string name, int length)
        {
            var fn = Marshal.GetFunctionPointerForDelegate(func);
            return JS_NewCFunction2(ctx, fn, name, length, JSCFunctionEnum.JS_CFUNC_generic, 0);
        }

        // [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        // public static extern JSClassID JS_NewClassID(ref JSClassID pclass_id);

        // [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        // public static extern int JS_NewClass(JSRuntime rt, JSClassID class_id, ref JSClassDef class_def);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_IsRegisteredClass(JSRuntime rt, JSClassID class_id);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JS_SetClassProto(JSContext ctx, JSClassID class_id, JSValue obj);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_GetClassProto(JSContext ctx, JSClassID class_id);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JS_SetConstructor(JSContext ctx, JSValueConst func_obj, JSValueConst proto);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_SetPropertyInternal(JSContext ctx, JSValueConst this_obj, JSAtom prop, JSValue val,
            int flags);

        public static int JS_SetProperty(JSContext ctx, JSValueConst this_obj, JSAtom prop, JSValue val)
        {
            return JS_SetPropertyInternal(ctx, this_obj, prop, val, (int)JSPropFlags.JS_PROP_THROW);
        }

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_SetPropertyUint32(JSContext ctx, JSValueConst this_obj, uint32_t idx, JSValue val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_SetPropertyInt64(JSContext ctx, JSValueConst this_obj, int64_t idx, JSValue val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int JS_SetPropertyStr(JSContext ctx, [In] JSValueConst this_obj,
            [MarshalAs(UnmanagedType.LPStr)] string prop, JSValue val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int JS_SetPropertyStr(JSContext ctx, [In] JSValueConst this_obj, byte* prop,
            JSValue val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_HasProperty(JSContext ctx, JSValueConst this_obj, JSAtom prop);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_Call(JSContext ctx, JSValueConst func_obj, JSValueConst this_obj,
            int argc, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            JSValueConst[] argv);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe JSValue JS_Call(JSContext ctx, JSValueConst func_obj, JSValueConst this_obj,
            int argc, JSValueConst* argv);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_Invoke(JSContext ctx, JSValueConst this_val, JSAtom atom,
            int argc, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            JSValueConst[] argv);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_IsExtensible(JSContext ctx, JSValueConst obj);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_PreventExtensions(JSContext ctx, JSValueConst obj);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_DeleteProperty(JSContext ctx, JSValueConst obj, JSAtom prop, int flags);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_SetPrototype(JSContext ctx, JSValueConst obj, JSValueConst proto_val);

        // 返回值不需要 FreeValue
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValueConst JS_GetPrototype(JSContext ctx, JSValueConst val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JS_RunGC(JSRuntime rt);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_ExecutePendingJob(JSRuntime rt, out JSContext pctx);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL JS_IsLiveObject(JSRuntime rt, JSValueConst obj);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_ToBool(JSContext ctx, JSValueConst val);

        /// <summary>
        /// 返回 <0 表示失败
        /// </summary>
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_ToInt32(JSContext ctx, out int pres, JSValue val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_ToInt64(JSContext ctx, out int64_t pres, JSValueConst val);
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_ToIndex(JSContext ctx, out uint64_t plen, JSValueConst val);
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int JS_ToFloat64(JSContext ctx, out double pres, JSValueConst val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint32_t JSB_ToUint32z(JSContext ctx, JSValueConst val);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JS_BOOL JSB_ToUint32(JSContext ctx, out uint32_t pres, JSValueConst val);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe JSValue JS_Eval(JSContext ctx, string input, string filename)
        {
            //TEMP CODE
            var input_bytes = Utils.TextUtils.GetNullTerminatedBytes(input);
            var fn_bytes = Utils.TextUtils.GetNullTerminatedBytes(filename);

            fixed (byte* input_ptr = input_bytes)
            fixed (byte* fn_ptr = fn_bytes)
            {
                var input_len = (size_t)(input_bytes.Length - 1);
                // return JS_Eval(ctx, input_ptr, input_len, fn_ptr, JSEvalFlags.JS_EVAL_TYPE_GLOBAL);
                return JS_Eval(ctx, input_ptr, input_len, fn_ptr,
                    JSEvalFlags.JS_EVAL_TYPE_MODULE | JSEvalFlags.JS_EVAL_FLAG_STRICT);
            }
        }

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe JSValue JS_Eval(JSContext ctx, byte* input, size_t input_len, byte* filename,
            JSEvalFlags eval_flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool JS_IsNumber(JSValueConst v)
        {
            var tag = v.tag;
            return tag == JS_TAG_INT || tag == JS_TAG_FLOAT64;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool JS_IsBigInt(JSContext ctx, JSValueConst v)
        {
            var tag = v.tag;
            return tag == JS_TAG_BIG_INT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool JS_IsBigFloat(JSValueConst v)
        {
            var tag = v.tag;
            return tag == JS_TAG_BIG_FLOAT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool JS_IsBigDecimal(JSValueConst v)
        {
            var tag = v.tag;
            return tag == JS_TAG_BIG_DECIMAL;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool JS_IsBool(JSValueConst v)
        {
            return v.tag == JS_TAG_BOOL;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool JS_IsNull(JSValueConst v)
        {
            return v.tag == JS_TAG_NULL;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool JS_IsUndefined(JSValueConst v)
        {
            return v.tag == JS_TAG_UNDEFINED;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool JS_IsException(JSValueConst v)
        {
            return (v.tag == JS_TAG_EXCEPTION);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool JS_IsUninitialized(JSValueConst v)
        {
            return (v.tag == JS_TAG_UNINITIALIZED);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool JS_IsString(JSValueConst v)
        {
            return v.tag == JS_TAG_STRING;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool JS_IsSymbol(JSValueConst v)
        {
            return v.tag == JS_TAG_SYMBOL;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool JS_IsObject(JSValueConst v)
        {
            return v.tag == JS_TAG_OBJECT;
        }

        #region ref counting

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "JSB_DupValue")]
        public static extern JSValue JS_DupValue(JSContext ctx, JSValueConst v);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "JSB_DupValueRT")]
        public static extern JSValue JS_DupValueRT(JSRuntime rt, JSValueConst v);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "JSB_FreeValue")]
        public static extern void JS_FreeValue(JSContext ctx, JSValue v);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "JSB_FreeValueRT")]
        public static extern void JS_FreeValueRT(JSRuntime rt, JSValue v);

        #endregion

        #region unity base

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSAtom JSB_ATOM_message();

        public static readonly JSAtom JS_ATOM_message = JSB_ATOM_message();

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSAtom JSB_ATOM_fileName();

        public static readonly JSAtom JS_ATOM_fileName = JSB_ATOM_fileName();

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSAtom JSB_ATOM_lineNumber();

        public static readonly JSAtom JS_ATOM_lineNumber = JSB_ATOM_lineNumber();

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSAtom JSB_ATOM_length();

        public static readonly JSAtom JS_ATOM_length = JSB_ATOM_length();

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSAtom JSB_ATOM_stack();

        public static readonly JSAtom JS_ATOM_stack = JSB_ATOM_stack();

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSAtom JSB_ATOM_prototype();

        public static readonly JSAtom JS_ATOM_prototype = JSB_ATOM_prototype();

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "JSB_Init")]
        public static extern void __JSB_Init();

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "JSB_GetClassID")]
        public static extern JSClassID __JSB_GetClassID();

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "JSB_NewClass")]
        public static extern JSClassID __JSB_NewClass(JSRuntime rt, JSClassID class_id,
            [MarshalAs(UnmanagedType.LPStr)] string class_name, IntPtr finalizer);

        public static JSClassID JS_NewClass(JSRuntime rt, JSClassID class_id, string class_name,
            JSClassFinalizer finalizer)
        {
            var fn_ptr = Marshal.GetFunctionPointerForDelegate(finalizer);
            return __JSB_NewClass(rt, class_id, class_name, fn_ptr);
        }

        #endregion

        #region string

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr JS_ToCStringLen2(JSContext ctx, out size_t len, [In] JSValue val,
            [MarshalAs(UnmanagedType.Bool)] bool cesu8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr JS_ToCStringLen(JSContext ctx, out size_t len, JSValue val)
        {
            return JS_ToCStringLen2(ctx, out len, val, false);
        }

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JS_FreeCString(JSContext ctx, IntPtr ptr);

        #endregion

        #region module

        /* module_normalize = NULL is allowed and invokes the default module filename normalizer */
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void JS_SetModuleLoaderFunc(JSRuntime rt, IntPtr module_normalize,
            IntPtr module_loader, IntPtr opaque);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void JS_SetModuleLoaderFunc(JSRuntime rt,
            JSModuleNormalizeFunc module_normalize,
            JSModuleLoaderFunc module_loader, IntPtr opaque)
        {
            JS_SetModuleLoaderFunc(rt,
                module_normalize != null ? Marshal.GetFunctionPointerForDelegate(module_normalize) : IntPtr.Zero,
                module_loader != null ? Marshal.GetFunctionPointerForDelegate(module_loader) : IntPtr.Zero, opaque);
        }

        /* return the import.meta object of a module */
        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSValue JS_GetImportMeta(JSContext ctx, JSModuleDef m);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern JSAtom JS_GetModuleName(JSContext ctx, JSModuleDef m);

        #endregion

        #region critical

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe IntPtr js_strndup(JSContext ctx, byte* s, size_t n);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe IntPtr js_strndup(JSContext ctx, string str)
        {
            var bytes = Utils.TextUtils.GetNullTerminatedBytes(str);
            fixed (byte* ptr = bytes)
            {
                return JSApi.js_strndup(ctx, ptr, bytes.Length - 1);
            }
        }

        #endregion

        #region array buffer

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr JS_GetArrayBuffer(JSContext ctx, out size_t psize, JSValueConst obj);

        [DllImport(JSBDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe JSValue JS_NewArrayBufferCopy(JSContext ctx, byte* buf, size_t len);
        #endregion
    }
}
