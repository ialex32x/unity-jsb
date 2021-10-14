import { ByteBuffer } from "QuickJS.IO";
import { JSScriptProperties } from "QuickJS.Unity";
import { Object as UObject } from "UnityEngine";
import { GetLatestSerializer, GetSerializer, ISerializer, SerializerImpl } from "./serialize";

let Symbol_PropertiesTouched = Symbol.for("PropertiesTouched");
let Symbol_MemberFuncs = Symbol.for("MemberFuncs");
let Symbol_SerializedFields = Symbol.for("SerializedFields");

export interface FunctionMetaInfo {

}

export interface ClassMetaInfo {

}

export type PropertyTypeID = "bool" | "float" | "double" | "string" | "object" | "int" | "uint" | "Vector2" | "Vector3" | "Vector4" | "Rect" | "Quaternion" | Function;

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

export function ScriptSerializable(meta?: any) {
    return ScriptType(meta);
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
    static forEach(target: any, cb: (slots: { [key: string]: PropertyMetaInfo }, propertyKey: string) => void) {
        let slots: {} = target[Symbol_SerializedFields];
        if (typeof slots !== "undefined") {
            for (let propertyKey in slots) {
                cb(slots, propertyKey);
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
        console.assert(typeof ps === "object");
        if (typeof impl === "object") {
            ps.dataFormat = impl.dataFormat;

            let slots: {} = target[Symbol_SerializedFields];
            if (typeof slots !== "undefined") {
                this._serializeField(target, slots, ps, buffer, impl);
            }
        }
    }

    private static _serializeField(target: any, slots: { [key: string]: PropertyMetaInfo }, ps: JSScriptProperties, parent: ByteBuffer, impl: SerializerImpl) {
        for (let propertyKey in slots) {
            let slot = slots[propertyKey];
            if (slot.serializable) {
                let value = target && target[propertyKey];
                if (typeof slot.type === "string") {
                    let s: ISerializer = impl.types[slot.type];
                    if (typeof s === "object") {
                        parent.WriteString(slot.name);
                        parent.WriteByte(s.typeid);
                        s.serialize(ps, parent, value);
                    }
                } else {
                    // typeof slot.type === "function" (a constructor)
                    // nested value

                    if (typeof value === "object") {
                        if (value instanceof UObject) {
                            let s: ISerializer = impl.types["object"];
                            parent.WriteString(slot.name);
                            parent.WriteByte(s.typeid);
                            s.serialize(ps, parent, value);
                        } else {
                            let fieldSlots = slot.type.prototype[Symbol_SerializedFields];
                            if (typeof fieldSlots !== "undefined") {
                                let fieldBuffer = ps.NewSection();
                                this._serializeField(value, fieldSlots, ps, fieldBuffer, impl);
                                parent.WriteString(slot.name);
                                parent.WriteByte(0);
                                parent.WriteInt32(fieldBuffer.readableBytes);
                                parent.WriteBytes(fieldBuffer);
                            } else {
                                console.error("no serialization info on field", slot.name);
                            }
                        }
                    } else {
                        // skip invalid value
                    }
                }
            }
        }
    }

    static deserialize(target: any, ps: JSScriptProperties, buffer: ByteBuffer) {
        this.markAsReady(target);
        let slots: {} = target[Symbol_SerializedFields];
        if (typeof slots !== "undefined") {
            let dataFormat = ps.dataFormat || 0;
            let impl = GetSerializer(dataFormat);

            if (typeof impl === "object") {
                this._deserializeField(target, slots, ps, buffer, impl);
            } else {
                if (ps.GenericCount > 0 && ps.dataFormat >= 0) {
                    console.error("no serializer for dataFormat", dataFormat);
                }
            }
        }
    }

    private static _deserializeField(target: any, slots: { [key: string]: PropertyMetaInfo }, ps: JSScriptProperties, parent: ByteBuffer, impl: SerializerImpl) {
        let slotByName = {};
        for (let propertyKey in slots) {
            let slot: PropertyMetaInfo = slots[propertyKey];
            if (slot.serializable) {
                slotByName[slot.name] = slot;

                if (typeof slot.type === "string") {
                    let defaultValue = impl.types[slot.type].defaultValue;
                    if (typeof defaultValue === "function") {
                        defaultValue = defaultValue();
                    }
                    target[slot.propertyKey] = defaultValue;
                } else {
                    target[slot.propertyKey] = null;
                }
            }
        }

        while (parent.readableBytes > 0) {
            let name = parent.ReadString();
            let typeid = parent.ReadUByte();
            let slot: PropertyMetaInfo = slotByName[name];

            if (typeid > 0) {
                let s = impl.typeids[typeid];
                let slot_value = s.deserilize(ps, parent);

                if (slot) {
                    if (typeof slot.type === "string") {
                        console.assert(typeid == impl.types[slot.type].typeid, "slot type mismatch");
                    } else {
                        if (typeof slot_value === "object") {
                            console.assert(slot_value instanceof slot.type, "slot type mismatch");
                        }
                    }
                    target[slot.propertyKey] = slot_value;
                } else {
                    console.warn("failed to read slot", name);
                }
            } else {
                let size = parent.ReadInt32();
                let fieldBuffer = ps.ReadSection(parent, size);
                if (slot && typeof slot.type === "function") {
                    let fieldSlots = slot.type.prototype[Symbol_SerializedFields];
                    if (fieldSlots) {
                        let fieldValue = Object.create(slot.type);
                        this._deserializeField(fieldValue, fieldSlots, ps, fieldBuffer, impl);
                        target[slot.propertyKey] = fieldValue;
                    } else {
                        console.error("no serialization info on field", slot.name);
                    }
                }
            }
        }
    }
}
