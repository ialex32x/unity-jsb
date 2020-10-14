#if UNITY_EDITOR
using System;
using System.Reflection;

namespace QuickJS.Unity
{
    using Native;
    using UnityEngine;
    using UnityEditor;

    public class EditorWindowFix
    {
        // as extended method
        public static EditorWindow CreateWindow(EditorWindow editorWindow, Type t)
        {
            //TODO: 提供新版本提供的 CreateWindow 接口的兼容实现, Not Implemented 
            return default(EditorWindow);
        }

        // inject: GetWindow(Type)
        public static JSValue js_get_window(JSContext ctx, JSValue ctor, Type type)
        {
            if (JSApi.JS_IsConstructor(ctx, ctor) == 1)
            {
                var header = JSApi.jsb_get_payload_header(ctor);
                if (header.type_id == BridgeObjectType.None) // it's a plain js value
                {
                    if (type == typeof(EditorWindow))
                    {
                        var bridgeValue = _js_get_window_clone(ctx, ctor, false, null, true);
                        if (!bridgeValue.IsUndefined())
                        {
                            return bridgeValue;
                        }

                        return JSApi.JS_NULL; // or return an empty array?
                    }
                }
            }

            return JSApi.JS_UNDEFINED;
        }

        private static JSValue _js_get_window_clone(JSContext ctx, JSValue ctor, bool utility, string title, bool focus)
        {
            var t = typeof(JSEditorWindow);
            var array = Resources.FindObjectsOfTypeAll(t);
            var editorWindow = _get_js_editor_window(array, ctor);
            if (!editorWindow)
            {
                editorWindow = ScriptableObject.CreateInstance<JSEditorWindow>();
                var cache = ScriptEngine.GetObjectCache(ctx);
                var object_id = cache.AddObject(editorWindow, false);
                var val = JSApi.jsb_construct_bridge_object(ctx, ctor, object_id);
                if (val.IsException())
                {
                    cache.RemoveObject(object_id);
                }
                else
                {
                    cache.AddJSValue(editorWindow, val);
                    editorWindow.SetBridge(ctx, val, ctor);
                    // JSApi.JSB_SetBridgeType(ctx, val, type_id);
                }

                if (title != null)
                {
                    editorWindow.titleContent = new GUIContent(title);
                }
                if (utility)
                {
                    editorWindow.ShowUtility();
                }
                else
                {
                    editorWindow.Show();
                }

                return val;
            }
            else if (focus)
            {
                editorWindow.Show();
                editorWindow.Focus();
            }

            return editorWindow.CloneValue();
        }

        private static JSEditorWindow _get_js_editor_window(Object[] array, JSValue ctor)
        {
            for (int i = 0, len = array.Length; i < len; i++)
            {
                var ew = (JSEditorWindow)array[0];
                if (ew.IsInstanceOf(ctor) == 1)
                {
                    return ew;
                }
            }
            return null;
        }
    }
}
#endif
