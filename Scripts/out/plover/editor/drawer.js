"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.DefaultPropertyDrawers = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
exports.DefaultPropertyDrawers = {
    "bool": {
        draw(rawValue, prop, label, editablePE) {
            let oldValue = !!rawValue;
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.Toggle(label, oldValue);
                return newValue;
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.Toggle(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
    },
    "int": {
        draw(rawValue, prop, label, editablePE) {
            let oldValue = rawValue || 0;
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.IntField(label, oldValue);
                return newValue;
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.IntField(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
    },
    "float": {
        draw(rawValue, prop, label, editablePE) {
            let oldValue = rawValue || 0;
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.FloatField(label, oldValue);
                return newValue;
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.FloatField(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
    },
    "double": {
        draw(rawValue, prop, label, editablePE) {
            let oldValue = rawValue || 0;
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.FloatField(label, oldValue);
                return newValue;
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.FloatField(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
    },
    "string": {
        draw(rawValue, prop, label, editablePE) {
            let oldValue = rawValue || "";
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.TextField(label, oldValue);
                return newValue;
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.TextField(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
    },
    "object": {
        draw(rawValue, prop, label, editablePE) {
            let oldValue = rawValue instanceof UnityEngine_1.Object || null;
            if (editablePE) {
                let allowSceneObjects = prop.extra && prop.extra.allowSceneObjects;
                let newValue = UnityEditor_1.EditorGUILayout.ObjectField(label, oldValue, prop.extra && prop.extra.type || Object, typeof allowSceneObjects === "boolean" ? allowSceneObjects : true);
                return newValue;
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.ObjectField(label, oldValue, Object, false);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
    },
    "Vector2": {
        draw(rawValue, prop, label, editablePE) {
            let oldValue = rawValue || UnityEngine_1.Vector2.zero;
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.Vector2Field(label, oldValue);
                return newValue;
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.Vector2Field(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
    },
    "Vector3": {
        draw(rawValue, prop, label, editablePE) {
            let oldValue = rawValue || UnityEngine_1.Vector3.zero;
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.Vector3Field(label, oldValue);
                return newValue;
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.Vector3Field(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
    },
    "Vector4": {
        draw(rawValue, prop, label, editablePE) {
            let oldValue = rawValue || UnityEngine_1.Vector4.zero;
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.Vector4Field(label, oldValue);
                return newValue;
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.Vector4Field(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
    },
    "Quaternion": {
        draw(rawValue, prop, label, editablePE) {
            let oldValue = rawValue || UnityEngine_1.Quaternion.identity;
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.Vector4Field(label, oldValue);
                return newValue;
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.Vector4Field(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
    },
};
//# sourceMappingURL=drawer.js.map