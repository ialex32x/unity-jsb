import { ByteBuffer } from "QuickJS.IO";
import { JSScriptProperties } from "QuickJS.Unity";
import { Editor, EditorApplication, EditorGUI, EditorGUILayout, EditorUtility, MessageType } from "UnityEditor";
import { Object, Vector3 } from "UnityEngine";
import { ValueTypeSerializer } from "./serialize";

let Symbol_SerializedFields = Symbol.for("SerializedFields");
let Symbol_PropertiesTouched = Symbol.for("PropertiesTouched");
let Symbol_CustomEditor = Symbol.for("CustomEditor");

type PropertyTypeID = "integer" | "float" | "string" | "object";

export interface PropertyMetaInfo {
    /**
     * slot name in property table
     */
    name?: string;

    propertyKey?: string;

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

    type?: PropertyTypeID | ValueTypeSerializer;

    label?: string;

    tooltip?: string;

    extra?: any;
}

export interface ClassMetaInfo {

}

export function ScriptAsset(meta?: any) {
    return ScriptType(meta);
}

// expose this script class type to JSBehaviour, so you can put it on a prefab gameObject
export function ScriptType(meta?: ClassMetaInfo) {
    return function (target: any) {
        let OnBeforeSerialize: Function = target.prototype["OnBeforeSerialize"];
        target.prototype["OnBeforeSerialize"] = function (ps, buffer) {
            this[Symbol_PropertiesTouched] = false;
            if (typeof OnBeforeSerialize === "function") {
                OnBeforeSerialize.call(this, ps, buffer);
            }
            if (!this[Symbol_PropertiesTouched]) {
                SerializationUtil.serialize(this, ps, buffer);
            }
        }

        let OnAfterDeserialize: Function = target.prototype["OnAfterDeserialize"];
        target.prototype["OnAfterDeserialize"] = function (ps, buffer) {
            this[Symbol_PropertiesTouched] = false;
            if (typeof OnAfterDeserialize === "function") {
                OnAfterDeserialize.call(this, ps, buffer);
            }
            if (!this[Symbol_PropertiesTouched]) {
                SerializationUtil.deserialize(this, ps, buffer);
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

        slot.propertyKey = propertyKey;
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
                let editablePE = slot.editable && (!slot.editorOnly || !EditorApplication.isPlaying);

                if (typeof slot.type === "object") {
                    if (typeof slot.type.draw === "function") {
                        slot.type.draw(target, slot, label, editablePE);
                    } else {
                        EditorGUILayout.LabelField(label);
                        EditorGUILayout.HelpBox("no draw operation for this type", MessageType.Warning);
                    }
                } else if (typeof slot.type === "string") {
                    switch (slot.type) {
                        case "integer": {
                            let oldValue: number = self[propertyKey];
                            if (editablePE) {
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
                            break;
                        }
                        case "string": {
                            let oldValue: string = self[propertyKey];
                            if (typeof oldValue !== "string") {
                                oldValue = "" + oldValue;
                            }
                            if (editablePE) {
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
                            if (editablePE) {
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
                } else {
                    EditorGUILayout.LabelField(label);
                    EditorGUILayout.HelpBox("unsupported type", MessageType.Warning);
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

    // 当不需要默认行为时, 调用此函数将序列化状态标记为已完成, 以便跳过默认的 serialize/deserialize 行为
    static markAsReady(target: any) {
        target[Symbol_PropertiesTouched] = true;
    }

    static serialize(target: any, ps: JSScriptProperties, buffer: ByteBuffer) {
        this.markAsReady(target);
        this.forEach(target, ps, (propertyKey, slot, self, extra: JSScriptProperties) => {
            if (slot.serializable) {
                let value = self[propertyKey];

                // console.log("serializing", propertyKey, value);
                if (typeof slot.type === "object") {
                    buffer.WriteString(slot.name);
                    slot.type.serialize(buffer, value);
                } else {
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
            }
        });
    }

    static deserialize(target: any, ps: JSScriptProperties, buffer: ByteBuffer) {
        this.markAsReady(target);
        let slots: {} = target[Symbol_SerializedFields];
        if (typeof slots !== "undefined") {
            let slotByName = {};
            for (let propertyKey in slots) {
                let slot: PropertyMetaInfo = slots[propertyKey];
                if (slot.serializable) {
                    if (typeof slot.type === "object") {
                        slotByName[slot.name] = slot;
                    } else {
                        let value = null;
                        switch (slot.type) {
                            case "integer": {
                                value = ps.GetInteger(slot.name);
                                break;
                            }
                            case "float": {
                                value = ps.GetNumber(slot.name);
                                break;
                            }
                            case "string": {
                                value = ps.GetString(slot.name);
                                break;
                            }
                            case "object": {
                                value = ps.GetObject(slot.name);
                                break;
                            }
                        }
                        target[propertyKey] = value;
                    }
                    // console.log("deserialize", propertyKey, value);
                }
            }

            while (buffer.readableBytes > 0) {
                let name = buffer.ReadString();
                let slot: PropertyMetaInfo = slotByName[name];
                if (slot) {
                    target[slot.propertyKey] = (<ValueTypeSerializer>slot.type).deserilize(buffer);
                } else {
                    let size = buffer.ReadInt32();
                    buffer.ReadBytes(size);
                }
            }
        }
    }
}
