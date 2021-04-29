"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.SerializationUtil = exports.Serialized = exports.SerializedObject = exports.SerializedString = exports.SerializedNumber = exports.Inspector = exports.ScriptType = void 0;
let SerializedFields = globalThis["__SerializedFields_Symbol__"];
if (typeof SerializedFields === "undefined") {
    SerializedFields = globalThis["__SerializedFields_Symbol__"] = Symbol("SerializedFields");
}
// expose this script class type to JSBehaviour, so you can put it on a prefab gameObject
function ScriptType(target) {
    return target;
}
exports.ScriptType = ScriptType;
// path: 指定编辑器脚本所在模块, 暂时不支持相对路径
function Inspector(path, className) {
    return function (target) {
        // 暂时简单实现
        target.prototype.__editor__ = require(path)[className];
        return target;
    };
}
exports.Inspector = Inspector;
function SerializedNumber(name) {
    return Serialized(name, "number");
}
exports.SerializedNumber = SerializedNumber;
function SerializedString(name) {
    return Serialized(name, "string");
}
exports.SerializedString = SerializedString;
function SerializedObject(name) {
    return Serialized(name, "object");
}
exports.SerializedObject = SerializedObject;
function Serialized(name, type) {
    return function (target, propertyKey) {
        let slots = target[SerializedFields];
        if (typeof slots === "undefined") {
            slots = target[SerializedFields] = {};
        }
        let theName = typeof name === "undefined" ? propertyKey : name;
        slots[theName] = {
            propertyKey: propertyKey,
            type: type,
        };
    };
}
exports.Serialized = Serialized;
class SerializationUtil {
    static forEach(target, extra, cb) {
        let slots = target[SerializedFields];
        if (typeof slots !== "undefined") {
            for (let slotName in slots) {
                cb(slotName, slots[slotName], target, extra);
            }
        }
    }
    static serialize(target, ps) {
        this.forEach(target, ps, (name, slot, self, extra) => {
            let value = self[slot.propertyKey];
            // console.log("serializing", slot.propertyKey, value);
            switch (slot.type) {
                case "number": {
                    extra.SetNumber(name, typeof value === "number" ? value : 0);
                    break;
                }
                case "string": {
                    extra.SetString(name, value);
                    break;
                }
                case "object": {
                    extra.SetObject(name, value);
                    break;
                }
            }
        });
    }
    static deserialize(target, ps) {
        this.forEach(target, ps, (name, slot, self, extra) => {
            let value = null;
            switch (slot.type) {
                case "number": {
                    value = extra.GetNumber(name);
                    break;
                }
                case "string": {
                    value = extra.GetString(name);
                    break;
                }
                case "object": {
                    value = extra.GetObject(name);
                    break;
                }
            }
            self[slot.propertyKey] = value;
            // console.log("deserialize", slot.propertyKey, value);
        });
    }
}
exports.SerializationUtil = SerializationUtil;
//# sourceMappingURL=inspector.js.map