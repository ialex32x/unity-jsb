import { ByteBuffer } from "QuickJS.IO";
import { JSScriptProperties } from "QuickJS.Unity";
import { Editor, EditorApplication, EditorGUI, EditorGUILayout, EditorUtility, MessageType } from "UnityEditor";
import { Object, Vector3 } from "UnityEngine";
import { SerializationUtil } from "../runtime/class_decorators";
import { DefaultPropertyDrawer } from "./drawer";

let Symbol_CustomEditor = Symbol.for("CustomEditor");

export interface EditorWindowMetaInfo {

}

export function ScriptEditor(forType: any) {
    return function (editorType: any) {
        forType.prototype[Symbol_CustomEditor] = editorType;
        return editorType;
    }
}

export function ScriptEditorWindow(meta?: EditorWindowMetaInfo) {
    return function (target: any) {
        return target;
    }
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
        SerializationUtil.forEach(target, (propertyKey, slot) => {
            if (slot.visible) {
                let label = slot.label || propertyKey;
                let editablePE = slot.editable && (!slot.editorOnly || !EditorApplication.isPlaying);

                if (typeof slot.type === "string") {
                    switch (slot.type) {
                        case "int": {
                            let oldValue: number = target[propertyKey];
                            if (editablePE) {
                                let newValue = EditorGUILayout.IntField(label, oldValue);
                                if (newValue != oldValue) {
                                    target[propertyKey] = newValue;
                                    EditorUtility.SetDirty(target);
                                }
                            } else {
                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.IntField(label, oldValue);
                                EditorGUI.EndDisabledGroup();
                            }
                            break;
                        }
                        case "float": {
                            let oldValue: number = target[propertyKey];
                            if (editablePE) {
                                let newValue = EditorGUILayout.FloatField(label, oldValue);
                                if (newValue != oldValue) {
                                    target[propertyKey] = newValue;
                                    EditorUtility.SetDirty(target);
                                }
                            } else {
                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.FloatField(label, oldValue);
                                EditorGUI.EndDisabledGroup();
                            }
                            break;
                        }
                        case "string": {
                            let oldValue: string = target[propertyKey];
                            if (typeof oldValue !== "string") {
                                oldValue = "" + oldValue;
                            }
                            if (editablePE) {
                                let newValue = EditorGUILayout.TextField(label, oldValue);
                                if (newValue != oldValue) {
                                    target[propertyKey] = newValue;
                                    EditorUtility.SetDirty(target);
                                }
                            } else {
                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.TextField(label, oldValue);
                                EditorGUI.EndDisabledGroup();
                            }
                            break;
                        }
                        case "object": {
                            let oldValue: Object = target[propertyKey];
                            if (typeof oldValue !== "object") {
                                oldValue = null;
                            }
                            if (editablePE) {
                                let allowSceneObjects = slot.extra && slot.extra.allowSceneObjects;
                                let newValue = EditorGUILayout.ObjectField(label, oldValue,
                                    slot.extra && slot.extra.type || Object,
                                    typeof allowSceneObjects === "boolean" ? allowSceneObjects : true);

                                if (newValue != oldValue) {
                                    target[propertyKey] = newValue;
                                    EditorUtility.SetDirty(target);
                                }
                            } else {
                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.ObjectField(label, oldValue, Object, false);
                                EditorGUI.EndDisabledGroup();
                            }
                            break;
                        }
                        default: {
                            if (!DefaultPropertyDrawer.draw(slot.type, target, slot, label, editablePE)) {
                                EditorGUILayout.LabelField(label);
                                EditorGUILayout.HelpBox("no draw operation for this type", MessageType.Warning);
                            }
                            break;
                        }
                    }
                } else {
                    EditorGUILayout.LabelField(label);
                    EditorGUILayout.HelpBox("unsupported type", MessageType.Warning);
                }
            }
        });
    }
}
