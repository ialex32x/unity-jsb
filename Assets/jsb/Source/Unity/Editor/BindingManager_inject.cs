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
