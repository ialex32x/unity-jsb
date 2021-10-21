"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.RegisterSerializer = exports.GetSerializer = exports.GetLatestSerializer = exports.SerializedTypeID = void 0;
const UnityEngine_1 = require("UnityEngine");
var SerializedTypeID;
(function (SerializedTypeID) {
    SerializedTypeID[SerializedTypeID["Null"] = 0] = "Null";
    SerializedTypeID[SerializedTypeID["UserDefinedMin"] = 1] = "UserDefinedMin";
    SerializedTypeID[SerializedTypeID["UserDefinedMax"] = 100] = "UserDefinedMax";
    SerializedTypeID[SerializedTypeID["Array"] = 101] = "Array";
    SerializedTypeID[SerializedTypeID["Object"] = 102] = "Object";
})(SerializedTypeID = exports.SerializedTypeID || (exports.SerializedTypeID = {}));
let _PrimitiveSerializerImpls = [];
let _LatestSerializer;
function GetLatestSerializer() {
    return _LatestSerializer;
}
exports.GetLatestSerializer = GetLatestSerializer;
function GetSerializer(dataFormat) {
    return _PrimitiveSerializerImpls[dataFormat];
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
        console.assert(type.typeid >= SerializedTypeID.UserDefinedMin && type.typeid <= SerializedTypeID.UserDefinedMax, "typeid must be greater than 0 and less than 100");
        impl.typeids[type.typeid] = type;
    }
    _PrimitiveSerializerImpls[dataFormat] = impl;
    if (!!bSetAsLatest) {
        _LatestSerializer = impl;
    }
}
exports.RegisterSerializer = RegisterSerializer;
RegisterSerializer(1, "v1: without size check", {
    "bool": {
        typeid: 1,
        defaultValue: false,
        serialize(context, buffer, value) {
            buffer.WriteBoolean(!!value);
        },
        deserilize(context, buffer) {
            return buffer.ReadBoolean();
        }
    },
    "float": {
        typeid: 2,
        defaultValue: 0,
        serialize(context, buffer, value) {
            if (typeof value === "number") {
                buffer.WriteSingle(value);
            }
            else {
                buffer.WriteSingle(0);
            }
        },
        deserilize(context, buffer) {
            return buffer.ReadSingle();
        }
    },
    "double": {
        typeid: 3,
        defaultValue: 0,
        serialize(context, buffer, value) {
            if (typeof value === "number") {
                buffer.WriteDouble(value);
            }
            else {
                buffer.WriteDouble(0);
            }
        },
        deserilize(context, buffer) {
            return buffer.ReadDouble();
        }
    },
    "string": {
        typeid: 4,
        defaultValue: null,
        serialize(context, buffer, value) {
            if (typeof value === "string") {
                buffer.WriteString(value);
            }
            else {
                buffer.WriteString(null);
            }
        },
        deserilize(context, buffer) {
            return buffer.ReadString();
        }
    },
    "int": {
        typeid: 5,
        defaultValue: 0,
        serialize(context, buffer, value) {
            if (typeof value === "number") {
                buffer.WriteInt32(value);
            }
            else {
                buffer.WriteInt32(0);
            }
        },
        deserilize(context, buffer) {
            return buffer.ReadInt32();
        }
    },
    "uint": {
        typeid: 6,
        defaultValue: 0,
        serialize(context, buffer, value) {
            if (typeof value === "number") {
                buffer.WriteUInt32(value);
            }
            else {
                buffer.WriteUInt32(0);
            }
        },
        deserilize(context, buffer) {
            return buffer.ReadUInt32();
        }
    },
    "Vector2": {
        typeid: 7,
        defaultValue: () => UnityEngine_1.Vector2.zero,
        serialize(context, buffer, value) {
            if (value instanceof UnityEngine_1.Vector2) {
                buffer.WriteSingle(value.x);
                buffer.WriteSingle(value.y);
            }
            else {
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
            }
        },
        deserilize(context, buffer) {
            return new UnityEngine_1.Vector2(buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
    "Vector3": {
        typeid: 8,
        defaultValue: () => UnityEngine_1.Vector3.zero,
        serialize(context, buffer, value) {
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
        deserilize(context, buffer) {
            return new UnityEngine_1.Vector3(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
    "Vector4": {
        typeid: 9,
        defaultValue: () => UnityEngine_1.Vector4.zero,
        serialize(context, buffer, value) {
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
        deserilize(context, buffer) {
            return new UnityEngine_1.Vector4(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
    "Rect": {
        typeid: 10,
        defaultValue: () => UnityEngine_1.Rect.zero,
        serialize(context, buffer, value) {
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
        deserilize(context, buffer) {
            return new UnityEngine_1.Rect(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
    "Quaternion": {
        typeid: 11,
        defaultValue: () => UnityEngine_1.Quaternion.identity,
        serialize(context, buffer, value) {
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
        deserilize(context, buffer) {
            return new UnityEngine_1.Quaternion(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
    "object": {
        typeid: 12,
        defaultValue: null,
        serialize(context, buffer, value) {
            if (value instanceof UnityEngine_1.Object) {
                let index = context.ps.AddReferencedObject(value);
                buffer.WriteInt32(index);
            }
            else {
                if (!!value) {
                    console.error("only types inheriting UnityEngine.Object is unsupported", value);
                }
                buffer.WriteInt32(-1);
            }
        },
        deserilize(context, buffer) {
            let index = buffer.ReadInt32();
            return context.ps.GetReferencedObject(index);
        }
    },
    // js Uint8ArrayBuffer
    "Uint8ArrayBuffer": {
        typeid: 13,
        defaultValue: null,
        serialize(context, buffer, value) {
            if (value instanceof Uint8Array) {
                let length = value.byteLength;
                buffer.WriteInt32(length);
                for (let i = 0; i < length; ++i) {
                    buffer.WriteByte(value[i]);
                }
            }
            else {
                buffer.WriteInt32(-1);
            }
        },
        deserilize(context, buffer) {
            let length = buffer.ReadInt32();
            if (length < 0) {
                return null;
            }
            else {
                let items = new Uint8Array(length);
                for (let i = 0; i < length; ++i) {
                    items[i] = buffer.ReadUByte();
                }
                return items;
            }
        }
    },
    "json": {
        typeid: 14,
        defaultValue: null,
        serialize(context, buffer, value) {
            if (typeof value === "object") {
                let json = JSON.stringify(value);
                buffer.WriteString(json);
            }
            else {
                buffer.WriteString(null);
            }
        },
        deserilize(context, buffer) {
            let json = buffer.ReadString();
            if (typeof json === "string") {
                return JSON.parse(json);
            }
            return null;
        }
    },
}, true);
//# sourceMappingURL=serialize.js.map