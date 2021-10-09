import { ByteBuffer } from "QuickJS.IO";
import { JSScriptProperties } from "QuickJS.Unity";
import { DefaultSerializer, GetLatestSerializer, GetSerializer, ISerializer } from "./serialize";

let Symbol_PropertiesTouched = Symbol.for("PropertiesTouched");
let Symbol_MemberFuncs = Symbol.for("MemberFuncs");
let Symbol_SerializedFields = Symbol.for("SerializedFields");

export interface FunctionMetaInfo {

}

export interface ClassMetaInfo {

}

export type PropertyTypeID = "bool" | "float" | "double" | "string" | "object" | "int" | "uint" | "Vector2" | "Vector3" | "Vector4" | "Rect" | "Quaternion";

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
    static forEach(target: any, cb: (propertyKey: string, slot: PropertyMetaInfo) => void) {
        let slots: {} = target[Symbol_SerializedFields];
        if (typeof slots !== "undefined") {
            for (let propertyKey in slots) {
                cb(propertyKey, slots[propertyKey]);
            }
        }
    }

    // 当不需要默认行为时, 调用此函数将序列化状态标记为已完成, 以便跳过默认的 serialize/deserialize 行为
    static markAsReady(target: any) {
        target[Symbol_PropertiesTouched] = true;
    }

    static serialize(target: any, ps: JSScriptProperties, buffer: ByteBuffer) {
        this.markAsReady(target);
        let impl = GetLatestSerializer();
        if (typeof impl === "object") {
            ps.dataFormat = impl.dataFormat;
            this.forEach(target, (propertyKey, slot) => {
                if (slot.serializable) {
                    let value = target[propertyKey];

                    switch (slot.type) {
                        case "object": {
                            ps.SetObject(slot.name, value);
                            break;
                        }
                        default: {
                            let s: ISerializer = impl.types[slot.type];
                            if (typeof s === "object") {
                                buffer.WriteString(slot.name);
                                buffer.WriteByte(s.typeid);
                                s.serialize(buffer, value);
                            }
                            break;
                        }
                    }
                }
            });
        }
    }

    static deserialize(target: any, ps: JSScriptProperties, buffer: ByteBuffer) {
        this.markAsReady(target);
        let slots: {} = target[Symbol_SerializedFields];
        if (typeof slots !== "undefined") {
            let slotByName = {};
            let dataFormat = ps.dataFormat || 0;
            let impl = GetSerializer(dataFormat);
            if (typeof impl === "object") {
                for (let propertyKey in slots) {
                    let slot: PropertyMetaInfo = slots[propertyKey];
                    if (slot.serializable) {
                        switch (slot.type) {
                            case "object": {
                                target[propertyKey] = ps.GetObject(slot.name);
                                break;
                            }
                            default: {
                                slotByName[slot.name] = slot;
                                target[slot.propertyKey] = impl.types[slot.type].defaultValue;
                                break;
                            }
                        }
                    }
                }

                while (buffer.readableBytes > 0) {
                    let name = buffer.ReadString();
                    let typeid = buffer.ReadUByte();
                    let s = impl.typeids[typeid];
                    let slot_value = s.deserilize(buffer);

                    let slot: PropertyMetaInfo = slotByName[name];
                    if (slot) {
                        console.assert(impl.types[slot.type].typeid == s.typeid);
                        target[slot.propertyKey] = slot_value;
                    }
                }
            } else {
                console.error("no serializer for dataFormat", dataFormat);
            }
        }
    }
}
