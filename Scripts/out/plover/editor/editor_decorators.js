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
    return class_decorators_1.ScriptType(meta);
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
    static draw(target) {
        class_decorators_1.SerializationUtil.forEach(target, (slots, propertyKey) => {
            let slot = slots[propertyKey];
            if (slot.visible) {
                let label = slot.label || propertyKey;
                let editablePE = slot.editable && (!slot.editorOnly || !UnityEditor_1.EditorApplication.isPlaying);
                if (typeof slot.type === "string") {
                    switch (slot.type) {
                        case "int": {
                            let oldValue = target[propertyKey];
                            if (editablePE) {
                                let newValue = UnityEditor_1.EditorGUILayout.IntField(label, oldValue);
                                if (newValue != oldValue) {
                                    target[propertyKey] = newValue;
                                    UnityEditor_1.EditorUtility.SetDirty(target);
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
                            let oldValue = target[propertyKey];
                            if (editablePE) {
                                let newValue = UnityEditor_1.EditorGUILayout.FloatField(label, oldValue);
                                if (newValue != oldValue) {
                                    target[propertyKey] = newValue;
                                    UnityEditor_1.EditorUtility.SetDirty(target);
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
                            let oldValue = target[propertyKey];
                            if (typeof oldValue !== "string") {
                                oldValue = "" + oldValue;
                            }
                            if (editablePE) {
                                let newValue = UnityEditor_1.EditorGUILayout.TextField(label, oldValue);
                                if (newValue != oldValue) {
                                    target[propertyKey] = newValue;
                                    UnityEditor_1.EditorUtility.SetDirty(target);
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
                            let oldValue = target[propertyKey];
                            if (typeof oldValue !== "object") {
                                oldValue = null;
                            }
                            if (editablePE) {
                                let allowSceneObjects = slot.extra && slot.extra.allowSceneObjects;
                                let newValue = UnityEditor_1.EditorGUILayout.ObjectField(label, oldValue, slot.extra && slot.extra.type || UnityEngine_1.Object, typeof allowSceneObjects === "boolean" ? allowSceneObjects : true);
                                if (newValue != oldValue) {
                                    target[propertyKey] = newValue;
                                    UnityEditor_1.EditorUtility.SetDirty(target);
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
                            if (typeof slot.type === "string") {
                                let d = drawer_1.DefaultPropertyDrawers[slot.type];
                                if (typeof d !== "undefined") {
                                    d.draw(target, slot, label, editablePE);
                                    return true;
                                }
                                else {
                                    UnityEditor_1.EditorGUILayout.LabelField(label);
                                    UnityEditor_1.EditorGUILayout.HelpBox("no draw operation for this type", UnityEditor_1.MessageType.Warning);
                                }
                            }
                            else {
                                //TODO draw nested value
                                UnityEditor_1.EditorGUILayout.LabelField(label);
                                UnityEditor_1.EditorGUILayout.HelpBox("not implemented for nested values", UnityEditor_1.MessageType.Warning);
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