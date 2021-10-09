"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.SerializationUtil = exports.ScriptFunction = exports.ScriptProperty = exports.ScriptObject = exports.ScriptString = exports.ScriptNumber = exports.ScriptInteger = exports.ScriptType = exports.ScriptAsset = void 0;
const serialize_1 = require("./serialize");
let Symbol_PropertiesTouched = Symbol.for("PropertiesTouched");
let Symbol_MemberFuncs = Symbol.for("MemberFuncs");
let Symbol_SerializedFields = Symbol.for("SerializedFields");
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
                cb(propertyKey, slots[propertyKey]);
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
                            let s = impl.types[slot.type];
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
    static deserialize(target, ps, buffer) {
        this.markAsReady(target);
        let slots = target[Symbol_SerializedFields];
        if (typeof slots !== "undefined") {
            let slotByName = {};
            let dataFormat = ps.dataFormat || 0;
            let impl = serialize_1.GetSerializer(dataFormat);
            if (typeof impl === "object") {
                for (let propertyKey in slots) {
                    let slot = slots[propertyKey];
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
                    let slot = slotByName[name];
                    if (slot) {
                        console.assert(impl.types[slot.type].typeid == s.typeid);
                        target[slot.propertyKey] = slot_value;
                    }
                }
            }
            else {
                console.error("no serializer for dataFormat", dataFormat);
            }
        }
    }
}
exports.SerializationUtil = SerializationUtil;
//# sourceMappingURL=class_decorators.js.map