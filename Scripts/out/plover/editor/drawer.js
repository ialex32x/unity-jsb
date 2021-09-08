"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.DefaultPropertyDrawer = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
class DefaultPropertyDrawer {
    static draw(type, target, prop, label, editablePE) {
        let d = DefaultPropertyDrawers[type];
        if (typeof d !== "undefined") {
            d.draw(target, prop, label, editablePE);
            return true;
        }
        return false;
    }
}
exports.DefaultPropertyDrawer = DefaultPropertyDrawer;
let DefaultPropertyDrawers = {
    "bool": {
        draw(self, prop, label, editablePE) {
            let propertyKey = prop.propertyKey;
            let oldValue = !!self[propertyKey];
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.Toggle(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    UnityEditor_1.EditorUtility.SetDirty(self);
                }
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.Toggle(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
    },
    "float": {
        draw(self, prop, label, editablePE) {
            let propertyKey = prop.propertyKey;
            let oldValue = self[propertyKey] || 0;
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.FloatField(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    UnityEditor_1.EditorUtility.SetDirty(self);
                }
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.FloatField(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
    },
    "double": {
        draw(self, prop, label, editablePE) {
            let propertyKey = prop.propertyKey;
            let oldValue = self[propertyKey] || 0;
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.FloatField(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    UnityEditor_1.EditorUtility.SetDirty(self);
                }
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.FloatField(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
    },
    "Vector3": {
        draw(self, prop, label, editablePE) {
            let propertyKey = prop.propertyKey;
            let oldValue = self[propertyKey] || UnityEngine_1.Vector3.zero;
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.Vector3Field(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    UnityEditor_1.EditorUtility.SetDirty(self);
                }
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.Vector3Field(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
    },
    "Vector4": {
        draw(self, prop, label, editablePE) {
            let propertyKey = prop.propertyKey;
            let oldValue = self[propertyKey] || UnityEngine_1.Vector4.zero;
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.Vector4Field(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    UnityEditor_1.EditorUtility.SetDirty(self);
                }
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.Vector4Field(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
    },
    "Quaternion": {
        draw(self, prop, label, editablePE) {
            let propertyKey = prop.propertyKey;
            let oldValue = self[propertyKey] || UnityEngine_1.Quaternion.identity;
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.Vector4Field(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    UnityEditor_1.EditorUtility.SetDirty(self);
                }
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