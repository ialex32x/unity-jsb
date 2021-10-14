import { EditorGUI, EditorGUILayout, EditorUtility } from "UnityEditor";
import { Quaternion, Vector3, Vector4 } from "UnityEngine";
import { PropertyMetaInfo, PropertyTypeID } from "../runtime/class_decorators";

interface IPropertyDrawer {
    draw(target: any, prop: PropertyMetaInfo, label: string, editablePE: boolean): void;
}

export let DefaultPropertyDrawers: { [key: string]: IPropertyDrawer } = {
    "bool": {
        draw(self: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let propertyKey = prop.propertyKey;
            let oldValue: boolean = !!self[propertyKey];
            if (editablePE) {
                let newValue = EditorGUILayout.Toggle(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    EditorUtility.SetDirty(self);
                }
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
    },
    "float": {
        draw(self: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let propertyKey = prop.propertyKey;
            let oldValue: number = self[propertyKey] || 0;
            if (editablePE) {
                let newValue = EditorGUILayout.FloatField(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    EditorUtility.SetDirty(self);
                }
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
    },
    "double": {
        draw(self: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let propertyKey = prop.propertyKey;
            let oldValue: number = self[propertyKey] || 0;
            if (editablePE) {
                let newValue = EditorGUILayout.FloatField(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    EditorUtility.SetDirty(self);
                }
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
    },
    "Vector3": {
        draw(self: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let propertyKey = prop.propertyKey;
            let oldValue: Vector3 = self[propertyKey] || Vector3.zero;
            if (editablePE) {
                let newValue = EditorGUILayout.Vector3Field(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    EditorUtility.SetDirty(self);
                }
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector3Field(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
    },
    "Vector4": {
        draw(self: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let propertyKey = prop.propertyKey;
            let oldValue: Vector4 = self[propertyKey] || Vector4.zero;
            if (editablePE) {
                let newValue = EditorGUILayout.Vector4Field(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    EditorUtility.SetDirty(self);
                }
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector4Field(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
    },
    "Quaternion": {
        draw(self: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let propertyKey = prop.propertyKey;
            let oldValue: Vector4 = self[propertyKey] || Quaternion.identity;
            if (editablePE) {
                let newValue = EditorGUILayout.Vector4Field(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    EditorUtility.SetDirty(self);
                }
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector4Field(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
    },
}
