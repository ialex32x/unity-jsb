"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.SerializationUtil = exports.Serialized = exports.SerializedNumber = exports.Inspector = void 0;
let SerializedFields = Symbol("SerializedFields");
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
            switch (slot.type) {
                case "number": {
                    self[slot.propertyKey] = extra.GetNumber(name);
                    // console.log("get number", name, slot.propertyKey, self[slot.propertyKey]);
                    break;
                }
                case "string": {
                    self[slot.propertyKey] = extra.GetString(name);
                    break;
                }
                case "object": {
                    self[slot.propertyKey] = extra.GetObject(name);
                    break;
                }
            }
        });
    }
}
exports.SerializationUtil = SerializationUtil;
//# sourceMappingURL=inspector.js.map