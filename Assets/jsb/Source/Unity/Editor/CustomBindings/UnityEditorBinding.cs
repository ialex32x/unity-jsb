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
            // [test] editor only
            bindingManager.AddExportedType(typeof(EditorApplication)).EditorRuntime();
            bindingManager.AddExportedType(typeof(EditorWindow)).EditorRuntime()
                .AddTSMethodDeclaration("GetWindow<T extends UnityEditor.EditorWindow>(type: { new(): T }): T",
                    "GetWindow", typeof(Type))
            ;
            
            if (!IsAvailable(typeof(EditorWindow).GetMethod("CreateWindow")))
            {
                bindingManager.TransformType(typeof(EditorWindow))
                    .AddExtensionMethod<EditorWindow, Type, EditorWindow>(UnityEditorBindingStub.CreateWindow, "CreateWindow<T extends UnityEditor.EditorWindow>(type: { new(): T }): T");
            }
        }
    }
}
