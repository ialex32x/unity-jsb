"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.SerializationUtil = exports.EditorUtil = exports.DefaultEditor = exports.ScriptProperty = exports.ScriptObject = exports.ScriptString = exports.ScriptNumber = exports.ScriptInteger = exports.ScriptEditor = exports.ScriptType = exports.ScriptAsset = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
let Symbol_SerializedFields = Symbol.for("SerializedFields");
let Symbol_PropertiesTouched = Symbol.for("PropertiesTouched");
let Symbol_CustomEditor = Symbol.for("CustomEditor");
function ScriptAsset(meta) {
    return ScriptType(meta);
}
exports.ScriptAsset = ScriptAsset;
// expose this script class type to JSBehaviour, so you can put it on a prefab gameObject
function ScriptType(meta) {
    return function (target) {
        let OnBeforeSerialize = target.prototype["OnBeforeSerialize"];
        target.prototype["OnBeforeSerialize"] = function (ps, buffer) {
            this[Symbol_PropertiesTouched] = false;
            if (typeof OnBeforeSerialize === "function") {
                OnBeforeSerialize.call(this, ps, buffer);
            }
            if (!this[Symbol_PropertiesTouched]) {
                SerializationUtil.serialize(this, ps, buffer);
            }
        };
        let OnAfterDeserialize = target.prototype["OnAfterDeserialize"];
        target.prototype["OnAfterDeserialize"] = function (ps, buffer) {
            this[Symbol_PropertiesTouched] = false;
            if (typeof OnAfterDeserialize === "function") {
                OnAfterDeserialize.call(this, ps, buffer);
            }
            if (!this[Symbol_PropertiesTouched]) {
                SerializationUtil.deserialize(this, ps, buffer);
            }
        };
        return target;
    };
}
exports.ScriptType = ScriptType;
function ScriptEditor(forType) {
    return function (editorType) {
        forType.prototype[Symbol_CustomEditor] = editorType;
        return editorType;
    };
}
exports.ScriptEditor = ScriptEditor;
function ScriptInteger(meta) {
    if (typeof meta === "undefined") {
        meta = { type: "integer" };
    }
    else {
        meta.type = "integer";
    }
    return ScriptProperty(meta);
}
exports.ScriptInteger = ScriptInteger;
function ScriptNumber(meta) {
    if (typeof meta === "undefined") {
        meta = { type: "float" };
    }
    else {
        meta.type = "float";
    }
    return ScriptProperty(meta);
}
exports.ScriptNumber = ScriptNumber;
function ScriptString(meta) {
    if (typeof meta === "undefined") {
        meta = { type: "string" };
    }
    else {
        meta.type = "string";
    }
    return ScriptProperty(meta);
}
exports.ScriptString = ScriptString;
function ScriptObject(meta) {
    if (typeof meta === "undefined") {
        meta = { type: "object" };
    }
    else {
        meta.type = "object";
    }
    return ScriptProperty(meta);
}
exports.ScriptObject = ScriptObject;
function ScriptProperty(meta) {
    return function (target, propertyKey) {
        let slots = target[Symbol_SerializedFields];
        if (typeof slots === "undefined") {
            slots = target[Symbol_SerializedFields] = {};
        }
        let slot = slots[propertyKey] = meta || {};
        slot.propertyKey = propertyKey;
        if (typeof slot.serializable !== "boolean") {
            slot.serializable = true;
        }
        if (typeof slot.editable !== "boolean") {
            slot.editable = true;
        }
        if (typeof slot.visible !== "boolean") {
            slot.visible = true;
        }
        if (typeof slot.name !== "string") {
            slot.name = propertyKey;
        }
    };
}
exports.ScriptProperty = ScriptProperty;
class DefaultEditor extends UnityEditor_1.Editor {
    OnInspectorGUI() {
        EditorUtil.draw(this.target);
    }
}
exports.DefaultEditor = DefaultEditor;
class EditorUtil {
    static getCustomEditor(forType) {
        return forType[Symbol_CustomEditor] || DefaultEditor;
    }
    /**
     * 默认编辑器绘制行为
     */
    static draw(target, extra) {
        SerializationUtil.forEach(target, extra, (propertyKey, slot, self, extra) => {
            if (slot.visible) {
                let label = slot.label || propertyKey;
                let editablePE = slot.editable && (!slot.editorOnly || !UnityEditor_1.EditorApplication.isPlaying);
                if (typeof slot.type === "object") {
                    if (typeof slot.type.draw === "function") {
                        slot.type.draw(target, slot, label, editablePE);
                    }
                    else {
                        UnityEditor_1.EditorGUILayout.LabelField(label);
                        UnityEditor_1.EditorGUILayout.HelpBox("no draw operation for this type", UnityEditor_1.MessageType.Warning);
                    }
                }
                else if (typeof slot.type === "string") {
                    switch (slot.type) {
                        case "integer": {
                            let oldValue = self[propertyKey];
                            if (editablePE) {
                                let newValue = UnityEditor_1.EditorGUILayout.IntField(label, oldValue);
                                if (newValue != oldValue) {
                                    self[propertyKey] = newValue;
                                    UnityEditor_1.EditorUtility.SetDirty(self);
                                }
                            }
                            else {
                                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                                UnityEditor_1.EditorGUILayout.IntField(label, oldValue);
                                UnityEditor_1.EditorGUI.EndDisabledGroup();
                            }
                            break;
                        }
                        case "float": {
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
                            break;
                        }
                        case "string": {
                            let oldValue = self[propertyKey];
                            if (typeof oldValue !== "string") {
                                oldValue = "" + oldValue;
                            }
                            if (editablePE) {
                                let newValue = UnityEditor_1.EditorGUILayout.TextField(label, oldValue);
                                if (newValue != oldValue) {
                                    self[propertyKey] = newValue;
                                    UnityEditor_1.EditorUtility.SetDirty(self);
                                }
                            }
                            else {
                                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                                UnityEditor_1.EditorGUILayout.TextField(label, oldValue);
                                UnityEditor_1.EditorGUI.EndDisabledGroup();
                            }
                            break;
                        }
                        case "object": {
                            let oldValue = self[propertyKey];
                            if (typeof oldValue !== "object") {
                                oldValue = null;
                            }
                            if (editablePE) {
                                let allowSceneObjects = slot.extra && slot.extra.allowSceneObjects;
                                let newValue = UnityEditor_1.EditorGUILayout.ObjectField(label, oldValue, slot.extra && slot.extra.type || UnityEngine_1.Object, typeof allowSceneObjects === "boolean" ? allowSceneObjects : true);
                                if (newValue != oldValue) {
                                    self[propertyKey] = newValue;
                                    UnityEditor_1.EditorUtility.SetDirty(self);
                                }
                            }
                            else {
                                UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
                                UnityEditor_1.EditorGUILayout.ObjectField(label, oldValue, UnityEngine_1.Object, false);
                                UnityEditor_1.EditorGUI.EndDisabledGroup();
                            }
                            break;
                        }
                    }
                }
                else {
                    UnityEditor_1.EditorGUILayout.LabelField(label);
                    UnityEditor_1.EditorGUILayout.HelpBox("unsupported type", UnityEditor_1.MessageType.Warning);
                }
            }
        });
    }
}
exports.EditorUtil = EditorUtil;
class SerializationUtil {
    static forEach(target, extra, cb) {
        let slots = target[Symbol_SerializedFields];
        if (typeof slots !== "undefined") {
            for (let propertyKey in slots) {
                cb(propertyKey, slots[propertyKey], target, extra);
            }
        }
    }
    // 当不需要默认行为时, 调用此函数将序列化状态标记为已完成, 以便跳过默认的 serialize/deserialize 行为
    static markAsReady(target) {
        target[Symbol_PropertiesTouched] = true;
    }
    static serialize(target, ps, buffer) {
        this.markAsReady(target);
        this.forEach(target, ps, (propertyKey, slot, self, extra) => {
            if (slot.serializable) {
                let value = self[propertyKey];
                // console.log("serializing", propertyKey, value);
                if (typeof slot.type === "object") {
                    buffer.WriteString(slot.name);
                    slot.type.serialize(buffer, value);
                }
                else {
                    switch (slot.type) {
                        case "integer": {
                            extra.SetInteger(slot.name, typeof value === "number" ? value : 0);
                            break;
                        }
                        case "float": {
                            extra.SetNumber(slot.name, typeof value === "number" ? value : 0);
                            break;
                        }
                        case "string": {
                            extra.SetString(slot.name, value);
                            break;
                        }
                        case "object": {
                            extra.SetObject(slot.name, value);
                            break;
                        }
                    }
                }
            }
        });
    }
    static deserialize(target, ps, buffer) {
        this.markAsReady(target);
        let slots = target[Symbol_SerializedFields];
        if (typeof slots !== "undefined") {
            let slotByName = {};
            for (let propertyKey in slots) {
                let slot = slots[propertyKey];
                if (slot.serializable) {
                    if (typeof slot.type === "object") {
                        slotByName[slot.name] = slot;
                    }
                    else {
                        let value = null;
                        switch (slot.type) {
                            case "integer": {
                                value = ps.GetInteger(slot.name);
                                break;
                            }
                            case "float": {
                                value = ps.GetNumber(slot.name);
                                break;
                            }
                            case "string": {
                                value = ps.GetString(slot.name);
                                break;
                            }
                            case "object": {
                                value = ps.GetObject(slot.name);
                                break;
                            }
                        }
                        target[propertyKey] = value;
                    }
                    // console.log("deserialize", propertyKey, value);
                }
            }
            while (buffer.readableBytes > 0) {
                let name = buffer.ReadString();
                let slot = slotByName[name];
                if (slot) {
                    target[slot.propertyKey] = slot.type.deserilize(buffer);
                }
                else {
                    let size = buffer.ReadInt32();
                    buffer.ReadBytes(size);
                }
            }
        }
    }
}
exports.SerializationUtil = SerializationUtil;
//# sourceMappingURL=editor_decorators.js.map