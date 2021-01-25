// using System;
// using System.Collections.Generic;
// using System.Runtime.CompilerServices;

// namespace QuickJS.Binding
// {
//     using Native;

//     public partial class Values
//     {
//         public delegate JSValue CSValueCast(JSContext ctx, object o);

//         // 用于根据 Type 信息将 CS Object 专为对应的 JSValue
//         private static Dictionary<Type, CSValueCast> _CSCastMap = new Dictionary<Type, CSValueCast>();

//         private static void init_cs_cast_map()
//         {
//             _CSCastMap[typeof(void)] = cs_value_cast_void;
//             _CSCastMap[typeof(bool)] = cs_value_cast_bool;
//             _CSCastMap[typeof(byte)] = cs_value_cast_byte;
//             _CSCastMap[typeof(char)] = cs_value_cast_char;
//             _CSCastMap[typeof(sbyte)] = cs_value_cast_sbyte;
//             _CSCastMap[typeof(short)] = cs_value_cast_short;
//             _CSCastMap[typeof(ushort)] = cs_value_cast_ushort;
//             _CSCastMap[typeof(int)] = cs_value_cast_int;
//             _CSCastMap[typeof(uint)] = cs_value_cast_uint;
//             _CSCastMap[typeof(long)] = cs_value_cast_long;
//             _CSCastMap[typeof(ulong)] = cs_value_cast_ulong;
//             _CSCastMap[typeof(float)] = cs_value_cast_float;
//             _CSCastMap[typeof(double)] = cs_value_cast_double;
//             _CSCastMap[typeof(string)] = cs_value_cast_string;
//             _CSCastMap[typeof(Type)] = cs_value_cast_type;
//             _CSCastMap[typeof(ScriptValue)] = cs_value_cast_script_value;
//             _CSCastMap[typeof(ScriptPromise)] = cs_value_cast_script_promise;
//         }

//         private static JSValue cs_value_cast_void(JSContext ctx, object o)
//         {
//             return JSApi.JS_UNDEFINED;
//         }

//         private static JSValue cs_value_cast_bool(JSContext ctx, object o)
//         {
//             return js_push_primitive(ctx, (bool)o);
//         }

//         private static JSValue cs_value_cast_byte(JSContext ctx, object o)
//         {
//             return js_push_primitive(ctx, (byte)o);
//         }

//         private static JSValue cs_value_cast_char(JSContext ctx, object o)
//         {
//             return js_push_primitive(ctx, (char)o);
//         }

//         private static JSValue cs_value_cast_sbyte(JSContext ctx, object o)
//         {
//             return js_push_primitive(ctx, (sbyte)o);
//         }

//         private static JSValue cs_value_cast_short(JSContext ctx, object o)
//         {
//             return js_push_primitive(ctx, (short)o);
//         }

//         private static JSValue cs_value_cast_ushort(JSContext ctx, object o)
//         {
//             return js_push_primitive(ctx, (ushort)o);
//         }

//         private static JSValue cs_value_cast_int(JSContext ctx, object o)
//         {
//             return js_push_primitive(ctx, (int)o);
//         }

//         private static JSValue cs_value_cast_uint(JSContext ctx, object o)
//         {
//             return js_push_primitive(ctx, (uint)o);
//         }

//         private static JSValue cs_value_cast_long(JSContext ctx, object o)
//         {
//             return js_push_primitive(ctx, (long)o);
//         }

//         private static JSValue cs_value_cast_ulong(JSContext ctx, object o)
//         {
//             return js_push_primitive(ctx, (ulong)o);
//         }

//         private static JSValue cs_value_cast_float(JSContext ctx, object o)
//         {
//             return js_push_primitive(ctx, (float)o);
//         }

//         private static JSValue cs_value_cast_double(JSContext ctx, object o)
//         {
//             return js_push_primitive(ctx, (double)o);
//         }

//         private static JSValue cs_value_cast_string(JSContext ctx, object o)
//         {
//             return js_push_primitive(ctx, (string)o);
//         }

//         private static JSValue cs_value_cast_type(JSContext ctx, object o)
//         {
//             return js_push_classvalue(ctx, (Type)o);
//         }

//         private static JSValue cs_value_cast_script_value(JSContext ctx, object o)
//         {
//             return js_push_classvalue(ctx, (ScriptValue)o);
//         }

//         private static JSValue cs_value_cast_script_promise(JSContext ctx, object o)
//         {
//             return js_push_classvalue(ctx, (ScriptPromise)o);
//         }
//     }
// }
