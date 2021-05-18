"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.As = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
exports.As = {
    "bool": {
        draw(self, prop, label, editablePE) {
            let propertyKey = prop.propertyKey;
            let oldValue = self[propertyKey];
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.Toggle(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    UnityEditor_1.EditorUtility.SetDirty(self);
                }
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.Toggle(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
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
        draw(self, prop, label, editablePE) {
            let propertyKey = prop.propertyKey;
            let oldValue = self[propertyKey];
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.FloatField(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    UnityEditor_1.EditorUtility.SetDirty(self);
                }
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.FloatField(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
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
        draw(self, prop, label, editablePE) {
            let propertyKey = prop.propertyKey;
            let oldValue = self[propertyKey];
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.FloatField(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    UnityEditor_1.EditorUtility.SetDirty(self);
                }
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.FloatField(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
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
        draw(self, prop, label, editablePE) {
            let propertyKey = prop.propertyKey;
            let oldValue = self[propertyKey];
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.Vector3Field(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    UnityEditor_1.EditorUtility.SetDirty(self);
                }
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.Vector3Field(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
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
        draw(self, prop, label, editablePE) {
            let propertyKey = prop.propertyKey;
            let oldValue = self[propertyKey];
            if (editablePE) {
                let newValue = UnityEditor_1.EditorGUILayout.Vector4Field(label, oldValue);
                if (newValue != oldValue) {
                    self[propertyKey] = newValue;
                    UnityEditor_1.EditorUtility.SetDirty(self);
                }
            }
            else {
                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                UnityEditor_1.EditorGUILayout.Vector4Field(label, oldValue);
                UnityEditor_1.EditorGUI.EndDisabledGroup();
            }
        },
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