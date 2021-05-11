import { JSBehaviourProperties } from "QuickJS.Unity";
import { Editor, EditorApplication, EditorGUI, EditorGUILayout, EditorUtility } from "UnityEditor";
import { Object } from "UnityEngine";

let Symbol_SerializedFields = Symbol.for("SerializedFields");
let Symbol_PropertiesTouched = Symbol.for("PropertiesTouched");
let Symbol_CustomEditor = Symbol.for("CustomEditor");

type PropertyTypeID = "integer" | "float" | "string" | "object";

export interface PropertyMetaInfo {
    /**
     * slot name in property table
     */
    name?: string;

    /**
     * (默认编辑器行为中) 是否可见
     */
    visible?: boolean;

    /**
     * (默认编辑器行为中) 是否可以编辑
     */
    editable?: boolean;

    /**
     * 是否仅编辑器状态可编辑
     */
    editorOnly?: boolean;

    /**
     * 是否序列化
     */
    serializable?: boolean;

    type?: PropertyTypeID;

    label?: string;

    tooltip?: string;

    extra?: any;
}

export interface ClassMetaInfo {

}

// expose this script class type to JSBehaviour, so you can put it on a prefab gameObject
export function ScriptType(meta?: ClassMetaInfo) {
    return function (target: any) {
        let OnBeforeSerialize: Function = target.prototype["OnBeforeSerialize"];
        target.prototype["OnBeforeSerialize"] = function (ps) {
            this[Symbol_PropertiesTouched] = false;
            if (typeof OnBeforeSerialize === "function") {
                OnBeforeSerialize.call(this, ps);
            }
            if (!this[Symbol_PropertiesTouched]) {
                SerializationUtil.serialize(this, ps);
            }
        }

        let OnAfterDeserialize: Function = target.prototype["OnAfterDeserialize"];
        target.prototype["OnAfterDeserialize"] = function (ps) {
            this[Symbol_PropertiesTouched] = false;
            if (typeof OnAfterDeserialize === "function") {
                OnAfterDeserialize.call(this, ps);
            }
            if (!this[Symbol_PropertiesTouched]) {
                SerializationUtil.deserialize(this, ps);
            }
        }

        return target;
    }
}

export function ScriptEditor(forType: any) {
    return function (editorType: any) {
        forType.prototype[Symbol_CustomEditor] = editorType;
        return editorType;
    }
}

export function ScriptInteger(meta?: PropertyMetaInfo) {
    if (typeof meta === "undefined") {
        meta = { type: "integer" };
    } else {
        meta.type = "integer";
    }
    return ScriptProperty(meta);
}

export function ScriptNumber(meta?: PropertyMetaInfo) {
    if (typeof meta === "undefined") {
        meta = { type: "float" };
    } else {
        meta.type = "float";
    }
    return ScriptProperty(meta);
}

export function ScriptString(meta?: PropertyMetaInfo) {
    if (typeof meta === "undefined") {
        meta = { type: "string" };
    } else {
        meta.type = "string";
    }
    return ScriptProperty(meta);
}

export function ScriptObject(meta?: PropertyMetaInfo) {
    if (typeof meta === "undefined") {
        meta = { type: "object" };
    } else {
        meta.type = "object";
    }
    return ScriptProperty(meta);
}

export function ScriptProperty(meta?: PropertyMetaInfo) {
    return function (target: any, propertyKey: string) {
        let slots: { [k: string]: PropertyMetaInfo } = target[Symbol_SerializedFields];
        if (typeof slots === "undefined") {
            slots = target[Symbol_SerializedFields] = {};
        }

        let slot = slots[propertyKey] = meta || {};

        if (typeof slot.serializable !== "boolean") {
            slot.serializable = true;
        }

        if (typeof slot.editable !== "boolean") {
            slot.editable = true;
        }

        if (typeof slot.visible !== "boolean") {
            slot.visible = true;
        }

        if (typeof slot.name !== "string") {
            slot.name = propertyKey;
        }
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
    static draw(target: any, extra?: any) {
        SerializationUtil.forEach(target, extra, (propertyKey, slot, self, extra) => {
            if (slot.visible) {
                let label = slot.label || propertyKey;
                let editablePE = !slot.editorOnly || !EditorApplication.isPlaying;

                switch (slot.type) {
                    case "integer": {
                        let oldValue: number = self[propertyKey];
                        if (slot.editable && editablePE) {
                            let newValue = EditorGUILayout.IntField(label, oldValue);
                            if (newValue != oldValue) {
                                self[propertyKey] = newValue;
                                EditorUtility.SetDirty(self);
                            }
                        } else {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.IntField(label, oldValue);
                            EditorGUI.EndDisabledGroup();
                        }
                        break;
                    }
                    case "float": {
                        let oldValue: number = self[propertyKey];
                        if (slot.editable && editablePE) {
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
                        break;
                    }
                    case "string": {
                        let oldValue: string = self[propertyKey];
                        if (typeof oldValue !== "string") {
                            oldValue = "" + oldValue;
                        }
                        if (slot.editable && editablePE) {
                            let newValue = EditorGUILayout.TextField(label, oldValue);
                            if (newValue != oldValue) {
                                self[propertyKey] = newValue;
                                EditorUtility.SetDirty(self);
                            }
                        } else {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.TextField(label, oldValue);
                            EditorGUI.EndDisabledGroup();
                        }
                        break;
                    }
                    case "object": {
                        let oldValue: Object = self[propertyKey];
                        if (typeof oldValue !== "object") {
                            oldValue = null;
                        }
                        if (slot.editable && editablePE) {
                            let allowSceneObjects = slot.extra && slot.extra.allowSceneObjects;
                            let newValue = EditorGUILayout.ObjectField(label, oldValue,
                                slot.extra && slot.extra.type || Object,
                                typeof allowSceneObjects === "boolean" ? allowSceneObjects : true);

                            if (newValue != oldValue) {
                                self[propertyKey] = newValue;
                                EditorUtility.SetDirty(self);
                            }
                        } else {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.ObjectField(label, oldValue, Object, false);
                            EditorGUI.EndDisabledGroup();
                        }
                        break;
                    }
                }
            }
        });
    }
}

export class SerializationUtil {
    static forEach(target: any, extra: any, cb: (propertyKey: string, slot: PropertyMetaInfo, target: any, extra: any) => void) {
        let slots: {} = target[Symbol_SerializedFields];
        if (typeof slots !== "undefined") {
            for (let propertyKey in slots) {
                cb(propertyKey, slots[propertyKey], target, extra);
            }
        }
    }

    static serialize(target: any, ps: JSBehaviourProperties) {
        target[Symbol_PropertiesTouched] = true;
        this.forEach(target, ps, (propertyKey, slot, self, extra: JSBehaviourProperties) => {
            if (slot.serializable) {
                let value = self[propertyKey];

                // console.log("serializing", slot.propertyKey, value);
                switch (slot.type) {
                    case "integer": {
                        extra.SetInteger(slot.name, typeof value === "number" ? value : 0);
                        break;
                    }
                    case "float": {
                        extra.SetNumber(slot.name, typeof value === "number" ? value : 0);
                        break;
                    }
                    case "string": {
                        extra.SetString(slot.name, value);
                        break;
                    }
                    case "object": {
                        extra.SetObject(slot.name, value);
                        break;
                    }
                }
            }
        });
    }

    static deserialize(target: any, ps: JSBehaviourProperties) {
        target[Symbol_PropertiesTouched] = true;
        this.forEach(target, ps, (propertyKey, slot, self, extra: JSBehaviourProperties) => {
            if (slot.serializable) {
                let value = null;

                switch (slot.type) {
                    case "integer": {
                        value = extra.GetInteger(slot.name);
                        break;
                    }
                    case "float": {
                        value = extra.GetNumber(slot.name);
                        break;
                    }
                    case "string": {
                        value = extra.GetString(slot.name);
                        break;
                    }
                    case "object": {
                        value = extra.GetObject(slot.name);
                        break;
                    }
                }
                self[propertyKey] = value;
            }
            // console.log("deserialize", slot.propertyKey, value);
        });
    }
}
