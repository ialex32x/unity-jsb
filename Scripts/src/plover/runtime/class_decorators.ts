import { ByteBuffer } from "QuickJS.IO";
import { JSScriptProperties } from "QuickJS.Unity";
import { DefaultSerializer } from "./serialize";

let Symbol_PropertiesTouched = Symbol.for("PropertiesTouched");
let Symbol_MemberFuncs = Symbol.for("MemberFuncs");
let Symbol_SerializedFields = Symbol.for("SerializedFields");

export interface FunctionMetaInfo {

}

export interface ClassMetaInfo {

}

export type PropertyTypeID = "bool" | "float" | "double" | "string" | "object" | "int" | "Vector3" | "Quaternion";

export interface WeakPropertyMetaInfo {
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

    label?: string;

    tooltip?: string;

    extra?: any;

    /**
     * UGUI, 自动绑定界面组件
     */
    bind?: {
        name?: string;
        widget?: Function;
    };
}

export interface PropertyMetaInfo extends WeakPropertyMetaInfo {
    type: PropertyTypeID;
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

export function ScriptInteger(meta?: WeakPropertyMetaInfo) {
    let meta_t = <PropertyMetaInfo>meta;
    if (typeof meta_t === "undefined") {
        meta_t = { type: "int" };
    } else {
        meta_t.type = "int";
    }
    return ScriptProperty(meta_t);
}

export function ScriptNumber(meta?: WeakPropertyMetaInfo) {
    let meta_t = <PropertyMetaInfo>meta;
    if (typeof meta_t === "undefined") {
        meta_t = { type: "float" };
    } else {
        meta_t.type = "float";
    }
    return ScriptProperty(meta_t);
}

export function ScriptString(meta?: WeakPropertyMetaInfo) {
    let meta_t = <PropertyMetaInfo>meta;
    if (typeof meta_t === "undefined") {
        meta_t = { type: "string" };
    } else {
        meta_t.type = "string";
    }
    return ScriptProperty(meta_t);
}

export function ScriptObject(meta?: WeakPropertyMetaInfo) {
    let meta_t = <PropertyMetaInfo>meta;
    if (typeof meta_t === "undefined") {
        meta_t = { type: "object" };
    } else {
        meta_t.type = "object";
    }
    return ScriptProperty(meta_t);
}

export function ScriptProperty(meta?: PropertyMetaInfo) {
    return function (target: any, propertyKey: string) {
        let slots: { [k: string]: PropertyMetaInfo } = target[Symbol_SerializedFields];
        if (typeof slots === "undefined") {
            slots = target[Symbol_SerializedFields] = {};
        }

        let slot = slots[propertyKey] = meta || { type: "object" };

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

export function ScriptFunction(meta?: any) {
    return function (target: any, propertyKey: string) {
        let funcMap = target[Symbol_MemberFuncs];
        if (typeof funcMap === "undefined") {
            funcMap = target[Symbol_MemberFuncs] = {};
        }

        funcMap[propertyKey] = propertyKey;
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
                switch (slot.type) {
                    case "int": {
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
                    default: {
                        buffer.WriteString(slot.name);
                        DefaultSerializer.serialize(slot.type, buffer, value);
                        break;
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
                        switch (slot.type) {
                            case "int": {
                                target[propertyKey] = ps.GetInteger(slot.name);
                                break;
                            }
                            case "float": {
                                target[propertyKey] = ps.GetNumber(slot.name);
                                break;
                            }
                            case "string": {
                                target[propertyKey] = ps.GetString(slot.name);
                                break;
                            }
                            case "object": {
                                target[propertyKey] = ps.GetObject(slot.name);
                                break;
                            }
                            default: {
                                slotByName[slot.name] = slot;
                                break;
                            }
                        }
                    }
                    // console.log("deserialize", propertyKey, value);
                }
            }

            while (buffer.readableBytes > 0) {
                let name = buffer.ReadString();
                let slot: PropertyMetaInfo = slotByName[name];
                if (slot) {
                    target[slot.propertyKey] = DefaultSerializer.deserilize(slot.type, buffer);
                } else {
                    let size = buffer.ReadInt32();
                    buffer.ReadBytes(size);
                    target[slot.propertyKey] = null;
                }
            }
        }
    }
}