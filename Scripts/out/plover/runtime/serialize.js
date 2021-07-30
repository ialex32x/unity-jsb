"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.DefaultSerializer = void 0;
const UnityEngine_1 = require("UnityEngine");
class DefaultSerializer {
    static serialize(type, buffer, value) {
        let s = DefaultSerializers[type];
        if (typeof s === "object") {
            s.serialize(buffer, value);
        }
    }
    static deserilize(type, buffer) {
        let s = DefaultSerializers[type];
        if (typeof s === "object") {
            return s.deserilize(buffer);
        }
        return undefined;
    }
}
exports.DefaultSerializer = DefaultSerializer;
let DefaultSerializers = {
    "bool": {
        serialize(buffer, value) {
            buffer.WriteInt32(1);
            buffer.WriteBoolean(!!value);
        },
        deserilize(buffer) {
            let size = buffer.ReadInt32();
            console.assert(size == 1);
            return buffer.ReadBoolean();
        }
    },
    "float": {
        serialize(buffer, value) {
            buffer.WriteInt32(4);
            if (value) {
                buffer.WriteSingle(value);
            }
            else {
                buffer.WriteSingle(0);
            }
        },
        deserilize(buffer) {
            let size = buffer.ReadInt32();
            console.assert(size == 4);
            return buffer.ReadSingle();
        }
    },
    "double": {
        serialize(buffer, value) {
            buffer.WriteInt32(8);
            if (value) {
                buffer.WriteDouble(value);
            }
            else {
                buffer.WriteDouble(0);
            }
        },
        deserilize(buffer) {
            let size = buffer.ReadInt32();
            console.assert(size == 8);
            return buffer.ReadDouble();
        }
    },
    "Vector3": {
        serialize(buffer, value) {
            buffer.WriteInt32(12);
            if (value) {
                buffer.WriteSingle(value.x);
                buffer.WriteSingle(value.y);
                buffer.WriteSingle(value.z);
            }
            else {
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
            }
        },
        deserilize(buffer) {
            let size = buffer.ReadInt32();
            console.assert(size == 12);
            return new UnityEngine_1.Vector3(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
    "Quaternion": {
        serialize(buffer, value) {
            buffer.WriteInt32(16);
            if (value) {
                buffer.WriteSingle(value.x);
                buffer.WriteSingle(value.y);
                buffer.WriteSingle(value.z);
                buffer.WriteSingle(value.w);
            }
            else {
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
                buffer.WriteSingle(1);
            }
        },
        deserilize(buffer) {
            let size = buffer.ReadInt32();
            console.assert(size == 16);
            return new UnityEngine_1.Quaternion(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
        }
    }
};
//# sourceMappingURL=serialize.js.map