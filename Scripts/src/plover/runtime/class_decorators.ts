import { ByteBuffer } from "QuickJS.IO";
import { JSSerializationContext } from "QuickJS.Unity";
import { Object as UObject } from "UnityEngine";
import { SerializedTypeID, GetLatestSerializer, GetSerializer, IPrimitiveSerializer, SerializationContext, PrimitiveSerializerImpl } from "./serialize";

let Symbol_PropertiesTouched = Symbol.for("PropertiesTouched");
let Symbol_MemberFuncs = Symbol.for("MemberFuncs");
let Symbol_SerializedFields = Symbol.for("SerializedFields");

export interface FunctionMetaInfo {

}

export interface ClassMetaInfo {

}

export type PropertyTypeID = "bool" | "float" | "double" | "string" | "object" | "int" | "uint" | "Uint8ArrayBuffer" | "Vector2" | "Vector3" | "Vector4" | "Rect" | "Quaternion" | "json" | Function;

export type PropertyLayout = "plain" | "array";

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
    layout?: PropertyLayout;
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

    static serialize(target: any, ps: JSSerializationContext) {
        this.markAsReady(target);
        let impl = GetLatestSerializer();
        console.assert(typeof ps === "object");
        if (typeof impl === "object") {
            ps.dataFormat = impl.dataFormat;

            let slots: {} = target[Symbol_SerializedFields];
            if (typeof slots !== "undefined") {
                let buffer = this._serializeObject({ impl: impl, ps: ps }, target, slots);
                ps.Flush(buffer);
            }
        }
    }

    private static _serializeValue(context: SerializationContext, slot: PropertyMetaInfo, value: any, buffer: ByteBuffer) {
        let slotType: PropertyTypeID = slot.type;
        let isArray = value instanceof Array;

        if (typeof value === "object") {
            if (value instanceof UObject) {
                slotType = "object";
            }
        }
        if (typeof slotType === "string") {
            // primitive serializer impl
            let s: IPrimitiveSerializer = context.impl.types[slotType];

            if (typeof s === "object") {
                if (isArray) {
                    let section = this._serializePrimitiveArray(context, s, value);

                    buffer.WriteByte(SerializedTypeID.Array);
                    buffer.WriteByte(s.typeid);
                    buffer.WriteInt32(section.readableBytes);
                    buffer.WriteBytes(section);
                } else {
                    buffer.WriteByte(s.typeid);
                    s.serialize(context, buffer, value);
                }
            } else {
                console.error("no serializer impl for", slotType);
            }
        } else {
            // typeof slot.type === "function" (a constructor)
            // nested value
            let fieldSlots: { [key: string]: PropertyMetaInfo } = slotType.prototype[Symbol_SerializedFields];
            if (typeof fieldSlots !== "undefined") {
                if (isArray) {
                    let section = this._serializeObjectArray(context, fieldSlots, value);

                    buffer.WriteByte(SerializedTypeID.Array);
                    buffer.WriteByte(SerializedTypeID.Object);
                    buffer.WriteInt32(section.readableBytes);
                    buffer.WriteBytes(section);
                } else {
                    let section = this._serializeObject(context, value, fieldSlots);

                    buffer.WriteByte(SerializedTypeID.Object);
                    buffer.WriteInt32(section.readableBytes);
                    buffer.WriteBytes(section);
                }
            } else {
                console.error("no serialization info on field", slot.name);
            }
        }
    }

    private static _serializeObjectArray(context: SerializationContext, slots: { [key: string]: PropertyMetaInfo }, value: any) {
        let length = value.length;
        let buffer = context.ps.AllocByteBuffer();

        for (let i = 0; i < length; ++i) {
            let section = this._serializeObject(context, value[i], slots);
            buffer.WriteInt32(section.readableBytes);
            buffer.WriteBytes(section);
        }
        return buffer;
    }

    private static _serializePrimitiveArray(context: SerializationContext, s: IPrimitiveSerializer, value: any) {
        let length = value.length;
        let buffer = context.ps.AllocByteBuffer();

        for (let i = 0; i < length; ++i) {
            s.serialize(context, buffer, value[i]);
        }
        return buffer;
    }

    private static _serializeObject(context: SerializationContext, target: any, slots: { [key: string]: PropertyMetaInfo }): ByteBuffer {
        let buffer = context.ps.AllocByteBuffer();

        for (let propertyKey in slots) {
            let slot = slots[propertyKey];

            if (slot.serializable) {
                let value = target && target[propertyKey];

                // skip undefined and null value
                if (value == null) {
                    continue;
                }
                buffer.WriteString(slot.name);
                this._serializeValue(context, slot, value, buffer);
            }
        }
        return buffer;
    }

    static deserialize(target: any, ps: JSSerializationContext, buffer: ByteBuffer) {
        this.markAsReady(target);
        let slots: {} = target[Symbol_SerializedFields];
        if (typeof slots !== "undefined") {
            let dataFormat = ps.dataFormat || 0;
            let impl = GetSerializer(dataFormat);

            if (typeof impl === "object") {
                this._deserializeObjectInternal({ impl: impl, ps: ps }, target, slots, buffer);
            } else {
                if (buffer.readableBytes > 0 && ps.dataFormat >= 0) {
                    console.error("no serializer for dataFormat", dataFormat);
                }
            }
        }
    }

    private static _deserializeObject(context: SerializationContext, slot: PropertyMetaInfo, buffer: ByteBuffer): any {
        if (typeof slot.type === "function") {
            let fieldValue = Object.create(slot.type);
            let fieldSlots = slot.type.prototype[Symbol_SerializedFields];
            this._deserializeObjectInternal(context, fieldValue, fieldSlots, buffer);
            return fieldValue;
        } else {
            console.error("expecting object but got primitive", slot.type);
        }
    }

    private static _deserializeObjectArray(context: SerializationContext, slot: PropertyMetaInfo, buffer: ByteBuffer): any[] {
        let items = [];
        while (buffer.readableBytes > 0) {
            let size = buffer.ReadInt32();
            let section = buffer.Slice(size);
            let value = this._deserializeObject(context, slot, section);
            items.push(value);
        }
        return items;
    }

    private static _deserializePrimitiveArray(context: SerializationContext, s: IPrimitiveSerializer, buffer: ByteBuffer): any[] {
        let items = [];
        while (buffer.readableBytes > 0) {
            let value = s.deserilize(context, buffer);
            items.push(value);
        }
        return items;
    }

    private static _deserializeObjectInternal(context: SerializationContext, target: any, slots: { [key: string]: PropertyMetaInfo }, buffer: ByteBuffer) {
        let slotByName = {};
        for (let propertyKey in slots) {
            let slot: PropertyMetaInfo = slots[propertyKey];
            if (slot.serializable) {
                slotByName[slot.name] = slot;

                if (typeof slot.type === "string") {
                    let defaultValue = context.impl.types[slot.type].defaultValue;
                    if (typeof defaultValue === "function") {
                        defaultValue = defaultValue();
                    }
                    target[slot.propertyKey] = defaultValue;
                } else {
                    target[slot.propertyKey] = null;
                }
            }
        }

        while (buffer.readableBytes > 0) {
            let name = buffer.ReadString();
            let typeid = buffer.ReadUByte();
            let slot: PropertyMetaInfo = slotByName[name];

            // should always read the buffer since the serialized field may be removed from script
            let s = context.impl.typeids[typeid];
            if (typeof s === "object") {
                let slot_value = s.deserilize(context, buffer);

                if (slot) {
                    if (typeof slot.type === "string") {
                        console.assert(typeid == context.impl.types[slot.type].typeid, "slot type mismatch");
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
                switch (typeid) {
                    case SerializedTypeID.Object: {
                        let size = buffer.ReadInt32();
                        let section = buffer.Slice(size);
                        target[slot.propertyKey] = this._deserializeObject(context, slot, section);
                        break;
                    }
                    case SerializedTypeID.Array: {
                        let elementTypeID = buffer.ReadUByte();
                        let size = buffer.ReadInt32();
                        let section = buffer.Slice(size);

                        let s: IPrimitiveSerializer = context.impl.typeids[elementTypeID];
                        if (typeof s === "undefined") {
                            target[slot.propertyKey] = this._deserializeObjectArray(context, slot, section);
                        } else {
                            target[slot.propertyKey] = this._deserializePrimitiveArray(context, s, section);
                        }
                        break;
                    }
                    case SerializedTypeID.Null: break;
                    default: {
                        console.error(`no serializer for serialized field ${name} with typeid ${typeid}`);
                        break;
                    }
                }
            }
        }
    }
}
