"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.SerializationUtil = exports.ScriptFunction = exports.ScriptProperty = exports.ScriptObject = exports.ScriptString = exports.ScriptNumber = exports.ScriptInteger = exports.ScriptType = exports.ScriptAsset = exports.ScriptSerializable = void 0;
const UnityEngine_1 = require("UnityEngine");
const serialize_1 = require("./serialize");
let Symbol_PropertiesTouched = Symbol.for("PropertiesTouched");
let Symbol_MemberFuncs = Symbol.for("MemberFuncs");
let Symbol_SerializedFields = Symbol.for("SerializedFields");
function ScriptSerializable(meta) {
    return ScriptType(meta);
}
exports.ScriptSerializable = ScriptSerializable;
function ScriptAsset(meta) {
    return ScriptType(meta);
}
exports.ScriptAsset = ScriptAsset;
// expose this script class type to JSBehaviour, so you can put it on a prefab gameObject
function ScriptType(meta) {
    return function (target) {
        let OnBeforeSerialize = target.prototype["OnBeforeSerialize"];
        target.prototype["OnBeforeSerialize"] = function (ps) {
            this[Symbol_PropertiesTouched] = false;
            if (typeof OnBeforeSerialize === "function") {
                OnBeforeSerialize.call(this, ps);
            }
            if (!this[Symbol_PropertiesTouched]) {
                SerializationUtil.serialize(this, ps);
            }
        };
        let OnAfterDeserialize = target.prototype["OnAfterDeserialize"];
        target.prototype["OnAfterDeserialize"] = function (ps, buffer) {
            this[Symbol_PropertiesTouched] = false;
            if (typeof OnAfterDeserialize === "function") {
                OnAfterDeserialize.call(this, ps, buffer);
            }
            if (!this[Symbol_PropertiesTouched]) {
                SerializationUtil.deserialize(this, ps, buffer);
            }
        };
        return target;
    };
}
exports.ScriptType = ScriptType;
function ScriptInteger(meta) {
    let meta_t = meta;
    if (typeof meta_t === "undefined") {
        meta_t = { type: "int" };
    }
    else {
        meta_t.type = "int";
    }
    return ScriptProperty(meta_t);
}
exports.ScriptInteger = ScriptInteger;
function ScriptNumber(meta) {
    let meta_t = meta;
    if (typeof meta_t === "undefined") {
        meta_t = { type: "float" };
    }
    else {
        meta_t.type = "float";
    }
    return ScriptProperty(meta_t);
}
exports.ScriptNumber = ScriptNumber;
function ScriptString(meta) {
    let meta_t = meta;
    if (typeof meta_t === "undefined") {
        meta_t = { type: "string" };
    }
    else {
        meta_t.type = "string";
    }
    return ScriptProperty(meta_t);
}
exports.ScriptString = ScriptString;
function ScriptObject(meta) {
    let meta_t = meta;
    if (typeof meta_t === "undefined") {
        meta_t = { type: "object" };
    }
    else {
        meta_t.type = "object";
    }
    return ScriptProperty(meta_t);
}
exports.ScriptObject = ScriptObject;
function ScriptProperty(meta) {
    return function (target, propertyKey) {
        let slots = target[Symbol_SerializedFields];
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
    };
}
exports.ScriptProperty = ScriptProperty;
function ScriptFunction(meta) {
    return function (target, propertyKey) {
        let funcMap = target[Symbol_MemberFuncs];
        if (typeof funcMap === "undefined") {
            funcMap = target[Symbol_MemberFuncs] = {};
        }
        funcMap[propertyKey] = propertyKey;
    };
}
exports.ScriptFunction = ScriptFunction;
class SerializationUtil {
    static forEach(target, cb) {
        let slots = target[Symbol_SerializedFields];
        if (typeof slots !== "undefined") {
            for (let propertyKey in slots) {
                cb(slots, propertyKey);
            }
        }
    }
    // 当不需要默认行为时, 调用此函数将序列化状态标记为已完成, 以便跳过默认的 serialize/deserialize 行为
    static markAsReady(target) {
        target[Symbol_PropertiesTouched] = true;
    }
    static serialize(target, ps) {
        this.markAsReady(target);
        let impl = serialize_1.GetLatestSerializer();
        console.assert(typeof ps === "object");
        if (typeof impl === "object") {
            ps.dataFormat = impl.dataFormat;
            let slots = target[Symbol_SerializedFields];
            if (typeof slots !== "undefined") {
                let buffer = this._serializeObject({ impl: impl, ps: ps }, target, slots);
                ps.Flush(buffer);
            }
        }
    }
    static _serializeValue(context, slot, value, buffer) {
        let slotType = slot.type;
        let isArray = value instanceof Array;
        if (typeof value === "object") {
            if (value instanceof UnityEngine_1.Object) {
                slotType = "object";
            }
        }
        if (typeof slotType === "string") {
            // primitive serializer impl
            let s = context.impl.types[slotType];
            if (typeof s === "object") {
                if (isArray) {
                    let section = this._serializePrimitiveArray(context, s, value);
                    buffer.WriteByte(serialize_1.SerializedTypeID.Array);
                    buffer.WriteByte(s.typeid);
                    buffer.WriteInt32(section.readableBytes);
                    buffer.WriteBytes(section);
                }
                else {
                    buffer.WriteByte(s.typeid);
                    s.serialize(context, buffer, value);
                }
            }
            else {
                console.error("no serializer impl for", slotType);
            }
        }
        else {
            // typeof slot.type === "function" (a constructor)
            // nested value
            let fieldSlots = slotType.prototype[Symbol_SerializedFields];
            if (typeof fieldSlots !== "undefined") {
                if (isArray) {
                    let section = this._serializeObjectArray(context, fieldSlots, value);
                    buffer.WriteByte(serialize_1.SerializedTypeID.Array);
                    buffer.WriteByte(serialize_1.SerializedTypeID.Object);
                    buffer.WriteInt32(section.readableBytes);
                    buffer.WriteBytes(section);
                }
                else {
                    let section = this._serializeObject(context, value, fieldSlots);
                    buffer.WriteByte(serialize_1.SerializedTypeID.Object);
                    buffer.WriteInt32(section.readableBytes);
                    buffer.WriteBytes(section);
                }
            }
            else {
                console.error("no serialization info on field", slot.name);
            }
        }
    }
    static _serializeObjectArray(context, slots, value) {
        let length = value.length;
        let buffer = context.ps.AllocByteBuffer();
        for (let i = 0; i < length; ++i) {
            let section = this._serializeObject(context, value[i], slots);
            buffer.WriteInt32(section.readableBytes);
            buffer.WriteBytes(section);
        }
        return buffer;
    }
    static _serializePrimitiveArray(context, s, value) {
        let length = value.length;
        let buffer = context.ps.AllocByteBuffer();
        for (let i = 0; i < length; ++i) {
            s.serialize(context, buffer, value[i]);
        }
        return buffer;
    }
    static _serializeObject(context, target, slots) {
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
    static deserialize(target, ps, buffer) {
        this.markAsReady(target);
        let slots = target[Symbol_SerializedFields];
        if (typeof slots !== "undefined") {
            let dataFormat = ps.dataFormat || 0;
            let impl = serialize_1.GetSerializer(dataFormat);
            if (typeof impl === "object") {
                this._deserializeObjectInternal({ impl: impl, ps: ps }, target, slots, buffer);
            }
            else {
                if (buffer.readableBytes > 0 && ps.dataFormat >= 0) {
                    console.error("no serializer for dataFormat", dataFormat);
                }
            }
        }
    }
    static _deserializeObject(context, slot, buffer) {
        if (typeof slot.type === "function") {
            let fieldValue = Object.create(slot.type);
            let fieldSlots = slot.type.prototype[Symbol_SerializedFields];
            this._deserializeObjectInternal(context, fieldValue, fieldSlots, buffer);
            return fieldValue;
        }
        else {
            console.error("expecting object but got primitive", slot.type);
        }
    }
    static _deserializeObjectArray(context, slot, buffer) {
        let items = [];
        while (buffer.readableBytes > 0) {
            let size = buffer.ReadInt32();
            let section = buffer.Slice(size);
            let value = this._deserializeObject(context, slot, section);
            items.push(value);
        }
        return items;
    }
    static _deserializePrimitiveArray(context, s, buffer) {
        let items = [];
        while (buffer.readableBytes > 0) {
            let value = s.deserilize(context, buffer);
            items.push(value);
        }
        return items;
    }
    static _deserializeObjectInternal(context, target, slots, buffer) {
        let slotByName = {};
        for (let propertyKey in slots) {
            let slot = slots[propertyKey];
            if (slot.serializable) {
                slotByName[slot.name] = slot;
                if (typeof slot.type === "string") {
                    let defaultValue = context.impl.types[slot.type].defaultValue;
                    if (typeof defaultValue === "function") {
                        defaultValue = defaultValue();
                    }
                    target[slot.propertyKey] = defaultValue;
                }
                else {
                    target[slot.propertyKey] = null;
                }
            }
        }
        while (buffer.readableBytes > 0) {
            let name = buffer.ReadString();
            let typeid = buffer.ReadUByte();
            let slot = slotByName[name];
            // should always read the buffer since the serialized field may be removed from script
            let s = context.impl.typeids[typeid];
            if (typeof s === "object") {
                let slot_value = s.deserilize(context, buffer);
                if (slot) {
                    if (typeof slot.type === "string") {
                        console.assert(typeid == context.impl.types[slot.type].typeid, "slot type mismatch");
                    }
                    else {
                        if (typeof slot_value === "object") {
                            console.assert(slot_value instanceof slot.type, "slot type mismatch");
                        }
                    }
                    target[slot.propertyKey] = slot_value;
                }
                else {
                    console.warn("failed to read slot", name);
                }
            }
            else {
                switch (typeid) {
                    case serialize_1.SerializedTypeID.Object: {
                        let size = buffer.ReadInt32();
                        let section = buffer.Slice(size);
                        target[slot.propertyKey] = this._deserializeObject(context, slot, section);
                        break;
                    }
                    case serialize_1.SerializedTypeID.Array: {
                        let elementTypeID = buffer.ReadUByte();
                        let size = buffer.ReadInt32();
                        let section = buffer.Slice(size);
                        let s = context.impl.typeids[elementTypeID];
                        if (typeof s === "undefined") {
                            target[slot.propertyKey] = this._deserializeObjectArray(context, slot, section);
                        }
                        else {
                            target[slot.propertyKey] = this._deserializePrimitiveArray(context, s, section);
                        }
                        break;
                    }
                    case serialize_1.SerializedTypeID.Null: break;
                    default: {
                        console.error(`no serializer for serialized field ${name} with typeid ${typeid}`);
                        break;
                    }
                }
            }
        }
    }
}
exports.SerializationUtil = SerializationUtil;
//# sourceMappingURL=class_decorators.js.map