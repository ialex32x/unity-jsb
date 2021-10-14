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
        target.prototype["OnBeforeSerialize"] = function (ps, buffer) {
            this[Symbol_PropertiesTouched] = false;
            if (typeof OnBeforeSerialize === "function") {
                OnBeforeSerialize.call(this, ps, buffer);
            }
            if (!this[Symbol_PropertiesTouched]) {
                SerializationUtil.serialize(this, ps, buffer);
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
    static serialize(target, ps, buffer) {
        this.markAsReady(target);
        let impl = serialize_1.GetLatestSerializer();
        console.assert(typeof ps === "object");
        if (typeof impl === "object") {
            ps.dataFormat = impl.dataFormat;
            let slots = target[Symbol_SerializedFields];
            if (typeof slots !== "undefined") {
                this._serializeField(target, slots, ps, buffer, impl);
            }
        }
    }
    static _serializeField(target, slots, ps, parent, impl) {
        for (let propertyKey in slots) {
            let slot = slots[propertyKey];
            if (slot.serializable) {
                let value = target && target[propertyKey];
                if (typeof slot.type === "string") {
                    let s = impl.types[slot.type];
                    if (typeof s === "object") {
                        parent.WriteString(slot.name);
                        parent.WriteByte(s.typeid);
                        s.serialize(ps, parent, value);
                    }
                }
                else {
                    // typeof slot.type === "function" (a constructor)
                    // nested value
                    if (typeof value === "object") {
                        if (value instanceof UnityEngine_1.Object) {
                            let s = impl.types["object"];
                            parent.WriteString(slot.name);
                            parent.WriteByte(s.typeid);
                            s.serialize(ps, parent, value);
                        }
                        else {
                            let fieldSlots = slot.type.prototype[Symbol_SerializedFields];
                            if (typeof fieldSlots !== "undefined") {
                                let fieldBuffer = ps.NewSection();
                                this._serializeField(value, fieldSlots, ps, fieldBuffer, impl);
                                parent.WriteString(slot.name);
                                parent.WriteByte(0);
                                parent.WriteInt32(fieldBuffer.readableBytes);
                                parent.WriteBytes(fieldBuffer);
                            }
                            else {
                                console.error("no serialization info on field", slot.name);
                            }
                        }
                    }
                    else {
                        // skip invalid value
                    }
                }
            }
        }
    }
    static deserialize(target, ps, buffer) {
        this.markAsReady(target);
        let slots = target[Symbol_SerializedFields];
        if (typeof slots !== "undefined") {
            let dataFormat = ps.dataFormat || 0;
            let impl = serialize_1.GetSerializer(dataFormat);
            if (typeof impl === "object") {
                this._deserializeField(target, slots, ps, buffer, impl);
            }
            else {
                if (ps.GenericCount > 0 && ps.dataFormat >= 0) {
                    console.error("no serializer for dataFormat", dataFormat);
                }
            }
        }
    }
    static _deserializeField(target, slots, ps, parent, impl) {
        let slotByName = {};
        for (let propertyKey in slots) {
            let slot = slots[propertyKey];
            if (slot.serializable) {
                slotByName[slot.name] = slot;
                if (typeof slot.type === "string") {
                    let defaultValue = impl.types[slot.type].defaultValue;
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
        while (parent.readableBytes > 0) {
            let name = parent.ReadString();
            let typeid = parent.ReadUByte();
            let slot = slotByName[name];
            if (typeid > 0) {
                let s = impl.typeids[typeid];
                let slot_value = s.deserilize(ps, parent);
                if (slot) {
                    if (typeof slot.type === "string") {
                        console.assert(typeid == impl.types[slot.type].typeid, "slot type mismatch");
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
                let size = parent.ReadInt32();
                let fieldBuffer = ps.ReadSection(parent, size);
                if (slot && typeof slot.type === "function") {
                    let fieldSlots = slot.type.prototype[Symbol_SerializedFields];
                    if (fieldSlots) {
                        let fieldValue = Object.create(slot.type);
                        this._deserializeField(fieldValue, fieldSlots, ps, fieldBuffer, impl);
                        target[slot.propertyKey] = fieldValue;
                    }
                    else {
                        console.error("no serialization info on field", slot.name);
                    }
                }
            }
        }
    }
}
exports.SerializationUtil = SerializationUtil;
//# sourceMappingURL=class_decorators.js.map