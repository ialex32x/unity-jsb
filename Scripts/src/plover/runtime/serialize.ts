import { ByteBuffer } from "QuickJS.IO";
import { JSScriptProperties } from "QuickJS.Unity";
import { Object, Quaternion, Rect, Vector2, Vector3, Vector4 } from "UnityEngine";

export interface ISerializer {
    typeid: number;
    defaultValue: any;
    serialize(ps: JSScriptProperties, buffer: ByteBuffer, value: any): void;
    deserilize(ps: JSScriptProperties, buffer: ByteBuffer): any;
}

export interface SerializerImpl {
    dataFormat: number;
    description: string
    types: { [key: string]: ISerializer }
    typeids: ISerializer[]
}

let _DefaultSerializers: SerializerImpl[] = [];
let _LatestSerializer: SerializerImpl;

export function GetLatestSerializer() {
    return _LatestSerializer;
}

export function GetSerializer(dataFormat: number) {
    return _DefaultSerializers[dataFormat];
}

export function RegisterSerializer(dataFormat: number, description: string, types: { [key: string]: ISerializer }, bSetAsLatest?: boolean) {
    let impl: SerializerImpl = {
        dataFormat: dataFormat,
        description: description,
        types: types,
        typeids: [],
    }

    for (let typename in types) {
        let type = types[typename];

        console.assert(type.typeid > 0, "typeid must be greater than 0");
        impl.typeids[type.typeid] = type;
    }

    _DefaultSerializers[dataFormat] = impl;
    if (!!bSetAsLatest) {
        _LatestSerializer = impl;
    }
}

RegisterSerializer(1, "v1: without size check", {
    "bool": {
        typeid: 1,
        defaultValue: false,
        serialize(ps: JSScriptProperties, buffer: ByteBuffer, value: boolean) {
            buffer.WriteBoolean(!!value);
        },

        deserilize(ps: JSScriptProperties, buffer: ByteBuffer) {
            return buffer.ReadBoolean();
        }
    },
    "float": {
        typeid: 2,
        defaultValue: 0,
        serialize(ps: JSScriptProperties, buffer: ByteBuffer, value: number) {
            if (typeof value === "number") {
                buffer.WriteSingle(value);
            } else {
                buffer.WriteSingle(0);
            }
        },

        deserilize(ps: JSScriptProperties, buffer: ByteBuffer) {
            return buffer.ReadSingle();
        }
    },
    "double": {
        typeid: 3,
        defaultValue: 0,
        serialize(ps: JSScriptProperties, buffer: ByteBuffer, value: number) {
            if (typeof value === "number") {
                buffer.WriteDouble(value);
            } else {
                buffer.WriteDouble(0);
            }
        },

        deserilize(ps: JSScriptProperties, buffer: ByteBuffer) {
            return buffer.ReadDouble();
        }
    },
    "string": {
        typeid: 4,
        defaultValue: null,
        serialize(ps: JSScriptProperties, buffer: ByteBuffer, value: string) {
            if (typeof value === "string") {
                buffer.WriteString(value);
            } else {
                buffer.WriteString(null);
            }
        },

        deserilize(ps: JSScriptProperties, buffer: ByteBuffer) {
            return buffer.ReadString();
        }
    },
    "int": {
        typeid: 5,
        defaultValue: 0,
        serialize(ps: JSScriptProperties, buffer: ByteBuffer, value: number) {
            if (typeof value === "number") {
                buffer.WriteInt32(value);
            } else {
                buffer.WriteInt32(0);
            }
        },

        deserilize(ps: JSScriptProperties, buffer: ByteBuffer) {
            return buffer.ReadInt32();
        }
    },
    "uint": {
        typeid: 6,
        defaultValue: 0,
        serialize(ps: JSScriptProperties, buffer: ByteBuffer, value: number) {
            if (typeof value === "number") {
                buffer.WriteUInt32(value);
            } else {
                buffer.WriteUInt32(0);
            }
        },

        deserilize(ps: JSScriptProperties, buffer: ByteBuffer) {
            return buffer.ReadUInt32();
        }
    },
    "Vector2": {
        typeid: 7,
        defaultValue: () => Vector2.zero,
        serialize(ps: JSScriptProperties, buffer: ByteBuffer, value: Vector2) {
            if (value instanceof Vector2) {
                buffer.WriteSingle(value.x);
                buffer.WriteSingle(value.y);
            } else {
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
            }
        },

        deserilize(ps: JSScriptProperties, buffer: ByteBuffer) {
            return new Vector2(buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
    "Vector3": {
        typeid: 8,
        defaultValue: () => Vector3.zero,
        serialize(ps: JSScriptProperties, buffer: ByteBuffer, value: Vector3) {
            if (value instanceof Vector3) {
                buffer.WriteSingle(value.x);
                buffer.WriteSingle(value.y);
                buffer.WriteSingle(value.z);
            } else {
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
            }
        },

        deserilize(ps: JSScriptProperties, buffer: ByteBuffer) {
            return new Vector3(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
    "Vector4": {
        typeid: 9,
        defaultValue: () => Vector4.zero,
        serialize(ps: JSScriptProperties, buffer: ByteBuffer, value: Vector4) {
            if (value instanceof Vector4) {
                buffer.WriteSingle(value.x);
                buffer.WriteSingle(value.y);
                buffer.WriteSingle(value.z);
                buffer.WriteSingle(value.w);
            } else {
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
            }
        },

        deserilize(ps: JSScriptProperties, buffer: ByteBuffer) {
            return new Vector4(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
    "Rect": {
        typeid: 10,
        defaultValue: () => Rect.zero,
        serialize(ps: JSScriptProperties, buffer: ByteBuffer, value: Rect) {
            if (value instanceof Rect) {
                buffer.WriteSingle(value.x);
                buffer.WriteSingle(value.y);
                buffer.WriteSingle(value.width);
                buffer.WriteSingle(value.height);
            } else {
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
            }
        },

        deserilize(ps: JSScriptProperties, buffer: ByteBuffer) {
            return new Rect(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
    "Quaternion": {
        typeid: 11,
        defaultValue: () => Quaternion.identity,
        serialize(ps: JSScriptProperties, buffer: ByteBuffer, value: Quaternion) {
            if (value instanceof Quaternion) {
                buffer.WriteSingle(value.x);
                buffer.WriteSingle(value.y);
                buffer.WriteSingle(value.z);
                buffer.WriteSingle(value.w);
            } else {
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
                buffer.WriteSingle(1);
            }
        },

        deserilize(ps: JSScriptProperties, buffer: ByteBuffer) {
            return new Quaternion(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
    "object": {
        typeid: 12,
        defaultValue: null,
        serialize(ps: JSScriptProperties, buffer: ByteBuffer, value: Object) {
            if (value instanceof Object) {
                let index = ps.AddReferencedObject(value);
                buffer.WriteInt32(index);
            } else {
                if (!!value) {
                    console.error("only types inheriting UnityEngine.Object is unsupported", value);
                }
                buffer.WriteInt32(-1);
            }
        },

        deserilize(ps: JSScriptProperties, buffer: ByteBuffer) {
            let index = buffer.ReadInt32();
            return ps.GetReferencedObject(index);
        }
    },
}, true);
