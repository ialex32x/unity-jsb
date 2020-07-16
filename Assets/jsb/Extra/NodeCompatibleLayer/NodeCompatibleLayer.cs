using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections;
using System.Collections.Generic;

namespace QuickJS.Extra
{
    using AOT;
    using UnityEngine;
    using QuickJS;
    using QuickJS.IO;
    using QuickJS.Native;
    using QuickJS.Binding;

    // 提供 nodejs 环境的少量基本兼容性 
    // 提供 global
    public class NodeCompatibleLayer : Values
    {

        public static void Bind(TypeRegister register)
        {
            var context = register.GetContext();
            JSContext ctx = context;
            var globalObject = context.GetGlobalObject();
            JSApi.JS_SetProperty(ctx, globalObject, register.GetAtom("global"), JSApi.JS_DupValue(ctx, globalObject));
            JSApi.JS_FreeValue(ctx, globalObject);
        }
    }
}
