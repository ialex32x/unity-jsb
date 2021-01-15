using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;
    using QuickJS.Native;

    public class ReflectBindValueOp
    {
        public static bool js_get_tvar<T>(JSContext ctx, JSValue val, out T o)
        {
            o = default(T);
            throw new NotImplementedException();
        }

        public static JSValue js_push_tvar<T>(JSContext ctx, T o)
        {
            throw new NotImplementedException();
        }
    }
}