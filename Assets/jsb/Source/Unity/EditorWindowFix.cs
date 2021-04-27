#if !JSB_UNITYLESS
#if UNITY_EDITOR
using System;
using System.Reflection;

namespace QuickJS.Unity
{
    using Native;
    using Binding;
    using UnityEngine;
    using UnityEditor;

    public static class EditorWindowFix
    {
        [MonoPInvokeCallbackAttribute(typeof(JSCFunction))]
        public static JSValue BindStatic_GetWindow(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            try
            {
                //TODO: 需要补充 GetWindow 其余重载匹配
                if (argc == 1)
                {
                    System.Type arg0;
                    if (!Values.js_get_classvalue(ctx, argv[0], out arg0))
                    {
                        throw new ParameterException(typeof(UnityEditor.EditorWindow), "GetWindow", typeof(System.Type), 0);
                    }
                    var inject = QuickJS.Unity.EditorWindowFix.js_get_window(ctx, argv[0], arg0);
                    if (!inject.IsUndefined())
                    {
                        return inject;
                    }
                    var ret = UnityEditor.EditorWindow.GetWindow(arg0);
                    return Values.js_push_classvalue(ctx, ret);
                }
                throw new NoSuitableMethodException("GetWindow", argc);
            }
            catch (Exception exception)
            {
                return JSApi.ThrowException(ctx, exception);
            }
        }

        // as extended method
        // public static T CreateWindow<T>(string title, params Type[] desiredDockNextTo) where T : EditorWindow
        // public static T CreateWindow<T>(params Type[] desiredDockNextTo) where T : EditorWindow
        [JSCFunction(true,
            "<T extends EditorWindow>(type: { new(): T }, ...desiredDockNextTo: any[]): T",
            "<T extends EditorWindow>(type: { new(): T }, title: string, ...desiredDockNextTo: any[]): T")]
        public static JSValue CreateWindow(JSContext ctx, JSValue this_obj, int argc, JSValue[] argv)
        {
            if (argc == 0 || JSApi.JS_IsConstructor(ctx, argv[0]) != 1)
            {
                throw new ParameterException("type", typeof(Type), 0);
            }

            if (argc > 1)
            {
                var firstAsTitle = argv[1].IsString() || argv[1].IsNullish();
                var title = firstAsTitle ? JSApi.GetString(ctx, argv[1]) : null;
                var firstArgIndex = firstAsTitle ? 2 : 1;

                return _new_js_editor_window(ctx, argv[0], false, title, argv, firstArgIndex);
            }

            return _new_js_editor_window(ctx, argv[0], false, null, null, 0);
        }

        private static bool __dock(JSValue[] desiredDockNextTo)
        {
            var ContainerWindow = typeof(EditorWindow).Assembly.GetType("UnityEditor.ContainerWindow");
            var ContainerWindow_windows = ContainerWindow.GetProperty("windows", BindingFlags.Public | BindingFlags.Static);
            var ContainerWindow_rootView = ContainerWindow.GetProperty("rootView", BindingFlags.Public);

            var View = typeof(EditorWindow).Assembly.GetType("UnityEditor.View");
            var View_allChildren = View.GetProperty("allChildren", BindingFlags.Public);

            // foreach (var desired in desiredDockNextTo)
            for (var dIndex = 0; dIndex < desiredDockNextTo.Length; dIndex++)
            {
                var desired = desiredDockNextTo[dIndex];
                // ContainerWindow[]
                var windows = (Array)ContainerWindow_windows.GetMethod.Invoke(null, null);
                for (var wIndex = 0; wIndex < windows.Length; wIndex++)
                {
                    var containerWindow = windows.GetValue(wIndex);
                    var rootView = ContainerWindow_rootView.GetMethod.Invoke(containerWindow, null);
                    // View[]
                    var allChildren = (Array)View_allChildren.GetMethod.Invoke(rootView, null);
                    for (var vIndex = 0; vIndex < allChildren.Length; vIndex++)
                    {
                        var view = allChildren.GetValue(vIndex);
                        var dockArea = view; //  as DockArea

                        // DockArea.m_Panes: internal List<EditorWindow> 
                        if (!((UnityEngine.Object)dockArea == null))
                        {
                            //TODO: 区分 JSEditorWindow 与 C# EditorWindow
                            // if (dockArea.m_Panes.Any((EditorWindow pane) => pane.GetType() == desired))
                            // {
                            //     dockArea.AddTab(val);
                            //     return true;
                            // }
                        }
                    }
                }
            }

            return false;
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
                return _new_js_editor_window(ctx, ctor, utility, title, null, 0);
            }
            else if (focus)
            {
                editorWindow.Show();
                editorWindow.Focus();
            }

            return editorWindow.CloneValue();
        }

        private static JSValue _new_js_editor_window(JSContext ctx, JSValue ctor, bool utility, string title, JSValue[] desiredDockNextTo, int desiredDockNextToOffset)
        {
            var editorWindow = ScriptableObject.CreateInstance<JSEditorWindow>();
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
#endif