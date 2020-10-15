using System;
using System.Reflection;

namespace jsb.Editor
{
    using QuickJS.Native;
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

            bindingManager.AddExportedType(typeof(GUI)).SetAllConstructorsBlocked();
            bindingManager.AddExportedType(typeof(GUIUtility)).SetAllConstructorsBlocked();
            bindingManager.AddExportedType(typeof(GUILayout)).SetAllConstructorsBlocked();
            bindingManager.AddExportedType(typeof(GUILayoutOption));
            bindingManager.AddExportedType(typeof(GUIContent));
            bindingManager.AddExportedType(typeof(GUISkin));
            bindingManager.AddExportedType(typeof(GUIStyle));
            bindingManager.AddExportedType(typeof(ScaleMode));
            bindingManager.AddExportedType(typeof(FocusType));
            bindingManager.AddExportedType(typeof(RectInt));
            bindingManager.AddExportedType(typeof(Bounds));
            bindingManager.AddExportedType(typeof(BoundsInt));
            bindingManager.AddExportedType(typeof(Gradient));
            bindingManager.AddExportedType(typeof(AnimationCurve));
            bindingManager.AddExportedType(typeof(Event));
            bindingManager.AddExportedType(typeof(Coroutine));
            bindingManager.AddExportedType(typeof(System.Collections.IEnumerator));
            bindingManager.AddExportedType(typeof(System.Collections.Generic.IEnumerable<string>));

            bindingManager.AddExportedType(typeof(MessageType)).EditorRuntime();
            bindingManager.AddExportedType(typeof(Hash128)).EditorRuntime();
            bindingManager.AddExportedType(typeof(ImportAssetOptions)).EditorRuntime();
            bindingManager.AddExportedType(typeof(ScriptingRuntimeVersion)).EditorRuntime();
            bindingManager.AddExportedType(typeof(MouseCursor)).EditorRuntime();
            bindingManager.AddExportedType(typeof(PauseState)).EditorRuntime();
            bindingManager.AddExportedType(typeof(PlayModeStateChange)).EditorRuntime();
            bindingManager.AddExportedType(typeof(ExportPackageOptions)).EditorRuntime();
            bindingManager.AddExportedType(typeof(ForceReserializeAssetsOptions)).EditorRuntime();
            bindingManager.AddExportedType(typeof(StatusQueryOptions)).EditorRuntime();
            bindingManager.AddExportedType(typeof(SerializedObject)).EditorRuntime();
            bindingManager.AddExportedType(typeof(SerializedProperty)).EditorRuntime();
            bindingManager.AddExportedType(typeof(SerializedPropertyType)).EditorRuntime();
            bindingManager.AddExportedType(typeof(AssetDatabase)).EditorRuntime().SetAllConstructorsBlocked();
            bindingManager.AddExportedType(typeof(EditorGUI)).EditorRuntime().SetAllConstructorsBlocked();
            bindingManager.AddExportedType(typeof(EditorGUIUtility)).EditorRuntime().SetAllConstructorsBlocked();
            bindingManager.AddExportedType(typeof(EditorGUILayout)).EditorRuntime().SetAllConstructorsBlocked();
            bindingManager.AddExportedType(typeof(EditorApplication)).EditorRuntime().SetAllConstructorsBlocked();
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
                .AddStaticMethod(EditorWindowFix.CreateWindow)
            ;
        }
    }
}
