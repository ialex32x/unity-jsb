import { JSBehaviourProperties } from "QuickJS.Unity";

let SerializedFields = Symbol("SerializedFields");

interface SlotEntry {
    propertyKey: string;
    type: string;
}

// expose this script class type to JSBehaviour, so you can put it on a prefab gameObject
export function ScriptType(target: any) {
    return target;
}

// path: 指定编辑器脚本所在模块, 暂时不支持相对路径
export function Inspector(path: string, className: string) {
    return function (target: any) {
        // 暂时简单实现
        target.prototype.__editor__ = require(path)[className];
        return target;
    }
}

export function SerializedNumber(name?: string) {
    return Serialized(name, "number");
}

export function SerializedString(name?: string) {
    return Serialized(name, "string");
}

export function SerializedObject(name?: string) {
    return Serialized(name, "object");
}

export function Serialized(name?: string, type?: string) {
    return function (target: any, propertyKey: string) {
        let slots: { [k: string]: SlotEntry } = target[SerializedFields];
        if (typeof slots === "undefined") {
            slots = target[SerializedFields] = {};
        }

        let theName = typeof name === "undefined" ? propertyKey : name;
        slots[theName] = {
            propertyKey: propertyKey,
            type: type,
        };
    }
}

export class SerializationUtil {
    static forEach(target: any, extra: any, cb: (name: string, slot: SlotEntry, target: any, extra: any) => void) {
        let slots: {} = target[SerializedFields];
        if (typeof slots !== "undefined") {
            for (let slotName in slots) {
                cb(slotName, slots[slotName], target, extra);
            }
        }
    }

    static serialize(target: any, ps: JSBehaviourProperties) {
        this.forEach(target, ps, (name, slot, self, extra) => {
            let value = self[slot.propertyKey];

            switch(slot.type) {
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

    static deserialize(target: any, ps: JSBehaviourProperties) {
        this.forEach(target, ps, (name, slot, self, extra) => {
            switch(slot.type) {
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