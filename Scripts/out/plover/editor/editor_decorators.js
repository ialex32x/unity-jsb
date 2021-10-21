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
                    let d = drawer_1.DefaultPropertyDrawers[slot.type];
                    if (typeof d !== "undefined") {
                        let propertyKey = slot.propertyKey;
                        let oldValue = target[propertyKey];
                        if (oldValue instanceof Array) {
                            let length = oldValue.length;
                            for (let i = 0; i < length; i++) {
                                let newValue = d.draw(oldValue[i], slot, label, editablePE);
                                if (editablePE && oldValue[i] != newValue) {
                                    oldValue[i] = newValue;
                                    UnityEditor_1.EditorUtility.SetDirty(target);
                                }
                            }
                            if (editablePE) {
                                if (UnityEngine_1.GUILayout.Button("Add Element")) {
                                    oldValue.push(null);
                                    UnityEditor_1.EditorUtility.SetDirty(target);
                                }
                            }
                        }
                        else {
                            let newValue = d.draw(oldValue, slot, label, editablePE);
                            if (editablePE && oldValue != newValue) {
                                target[propertyKey] = newValue;
                                UnityEditor_1.EditorUtility.SetDirty(target);
                            }
                        }
                        return true;
                    }
                    else {
                        UnityEditor_1.EditorGUILayout.LabelField(label);
                        UnityEditor_1.EditorGUILayout.HelpBox("no draw operation for this type", UnityEditor_1.MessageType.Warning);
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