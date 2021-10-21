import { Editor, EditorApplication, EditorGUI, EditorGUILayout, EditorUtility, MessageType } from "UnityEditor";
import { GUILayout, Object } from "UnityEngine";
import { ClassMetaInfo, ScriptType, SerializationUtil } from "../runtime/class_decorators";
import { DefaultPropertyDrawers } from "./drawer";

let Symbol_CustomEditor = Symbol.for("CustomEditor");

export interface EditorWindowMetaInfo extends ClassMetaInfo {

}

export function ScriptEditor(forType: any) {
    return function (editorType: any) {
        forType.prototype[Symbol_CustomEditor] = editorType;
        return editorType;
    }
}

export function ScriptEditorWindow(meta?: EditorWindowMetaInfo) {
    return ScriptType(meta);
}

export class DefaultEditor extends Editor {
    OnInspectorGUI() {
        EditorUtil.draw(this.target);
    }
}

export class EditorUtil {
    static getCustomEditor(forType: any) {
        return forType[Symbol_CustomEditor] || DefaultEditor;
    }

    /**
     * 默认编辑器绘制行为
     */
    static draw(target: any) {
        SerializationUtil.forEach(target, (slots, propertyKey) => {
            let slot = slots[propertyKey];
            if (slot.visible) {
                let label = slot.label || propertyKey;
                let editablePE = slot.editable && (!slot.editorOnly || !EditorApplication.isPlaying);

                if (typeof slot.type === "string") {
                    let d = DefaultPropertyDrawers[slot.type];
                    if (typeof d !== "undefined") {
                        let propertyKey = slot.propertyKey;
                        let oldValue = target[propertyKey];
                        if (oldValue instanceof Array) {
                            let length = oldValue.length;
                            for (let i = 0; i < length; i++) {
                                let newValue = d.draw(oldValue[i], slot, label, editablePE);
                                if (editablePE && oldValue[i] != newValue) {
                                    oldValue[i] = newValue;
                                    EditorUtility.SetDirty(target);
                                }
                            }
                            if (editablePE) {
                                if (GUILayout.Button("Add Element")) {
                                    oldValue.push(null);
                                    EditorUtility.SetDirty(target);
                                }
                            }
                        } else {
                            let newValue = d.draw(oldValue, slot, label, editablePE);
                            if (editablePE && oldValue != newValue) {
                                target[propertyKey] = newValue;
                                EditorUtility.SetDirty(target);
                            }
                        }
                        return true;
                    } else {
                        EditorGUILayout.LabelField(label);
                        EditorGUILayout.HelpBox("no draw operation for this type", MessageType.Warning);
                    }
                } else {
                    EditorGUILayout.LabelField(label);
                    EditorGUILayout.HelpBox("unsupported type", MessageType.Warning);
                }
            }
        });
    }
}
