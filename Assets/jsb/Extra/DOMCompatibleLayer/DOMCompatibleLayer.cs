using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections;
using System.Collections.Generic;

namespace QuickJS.Extra
{
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
                var co = context.GetAsyncManager();
                if (co != null)
                {
                    co.EvalSourceAsync(context, srcValue);
                }
                return JSApi.JS_UNDEFINED;
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue js_self_addEventListener(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            Debug.LogFormat("js_self_addEventListener");
            //TODO: self.addEventListener("beforeunload", function () { ... });
            return JSApi.JS_UNDEFINED;
        }

        [MonoPInvokeCallback(typeof(JSCFunction))]
        public static JSValue js_self_location_reload(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            Debug.LogFormat("js_self_location_reload");
            //TODO: self.location.reload();
            return JSApi.JS_UNDEFINED;
        }

        public static void Bind(TypeRegister register, Uri uri)
        {
            //TODO: 临时代码
            
            var context = register.GetContext();
            JSContext ctx = context;
            var globalObject = context.GetGlobalObject();

            var locationObj = JSApi.JS_NewObject(ctx);
            JSApi.JS_SetProperty(ctx, locationObj, register.GetAtom("href"), JSApi.JS_NewString(ctx, uri.ToString()));
            JSApi.JS_SetProperty(ctx, locationObj, register.GetAtom("port"), JSApi.JS_NewInt32(ctx, uri.Port));
            JSApi.JS_SetProperty(ctx, locationObj, register.GetAtom("hostname"), JSApi.JS_NewString(ctx, uri.Host));
            JSApi.JS_SetProperty(ctx, locationObj, register.GetAtom("protocol"), JSApi.JS_NewString(ctx, uri.Scheme));
            JSApi.JS_SetProperty(ctx, locationObj, register.GetAtom("search"), JSApi.JS_NewString(ctx, ""));
            JSApi.JS_SetProperty(ctx, locationObj, register.GetAtom("reload"), JSApi.JS_NewCFunction(ctx, js_self_location_reload, "reload", 0));
            JSApi.JS_SetProperty(ctx, globalObject, register.GetAtom("location"), JSApi.JS_DupValue(ctx, locationObj));
            JSApi.JS_SetProperty(ctx, globalObject, register.GetAtom("window"), JSApi.JS_DupValue(ctx, globalObject));
            {
                var selfObj = JSApi.JS_NewObject(ctx);

                JSApi.JS_SetProperty(ctx, selfObj, register.GetAtom("location"), JSApi.JS_DupValue(ctx, locationObj));
                JSApi.JS_SetProperty(ctx, selfObj, register.GetAtom("addEventListener"), JSApi.JS_NewCFunction(ctx, js_self_addEventListener, "addEventListener", 2));
                JSApi.JS_SetProperty(ctx, globalObject, register.GetAtom("self"), selfObj);
            }
            JSApi.JS_FreeValue(ctx, locationObj);

            var ns_document = JSApi.JS_NewObject(ctx);
            context.AddFunction(ns_document, "createElement", js_document_createElement, 1);
            {
                var ns_head = JSApi.JS_NewObject(ctx);
                context.AddFunction(ns_head, "appendChild", js_element_appendChild, 1);
                JSApi.JS_SetProperty(ctx, ns_document, register.GetAtom("head"), ns_head);
            }
            JSApi.JS_SetProperty(ctx, globalObject, register.GetAtom("document"), ns_document);

            JSApi.JS_FreeValue(ctx, globalObject);
        }
    }
}
