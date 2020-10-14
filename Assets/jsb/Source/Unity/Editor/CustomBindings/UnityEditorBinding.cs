using System;
using System.Reflection;

namespace jsb.Editor
{
    using QuickJS.Unity;
    using UnityEngine;
    using UnityEditor;

    public class UnityEditorBinding : AbstractBindingProcess
    {
        public bool IsAvailable(MethodInfo methodInfo)
        {
            return methodInfo != null && methodInfo.IsPublic;
        }

        public override void OnPreExporting(BindingManager bindingManager)
        {
            if (!bindingManager.prefs.editorScripting)
            {
                return;
            }

            bindingManager.AddExportedType(typeof(GUI));
            bindingManager.AddExportedType(typeof(GUILayout));

            bindingManager.AddExportedType(typeof(GUIContent)).EditorRuntime();
            bindingManager.AddExportedType(typeof(EditorGUI)).EditorRuntime();
            bindingManager.AddExportedType(typeof(EditorGUILayout)).EditorRuntime();
            bindingManager.AddExportedType(typeof(EditorApplication)).EditorRuntime();
            bindingManager.AddExportedType(typeof(EditorWindow)).EditorRuntime()
                .SetMemberBlocked("GetWindowWithRect")
                //TODO: 此方法需要接管, 待处理, 暂时屏蔽
                .SetMethodBlocked("GetWindow", typeof(Type), typeof(bool), typeof(string), typeof(bool))
                //TODO: 此方法需要接管, 待处理, 暂时屏蔽
                .SetMethodBlocked("GetWindow", typeof(Type), typeof(bool), typeof(string))
                //TODO: 此方法需要接管, 待处理, 暂时屏蔽
                .SetMethodBlocked("GetWindow", typeof(Type), typeof(bool))
                .AddTSMethodDeclaration("static GetWindow<T extends UnityEditor.EditorWindow>(type: { new(): T }): T", "GetWindow", typeof(Type))
                .WriteCSConstructorBinding((bindPoint, cg, info) =>
                {
                    if (bindPoint == BindingPoints.METHOD_BINDING_FULL)
                    {
                        cg.cs.AppendLine("return _js_mono_behaviour_constructor(ctx, new_target);");
                        return true;
                    }

                    return false;
                })
                .WriteCSMethodBinding((bindPoint, cg, info) =>
                {
                    if (bindPoint == BindingPoints.METHOD_BINDING_BEFORE_INVOKE)
                    {
                        cg.cs.AppendLine("var inject = QuickJS.Unity.EditorWindowFix.js_get_window(ctx, argv[0], arg0);");
                        cg.cs.AppendLine("if (!inject.IsUndefined())");
                        using (cg.cs.CodeBlockScope())
                        {
                            cg.cs.AppendLine("return inject;");
                        }

                        return true;
                    }
                    return false;
                }, "GetWindow", typeof(Type))
            ;

            if (!IsAvailable(typeof(EditorWindow).GetMethod("CreateWindow")))
            {
                bindingManager.TransformType(typeof(EditorWindow))
                    .AddExtensionMethod<EditorWindow, Type, EditorWindow>(EditorWindowFix.CreateWindow, "CreateWindow<T extends UnityEditor.EditorWindow>(type: { new(): T }): T");
            }
        }
    }
}
