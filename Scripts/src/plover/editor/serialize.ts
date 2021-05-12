import { ByteBuffer } from "QuickJS.IO";
import { EditorGUI, EditorGUILayout, EditorUtility } from "UnityEditor";
import { Quaternion, Vector3, Vector4 } from "UnityEngine";
import { PropertyMetaInfo } from "./editor_decorators";

export interface ValueTypeSerializer {
    serialize(buffer: ByteBuffer, value: any): void;
    deserilize(buffer: ByteBuffer): any;
    draw(target: any, prop: PropertyMetaInfo, label: string, editablePE: boolean): void;
}

export let As = {
    "bool": {
        draw(self: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let propertyKey = prop.propertyKey;
            let oldValue: boolean = self[propertyKey];
            if (editablePE) {
                let newValue = EditorGUILayout.Toggle(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    EditorUtility.SetDirty(self);
                }
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
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
        draw(self: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let propertyKey = prop.propertyKey;
            let oldValue: number = self[propertyKey];
            if (editablePE) {
                let newValue = EditorGUILayout.FloatField(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    EditorUtility.SetDirty(self);
                }
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
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
        draw(self: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let propertyKey = prop.propertyKey;
            let oldValue: number = self[propertyKey];
            if (editablePE) {
                let newValue = EditorGUILayout.FloatField(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    EditorUtility.SetDirty(self);
                }
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
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
        draw(self: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let propertyKey = prop.propertyKey;
            let oldValue: Vector3 = self[propertyKey];
            if (editablePE) {
                let newValue = EditorGUILayout.Vector3Field(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    EditorUtility.SetDirty(self);
                }
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector3Field(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
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
        draw(self: any, prop: PropertyMetaInfo, label: string, editablePE: boolean) {
            let propertyKey = prop.propertyKey;
            let oldValue: Vector4 = self[propertyKey];
            if (editablePE) {
                let newValue = EditorGUILayout.Vector4Field(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    EditorUtility.SetDirty(self);
                }
            } else {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector4Field(label, oldValue);
                EditorGUI.EndDisabledGroup();
            }
        },
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
