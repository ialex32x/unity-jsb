"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.RegisterSerializer = exports.GetSerializer = exports.GetLatestSerializer = exports.DefaultSerializer = void 0;
const UnityEngine_1 = require("UnityEngine");
class DefaultSerializer {
    static serialize(type, buffer, value) {
        let p = _LatestSerializer;
        if (typeof p === "object") {
            let s = p[type];
            if (typeof s === "object") {
                s.serialize(buffer, value);
            }
        }
    }
    static deserilize(dataFormat, type, buffer) {
        let p = _DefaultSerializers[dataFormat] || _LatestSerializer;
        if (typeof p === "object") {
            let s = p[type];
            if (typeof s === "object") {
                return s.deserilize(buffer);
            }
        }
        return undefined;
    }
}
exports.DefaultSerializer = DefaultSerializer;
let _DefaultSerializers = [];
let _LatestSerializer;
function GetLatestSerializer() {
    return _LatestSerializer;
}
exports.GetLatestSerializer = GetLatestSerializer;
function GetSerializer(dataFormat) {
    return _DefaultSerializers[dataFormat];
}
exports.GetSerializer = GetSerializer;
function RegisterSerializer(dataFormat, description, types, bSetAsLatest) {
    let impl = {
        dataFormat: dataFormat,
        description: description,
        types: types,
        typeids: [],
    };
    for (let typename in types) {
        let type = types[typename];
        impl.typeids[type.typeid] = type;
    }
    _DefaultSerializers[dataFormat] = impl;
    if (!!bSetAsLatest) {
        _LatestSerializer = impl;
    }
}
exports.RegisterSerializer = RegisterSerializer;
RegisterSerializer(1, "v1: without size check", {
    "bool": {
        typeid: 1,
        defaultValue: false,
        serialize(buffer, value) {
            buffer.WriteBoolean(!!value);
        },
        deserilize(buffer) {
            return buffer.ReadBoolean();
        }
    },
    "float": {
        typeid: 2,
        defaultValue: 0,
        serialize(buffer, value) {
            if (typeof value === "number") {
                buffer.WriteSingle(value);
            }
            else {
                buffer.WriteSingle(0);
            }
        },
        deserilize(buffer) {
            return buffer.ReadSingle();
        }
    },
    "double": {
        typeid: 3,
        defaultValue: 0,
        serialize(buffer, value) {
            if (typeof value === "number") {
                buffer.WriteDouble(value);
            }
            else {
                buffer.WriteDouble(0);
            }
        },
        deserilize(buffer) {
            return buffer.ReadDouble();
        }
    },
    "string": {
        typeid: 4,
        defaultValue: null,
        serialize(buffer, value) {
            if (typeof value === "string") {
                buffer.WriteString(value);
            }
            else {
                buffer.WriteString(null);
            }
        },
        deserilize(buffer) {
            return buffer.ReadString();
        }
    },
    "int": {
        typeid: 5,
        defaultValue: 0,
        serialize(buffer, value) {
            if (typeof value === "number") {
                buffer.WriteInt32(value);
            }
            else {
                buffer.WriteInt32(0);
            }
        },
        deserilize(buffer) {
            return buffer.ReadInt32();
        }
    },
    "uint": {
        typeid: 6,
        defaultValue: 0,
        serialize(buffer, value) {
            if (typeof value === "number") {
                buffer.WriteUInt32(value);
            }
            else {
                buffer.WriteUInt32(0);
            }
        },
        deserilize(buffer) {
            return buffer.ReadUInt32();
        }
    },
    "Vector2": {
        typeid: 7,
        defaultValue: UnityEngine_1.Vector2.zero,
        serialize(buffer, value) {
            if (value instanceof UnityEngine_1.Vector2) {
                buffer.WriteSingle(value.x);
                buffer.WriteSingle(value.y);
            }
            else {
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
            }
        },
        deserilize(buffer) {
            return new UnityEngine_1.Vector2(buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
    "Vector3": {
        typeid: 8,
        defaultValue: UnityEngine_1.Vector3.zero,
        serialize(buffer, value) {
            if (value instanceof UnityEngine_1.Vector3) {
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
            return new UnityEngine_1.Vector3(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
    "Vector4": {
        typeid: 9,
        defaultValue: UnityEngine_1.Vector4.zero,
        serialize(buffer, value) {
            if (value instanceof UnityEngine_1.Vector4) {
                buffer.WriteSingle(value.x);
                buffer.WriteSingle(value.y);
                buffer.WriteSingle(value.z);
                buffer.WriteSingle(value.w);
            }
            else {
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
            }
        },
        deserilize(buffer) {
            return new UnityEngine_1.Vector4(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
    "Rect": {
        typeid: 10,
        defaultValue: UnityEngine_1.Rect.zero,
        serialize(buffer, value) {
            if (value instanceof UnityEngine_1.Rect) {
                buffer.WriteSingle(value.x);
                buffer.WriteSingle(value.y);
                buffer.WriteSingle(value.width);
                buffer.WriteSingle(value.height);
            }
            else {
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
            }
        },
        deserilize(buffer) {
            return new UnityEngine_1.Rect(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
    "Quaternion": {
        typeid: 11,
        defaultValue: UnityEngine_1.Quaternion.identity,
        serialize(buffer, value) {
            if (value instanceof UnityEngine_1.Quaternion) {
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
            return new UnityEngine_1.Quaternion(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
}, true);
//# sourceMappingURL=serialize.js.map