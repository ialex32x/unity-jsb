#if !JSB_UNITYLESS
using System;
using System.Reflection;

namespace jsb.Editor
{
    using QuickJS.Native;
    using QuickJS.Unity;
    using QuickJS.Binding;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;

    public class UnityBinding : AbstractBindingProcess
    {
        public override string GetBindingProcessName()
        {
            return "Unity (Basic)";
        }

        public bool IsAvailable(MethodInfo methodInfo)
        {
            return methodInfo != null && methodInfo.IsPublic;
        }

        public override void OnPreCollectAssemblies(BindingManager bindingManager)
        {
            bindingManager.SetAssemblyBlocked("ExCSS.Unity");
            HackGetComponents(bindingManager.TransformType(typeof(GameObject)));
            HackGetComponents(bindingManager.TransformType(typeof(Component)));

            bindingManager.TransformType(typeof(MonoBehaviour))
                .WriteCrossBindingConstructor();

            bindingManager.TransformType(typeof(ScriptableObject))
                .WriteCrossBindingConstructor();

            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            if (buildTarget != BuildTarget.iOS)
            {
                bindingManager.AddTypePrefixBlacklist("UnityEngine.Apple");
            }
            if (buildTarget != BuildTarget.Android)
            {
                bindingManager.AddTypePrefixBlacklist("UnityEngine.Android");
            }
            bindingManager.AddTypePrefixBlacklist("SyntaxTree.");

            // fix d.ts, some C# classes use explicit implemented interface method
            bindingManager.SetTypeBlocked(typeof(UnityEngine.ILogHandler));
            bindingManager.SetTypeBlocked(typeof(UnityEngine.ISerializationCallbackReceiver));
            bindingManager.SetTypeBlocked(typeof(UnityEngine.Playables.ScriptPlayable<>));
            bindingManager.SetTypeBlocked(typeof(AOT.MonoPInvokeCallbackAttribute));

            // SetTypeBlocked(typeof(RendererExtensions));
            bindingManager.TransformType(typeof(UnityEngine.Events.UnityEvent<>))
                .Rename("UnityEvent1");

            bindingManager.TransformType(typeof(UnityEngine.Events.UnityEvent<,>))
                .Rename("UnityEvent2");

            bindingManager.TransformType(typeof(UnityEngine.Events.UnityEvent<,,>))
                .Rename("UnityEvent3");

            bindingManager.TransformType(typeof(UnityEngine.Events.UnityEvent<,,,>))
                .Rename("UnityEvent4");

            bindingManager.TransformType(typeof(UnityEngine.Texture))
                .SetMemberBlocked("imageContentsHash");
            bindingManager.TransformType(typeof(UnityEngine.Texture2D))
                .SetMemberBlocked("alphaIsTransparency"); //TODO: 增加成员的 defines 条件编译功能
            bindingManager.TransformType(typeof(UnityEngine.Input))
                .SetMemberBlocked("IsJoystickPreconfigured"); // specific platform available only
            bindingManager.TransformType(typeof(UnityEngine.MonoBehaviour))
                .SetMemberBlocked("runInEditMode"); // editor only
            bindingManager.TransformType(typeof(UnityEngine.QualitySettings))
                .SetMemberBlocked("streamingMipmapsRenderersPerFrame");
        }

        public override void OnPreExporting(BindingManager bindingManager)
        {
            bindingManager.AddExportedType(typeof(LayerMask));
            bindingManager.AddExportedType(typeof(Color));
            bindingManager.AddExportedType(typeof(Color32));
            bindingManager.AddExportedType(typeof(Vector2));
            bindingManager.AddExportedType(typeof(Vector2Int));
            bindingManager.AddExportedType(typeof(Vector3));
            bindingManager.AddExportedType(typeof(Vector3Int));
            bindingManager.AddExportedType(typeof(Vector4));
            bindingManager.AddExportedType(typeof(Quaternion));
            bindingManager.AddExportedType(typeof(Matrix4x4));
            bindingManager.AddExportedType(typeof(PrimitiveType));
            bindingManager.AddExportedType(typeof(UnityEngine.Object))
                .EnableOperatorOverloading(false)
                ;
            bindingManager.AddExportedType(typeof(GameObject), true);
            bindingManager.AddExportedType(typeof(Camera), true);
            bindingManager.AddExportedType(typeof(Transform), true);
            bindingManager.AddExportedType(typeof(MonoBehaviour), true);
            bindingManager.AddExportedType(typeof(Sprite), true);
            bindingManager.AddExportedType(typeof(SpriteRenderer), true);
            bindingManager.AddExportedType(typeof(Animation), true);
            bindingManager.AddExportedType(typeof(AnimationClip), true);
            bindingManager.AddExportedType(typeof(Animator), true);
            bindingManager.AddExportedType(typeof(AnimationState), true);
            bindingManager.AddExportedType(typeof(WrapMode), true);

            bindingManager.AddExportedType(typeof(Resources)).SetAllConstructorsBlocked();
            bindingManager.AddExportedType(typeof(QuickJS.Unity.JSScriptProperties));
        }
        
        private static TypeTransform HackGetComponents(TypeTransform typeTransform)
        {
            if (typeTransform.type == typeof(GameObject))
            {
                typeTransform.AddTSMethodDeclaration($"AddComponent<T extends Component>(type: {{ new(): T }}): T",
                     "AddComponent", typeof(Type));

                typeTransform.WriteCSMethodOverrideBinding("AddComponent", GameObjectFix.Bind_AddComponent);
                
                typeTransform.WriteCSMethodOverrideBinding("GetComponent", GameObjectFix.Bind_GetComponent);
                typeTransform.WriteCSMethodOverrideBinding("GetComponentInChildren", GameObjectFix.Bind_GetComponentInChildren);
                typeTransform.WriteCSMethodOverrideBinding("GetComponentInParent", GameObjectFix.Bind_GetComponentInParent);
                typeTransform.WriteCSMethodOverrideBinding("GetComponentsInChildren", GameObjectFix.Bind_GetComponentsInChildren);
                typeTransform.WriteCSMethodOverrideBinding("GetComponentsInParent", GameObjectFix.Bind_GetComponentsInParent);
                typeTransform.WriteCSMethodOverrideBinding("GetComponents", GameObjectFix.Bind_GetComponents);
            }
            else
            {
                typeTransform.WriteCSMethodOverrideBinding("GetComponent", ComponentFix.Bind_GetComponent);
                typeTransform.WriteCSMethodOverrideBinding("GetComponentInChildren", ComponentFix.Bind_GetComponentInChildren);
                typeTransform.WriteCSMethodOverrideBinding("GetComponentInParent", ComponentFix.Bind_GetComponentInParent);
                typeTransform.WriteCSMethodOverrideBinding("GetComponentsInChildren", ComponentFix.Bind_GetComponentsInChildren);
                typeTransform.WriteCSMethodOverrideBinding("GetComponentsInParent", ComponentFix.Bind_GetComponentsInParent);
                typeTransform.WriteCSMethodOverrideBinding("GetComponents", ComponentFix.Bind_GetComponents);
            }

            typeTransform.AddTSMethodDeclaration($"GetComponent<T extends Component>(type: {{ new(): T }}): T",
                    "GetComponent", typeof(Type))
                .AddTSMethodDeclaration($"GetComponentInChildren<T extends Component>(type: {{ new(): T }}, includeInactive: boolean): T",
                    "GetComponentInChildren", typeof(Type), typeof(bool))
                .AddTSMethodDeclaration($"GetComponentInChildren<T extends Component>(type: {{ new(): T }}): T",
                    "GetComponentInChildren", typeof(Type))
                .AddTSMethodDeclaration($"GetComponentInParent<T extends Component>(type: {{ new(): T }}): T",
                    "GetComponentInParent", typeof(Type))
                .AddTSMethodDeclaration($"GetComponents<T extends Component>(type: {{ new(): T }}): T[]",
                    "GetComponents", typeof(Type))
                .AddTSMethodDeclaration($"GetComponentsInChildren<T extends Component>(type: {{ new(): T }}, includeInactive: boolean): T[]",
                    "GetComponentsInChildren", typeof(Type), typeof(bool))
                .AddTSMethodDeclaration($"GetComponentsInChildren<T extends Component>(type: {{ new(): T }}): T[]",
                    "GetComponentsInChildren", typeof(Type))
                .AddTSMethodDeclaration($"GetComponentsInParent<T extends Component>(type: {{ new(): T }}, includeInactive: boolean): T[]",
                    "GetComponentsInParent", typeof(Type), typeof(bool))
                .AddTSMethodDeclaration($"GetComponentsInParent<T extends Component>(type: {{ new(): T }}): T[]",
                    "GetComponentsInParent", typeof(Type))
                ;
            return typeTransform;
        }
    }
}
#endif