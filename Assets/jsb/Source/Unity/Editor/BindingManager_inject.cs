using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public partial class BindingManager
    {
        private static TypeTransform HackGetComponents(TypeTransform typeTransform)
        {
            if (typeTransform.type == typeof(GameObject))
            {
                typeTransform.AddTSMethodDeclaration($"AddComponent<T extends Component>(type: {{ new(): T }}): T",
                     "AddComponent", typeof(Type));
            }

            typeTransform.AddTSMethodDeclaration($"GetComponent<T extends Component>(type: {{ new(): T }}): T",
                    "GetComponent", typeof(Type))
                .AddTSMethodDeclaration($"GetComponentInChildren<T extends Component>(type: {{ new(): T }}, includeInactive: boolean): T",
                    "GetComponentInChildren", typeof(Type), typeof(bool))
                .AddTSMethodDeclaration($"GetComponentInChildren<T extends Component>(type: {{ new(): T }}): T",
                    "GetComponentInChildren", typeof(Type))
                .AddTSMethodDeclaration($"GetComponentInParent<T extends Component>(type: {{ new(): T }}): T",
                    "GetComponentInParent", typeof(Type))
                // .AddTSMethodDeclaration($"GetComponents<T extends Component>(type: {{ new(): T }}, results: any): void", 
                //     "GetComponents", typeof(Type))
                .AddTSMethodDeclaration($"GetComponents<T extends Component>(type: {{ new(): T }}): T[]",
                    "GetComponents", typeof(Type))
                .SetMethodReturnPusher("js_push_classvalue_array", "GetComponents", typeof(Type))
                .AddTSMethodDeclaration($"GetComponentsInChildren<T extends Component>(type: {{ new(): T }}, includeInactive: boolean): T[]",
                    "GetComponentsInChildren", typeof(Type), typeof(bool))
                .SetMethodReturnPusher("js_push_classvalue_array", "GetComponentsInChildren", typeof(Type), typeof(bool))
                .AddTSMethodDeclaration($"GetComponentsInChildren<T extends Component>(type: {{ new(): T }}): T[]",
                    "GetComponentsInChildren", typeof(Type))
                .SetMethodReturnPusher("js_push_classvalue_array", "GetComponentsInChildren", typeof(Type))
                .AddTSMethodDeclaration($"GetComponentsInParent<T extends Component>(type: {{ new(): T }}, includeInactive: boolean): T[]",
                    "GetComponentsInParent", typeof(Type), typeof(bool))
                .SetMethodReturnPusher("js_push_classvalue_array", "GetComponentsInParent", typeof(Type), typeof(bool))
                .AddTSMethodDeclaration($"GetComponentsInParent<T extends Component>(type: {{ new(): T }}): T[]",
                    "GetComponentsInParent", typeof(Type))
                .SetMethodReturnPusher("js_push_classvalue_array", "GetComponentsInParent", typeof(Type))
                .WriteCSMethodBinding((bindPoint, cg, info) =>
                {
                    if (bindPoint == BindingPoints.METHOD_BINDING_BEFORE_INVOKE)
                    {
                        cg.cs.AppendLine("var inject = _js_game_object_get_component(ctx, argv[0], self, arg0);");
                        cg.cs.AppendLine("if (!inject.IsUndefined())");
                        using (cg.cs.CodeBlockScope())
                        {
                            cg.cs.AppendLine("return inject;");
                        }

                        return true;
                    }
                    return false;
                }, "GetComponent", typeof(Type))
                .WriteCSMethodBinding((bindPoint, cg, info) =>
                {
                    if (bindPoint == BindingPoints.METHOD_BINDING_BEFORE_INVOKE)
                    {
                        cg.cs.AppendLine("var inject = _js_game_object_get_component_in_children(ctx, argv[0], self, arg0, false);");
                        cg.cs.AppendLine("if (!inject.IsUndefined())");
                        using (cg.cs.CodeBlockScope())
                        {
                            cg.cs.AppendLine("return inject;");
                        }

                        return true;
                    }
                    return false;
                }, "GetComponentInChildren", typeof(Type))
                .WriteCSMethodBinding((bindPoint, cg, info) =>
                {
                    if (bindPoint == BindingPoints.METHOD_BINDING_BEFORE_INVOKE)
                    {
                        cg.cs.AppendLine("var inject = _js_game_object_get_component_in_children(ctx, argv[0], self, arg0, arg1);");
                        cg.cs.AppendLine("if (!inject.IsUndefined())");
                        using (cg.cs.CodeBlockScope())
                        {
                            cg.cs.AppendLine("return inject;");
                        }

                        return true;
                    }
                    return false;
                }, "GetComponentInChildren", typeof(Type), typeof(bool))
                .WriteCSMethodBinding((bindPoint, cg, info) =>
                {
                    if (bindPoint == BindingPoints.METHOD_BINDING_BEFORE_INVOKE)
                    {
                        cg.cs.AppendLine("var inject = _js_game_object_get_component_in_parent(ctx, argv[0], self, arg0, false);");
                        cg.cs.AppendLine("if (!inject.IsUndefined())");
                        using (cg.cs.CodeBlockScope())
                        {
                            cg.cs.AppendLine("return inject;");
                        }

                        return true;
                    }
                    return false;
                }, "GetComponentInParent", typeof(Type))
                .WriteCSMethodBinding((bindPoint, cg, info) =>
                {
                    if (bindPoint == BindingPoints.METHOD_BINDING_BEFORE_INVOKE)
                    {
                        cg.cs.AppendLine("var inject = _js_game_object_get_component_in_parent(ctx, argv[0], self, arg0, arg1);");
                        cg.cs.AppendLine("if (!inject.IsUndefined())");
                        using (cg.cs.CodeBlockScope())
                        {
                            cg.cs.AppendLine("return inject;");
                        }

                        return true;
                    }
                    return false;
                }, "GetComponentInParent", typeof(Type), typeof(bool))
                .WriteCSMethodBinding((bindPoint, cg, info) =>
                {
                    if (bindPoint == BindingPoints.METHOD_BINDING_BEFORE_INVOKE)
                    {
                        cg.cs.AppendLine("var inject = _js_game_object_get_components_in_children(ctx, argv[0], self, arg0, false);");
                        cg.cs.AppendLine("if (!inject.IsUndefined())");
                        using (cg.cs.CodeBlockScope())
                        {
                            cg.cs.AppendLine("return inject;");
                        }

                        return true;
                    }
                    return false;
                }, "GetComponentsInChildren", typeof(Type))
                .WriteCSMethodBinding((bindPoint, cg, info) =>
                {
                    if (bindPoint == BindingPoints.METHOD_BINDING_BEFORE_INVOKE)
                    {
                        cg.cs.AppendLine("var inject = _js_game_object_get_components_in_children(ctx, argv[0], self, arg0, arg1);");
                        cg.cs.AppendLine("if (!inject.IsUndefined())");
                        using (cg.cs.CodeBlockScope())
                        {
                            cg.cs.AppendLine("return inject;");
                        }

                        return true;
                    }
                    return false;
                }, "GetComponentsInChildren", typeof(Type), typeof(bool))
                .WriteCSMethodBinding((bindPoint, cg, info) =>
                {
                    if (bindPoint == BindingPoints.METHOD_BINDING_BEFORE_INVOKE)
                    {
                        cg.cs.AppendLine("var inject = _js_game_object_get_components_in_parent(ctx, argv[0], self, arg0, false);");
                        cg.cs.AppendLine("if (!inject.IsUndefined())");
                        using (cg.cs.CodeBlockScope())
                        {
                            cg.cs.AppendLine("return inject;");
                        }

                        return true;
                    }
                    return false;
                }, "GetComponentsInParent", typeof(Type))
                .WriteCSMethodBinding((bindPoint, cg, info) =>
                {
                    if (bindPoint == BindingPoints.METHOD_BINDING_BEFORE_INVOKE)
                    {
                        cg.cs.AppendLine("var inject = _js_game_object_get_components_in_parent(ctx, argv[0], self, arg0, arg1);");
                        cg.cs.AppendLine("if (!inject.IsUndefined())");
                        using (cg.cs.CodeBlockScope())
                        {
                            cg.cs.AppendLine("return inject;");
                        }

                        return true;
                    }
                    return false;
                }, "GetComponentsInParent", typeof(Type), typeof(bool))
                .WriteCSMethodBinding((bindPoint, cg, info) =>
                {
                    if (bindPoint == BindingPoints.METHOD_BINDING_BEFORE_INVOKE)
                    {
                        cg.cs.AppendLine("var inject = _js_game_object_get_components(ctx, argv[0], self, arg0);");
                        cg.cs.AppendLine("if (!inject.IsUndefined())");
                        using (cg.cs.CodeBlockScope())
                        {
                            cg.cs.AppendLine("return inject;");
                        }

                        return true;
                    }
                    return false;
                }, "GetComponents", typeof(Type))
                .WriteCSMethodBinding((bindPoint, cg, info) =>
                {
                    if (bindPoint == BindingPoints.METHOD_BINDING_BEFORE_INVOKE)
                    {
                        cg.cs.AppendLine("var inject = _js_game_object_get_components(ctx, argv[0], self, arg0, arg1);");
                        cg.cs.AppendLine("if (!inject.IsUndefined())");
                        using (cg.cs.CodeBlockScope())
                        {
                            cg.cs.AppendLine("return inject;");
                        }

                        return true;
                    }
                    return false;
                }, "GetComponents", typeof(Type), typeof(List<Component>));
            return typeTransform;
        }
    }
}
