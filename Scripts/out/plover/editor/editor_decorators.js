"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.EditorUtil = exports.DefaultEditor = exports.ScriptEditorWindow = exports.ScriptEditor = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
const class_decorators_1 = require("../runtime/class_decorators");
const drawer_1 = require("./drawer");
let Symbol_CustomEditor = Symbol.for("CustomEditor");
function ScriptEditor(forType) {
    return function (editorType) {
        forType.prototype[Symbol_CustomEditor] = editorType;
        return editorType;
    };
}
exports.ScriptEditor = ScriptEditor;
function ScriptEditorWindow(meta) {
    return function (target) {
        return target;
    };
}
exports.ScriptEditorWindow = ScriptEditorWindow;
class DefaultEditor extends UnityEditor_1.Editor {
    OnInspectorGUI() {
        EditorUtil.draw(this.target);
    }
}
exports.DefaultEditor = DefaultEditor;
class EditorUtil {
    static getCustomEditor(forType) {
        return forType[Symbol_CustomEditor] || DefaultEditor;
    }
    /**
     * 默认编辑器绘制行为
     */
    static draw(target, extra) {
        class_decorators_1.SerializationUtil.forEach(target, extra, (propertyKey, slot, self, extra) => {
            if (slot.visible) {
                let label = slot.label || propertyKey;
                let editablePE = slot.editable && (!slot.editorOnly || !UnityEditor_1.EditorApplication.isPlaying);
                if (typeof slot.type === "string") {
                    switch (slot.type) {
                        case "int": {
                            let oldValue = self[propertyKey];
                            if (editablePE) {
                                let newValue = UnityEditor_1.EditorGUILayout.IntField(label, oldValue);
                                if (newValue != oldValue) {
                                    self[propertyKey] = newValue;
                                    UnityEditor_1.EditorUtility.SetDirty(self);
                                }
                            }
                            else {
                                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                                UnityEditor_1.EditorGUILayout.IntField(label, oldValue);
                                UnityEditor_1.EditorGUI.EndDisabledGroup();
                            }
                            break;
                        }
                        case "float": {
                            let oldValue = self[propertyKey];
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
                            break;
                        }
                        case "string": {
                            let oldValue = self[propertyKey];
                            if (typeof oldValue !== "string") {
                                oldValue = "" + oldValue;
                            }
                            if (editablePE) {
                                let newValue = UnityEditor_1.EditorGUILayout.TextField(label, oldValue);
                                if (newValue != oldValue) {
                                    self[propertyKey] = newValue;
                                    UnityEditor_1.EditorUtility.SetDirty(self);
                                }
                            }
                            else {
                                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                                UnityEditor_1.EditorGUILayout.TextField(label, oldValue);
                                UnityEditor_1.EditorGUI.EndDisabledGroup();
                            }
                            break;
                        }
                        case "object": {
                            let oldValue = self[propertyKey];
                            if (typeof oldValue !== "object") {
                                oldValue = null;
                            }
                            if (editablePE) {
                                let allowSceneObjects = slot.extra && slot.extra.allowSceneObjects;
                                let newValue = UnityEditor_1.EditorGUILayout.ObjectField(label, oldValue, slot.extra && slot.extra.type || UnityEngine_1.Object, typeof allowSceneObjects === "boolean" ? allowSceneObjects : true);
                                if (newValue != oldValue) {
                                    self[propertyKey] = newValue;
                                    UnityEditor_1.EditorUtility.SetDirty(self);
                                }
                            }
                            else {
                                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                                UnityEditor_1.EditorGUILayout.ObjectField(label, oldValue, UnityEngine_1.Object, false);
                                UnityEditor_1.EditorGUI.EndDisabledGroup();
                            }
                            break;
                        }
                        default: {
                            if (!drawer_1.DefaultPropertyDrawer.draw(slot.type, target, slot, label, editablePE)) {
                                UnityEditor_1.EditorGUILayout.LabelField(label);
                                UnityEditor_1.EditorGUILayout.HelpBox("no draw operation for this type", UnityEditor_1.MessageType.Warning);
                            }
                            break;
                        }
                    }
                }
                else {
                    UnityEditor_1.EditorGUILayout.LabelField(label);
                    UnityEditor_1.EditorGUILayout.HelpBox("unsupported type", UnityEditor_1.MessageType.Warning);
                }
            }
        });
    }
}
exports.EditorUtil = EditorUtil;
//# sourceMappingURL=editor_decorators.js.map