import { ByteBuffer } from "QuickJS.IO";
import { Quaternion, Vector3, Vector4 } from "UnityEngine";
import { PropertyTypeID } from "./class_decorators";

interface ISerializer {
    serialize(buffer: ByteBuffer, value: any): void;
    deserilize(buffer: ByteBuffer): any;
}

export class DefaultSerializer {
    static serialize(type: PropertyTypeID, buffer: ByteBuffer, value: any): void {
        let s: ISerializer = DefaultSerializers[type];
        if (typeof s === "object") {
            s.serialize(buffer, value);
        }
    }

    static deserilize(type: PropertyTypeID, buffer: ByteBuffer): any {
        let s: ISerializer = DefaultSerializers[type];
        
        if (typeof s === "object") {
            return s.deserilize(buffer);
        }

        return undefined;
    }
}

let DefaultSerializers = {
    "bool": {
        serialize(buffer: ByteBuffer, value: boolean) {
            buffer.WriteInt32(1);
            buffer.WriteBoolean(!!value);
        },

        deserilize(buffer: ByteBuffer) {
            let size = buffer.ReadInt32();
            console.assert(size == 1);
            return buffer.ReadBoolean();
        }
    },
    "float": {
        serialize(buffer: ByteBuffer, value: number) {
            buffer.WriteInt32(4);
            if (value) {
                buffer.WriteSingle(value);
            } else {
                buffer.WriteSingle(0);
            }
        },

        deserilize(buffer: ByteBuffer) {
            let size = buffer.ReadInt32();
            console.assert(size == 4);
            return buffer.ReadSingle();
        }
    },
    "double": {
        serialize(buffer: ByteBuffer, value: number) {
            buffer.WriteInt32(8);
            if (value) {
                buffer.WriteDouble(value);
            } else {
                buffer.WriteDouble(0);
            }
        },

        deserilize(buffer: ByteBuffer) {
            let size = buffer.ReadInt32();
            console.assert(size == 8);
            return buffer.ReadDouble();
        }
    },
    "Vector3": {
        serialize(buffer: ByteBuffer, value: Vector3) {
            buffer.WriteInt32(12);
            if (value) {
                buffer.WriteSingle(value.x);
                buffer.WriteSingle(value.y);
                buffer.WriteSingle(value.z);
            } else {
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
                buffer.WriteSingle(0);
            }
        },

        deserilize(buffer: ByteBuffer) {
            let size = buffer.ReadInt32();
            console.assert(size == 12);
            return new Vector3(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
        }
    },
    "Quaternion": {
        serialize(buffer: ByteBuffer, value: Quaternion) {
            buffer.WriteInt32(16);
            if (value) {
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

        deserilize(buffer: ByteBuffer) {
            let size = buffer.ReadInt32();
            console.assert(size == 16);
            return new Quaternion(buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
        }
    }
}
