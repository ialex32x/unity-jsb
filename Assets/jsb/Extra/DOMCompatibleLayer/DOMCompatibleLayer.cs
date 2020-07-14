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

    public class DOMCompatibleLayer : Values, IScriptFinalize
    {
        public void OnJSFinalize()
        {
            throw new NotImplementedException();
        }


        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue js_document_createElement(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            return JSApi.JS_NewObject(ctx);
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue js_element_appendChild(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            try
            {
                if (argc < 1)
                {
                    return JSApi.JS_ThrowInternalError(ctx, "element expected");
                }
                if (!argv[0].IsObject())
                {
                    return JSApi.JS_ThrowInternalError(ctx, "element expected");
                }

                var context = ScriptEngine.GetContext(ctx);
                var srcProp = JSApi.JS_GetProperty(ctx, argv[0], context.GetAtom("src"));
                var srcValue = JSApi.GetString(ctx, srcProp);
                JSApi.JS_FreeValue(ctx, srcProp);
                var co = context.GetCoroutineManager();
                if (co != null)
                {
                    co.Load(context, srcValue);
                }
                return JSApi.JS_UNDEFINED;
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        public static void Bind(TypeRegister register)
        {
            var context = register.GetContext();
            JSContext ctx = context;
            var globalObject = context.GetGlobalObject();
            JSApi.JS_SetProperty(ctx, globalObject, register.GetAtom("window"), JSApi.JS_DupValue(ctx, globalObject));
            JSApi.JS_FreeValue(ctx, globalObject);

            //TODO: 临时代码
            var ns_document = register.CreateNamespace("document");
            ns_document.AddFunction("createElement", js_document_createElement, 1);
            var ns_document_head = ns_document.CreateNamespace("head");
            ns_document_head.AddFunction("appendChild", js_element_appendChild, 1);
            ns_document_head.Close();
            ns_document.Close();
        }
    }
}
