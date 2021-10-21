import { EditorGUI, EditorGUILayout, EditorUtility } from "UnityEditor";
import { Object as UObject, Quaternion, Vector2, Vector3, Vector4 } from "UnityEngine";
import { PropertyMetaInfo, PropertyTypeID } from "../runtime/class_decorators";

interface IPropertyDrawer {
    draw(value: any, prop: PropertyMetaInfo, label: string, editablePE: boolean): any;
}

export let DefaultPropertyDrawers: { [key: string]: IPropertyDrawer } = {
    "bool": {
        draw(rawValue: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let oldValue: boolean = !!rawValue;
            if (editablePE) {
                let newValue = EditorGUILayout.Toggle(label, oldValue);
                return newValue;
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
    },
    "int": {
        draw(rawValue: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let oldValue: number = rawValue || 0;
            if (editablePE) {
                let newValue = EditorGUILayout.IntField(label, oldValue);
                return newValue;
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
    },
    "float": {
        draw(rawValue: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let oldValue: number = rawValue || 0;
            if (editablePE) {
                let newValue = EditorGUILayout.FloatField(label, oldValue);
                return newValue;
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
    },
    "double": {
        draw(rawValue: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let oldValue: number = rawValue || 0;
            if (editablePE) {
                let newValue = EditorGUILayout.FloatField(label, oldValue);
                return newValue;
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
    },
    "string": {
        draw(rawValue: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let oldValue: string = rawValue || "";
            if (editablePE) {
                let newValue = EditorGUILayout.TextField(label, oldValue);
                return newValue;
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
    },
    "object": {
        draw(rawValue: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let oldValue: any = rawValue instanceof UObject || null;
            if (editablePE) {
                let allowSceneObjects = prop.extra && prop.extra.allowSceneObjects;
                let newValue = EditorGUILayout.ObjectField(label, oldValue,
                    prop.extra && prop.extra.type || Object,
                    typeof allowSceneObjects === "boolean" ? allowSceneObjects : true);

                return newValue;
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(label, oldValue, Object, false);
                EditorGUI.EndDisabledGroup();
            }
        },
    },
    "Vector2": {
        draw(rawValue: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let oldValue: Vector2 = rawValue || Vector2.zero;
            if (editablePE) {
                let newValue = EditorGUILayout.Vector2Field(label, oldValue);
                return newValue;
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector2Field(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
    },
    "Vector3": {
        draw(rawValue: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let oldValue: Vector3 = rawValue || Vector3.zero;
            if (editablePE) {
                let newValue = EditorGUILayout.Vector3Field(label, oldValue);
                return newValue;
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector3Field(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
    },
    "Vector4": {
        draw(rawValue: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let oldValue: Vector4 = rawValue || Vector4.zero;
            if (editablePE) {
                let newValue = EditorGUILayout.Vector4Field(label, oldValue);
                return newValue;
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector4Field(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
    },
    "Quaternion": {
        draw(rawValue: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let oldValue: Vector4 = rawValue || Quaternion.identity;
            if (editablePE) {
                let newValue = EditorGUILayout.Vector4Field(label, oldValue);
                return newValue;
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector4Field(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
    },
}
