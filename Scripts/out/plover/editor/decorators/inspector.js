"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.SerializationUtil = exports.EditorUtil = exports.ScriptProperty = exports.ScriptObject = exports.ScriptString = exports.ScriptNumber = exports.ScriptInteger = exports.Inspector = exports.ScriptType = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
let SerializedFields = Symbol.for("SerializedFields");
let PropertiesTouched = Symbol.for("PropertiesTouched");
// expose this script class type to JSBehaviour, so you can put it on a prefab gameObject
function ScriptType(meta) {
    return function (target) {
        let OnBeforeSerialize = target.prototype["OnBeforeSerialize"];
        target.prototype["OnBeforeSerialize"] = function (ps) {
            this[PropertiesTouched] = false;
            if (typeof OnBeforeSerialize === "function") {
                OnBeforeSerialize.call(this, ps);
            }
            if (!this[PropertiesTouched]) {
                SerializationUtil.serialize(this, ps);
            }
        };
        let OnAfterDeserialize = target.prototype["OnAfterDeserialize"];
        target.prototype["OnAfterDeserialize"] = function (ps) {
            this[PropertiesTouched] = false;
            if (typeof OnAfterDeserialize === "function") {
                OnAfterDeserialize.call(this, ps);
            }
            if (!this[PropertiesTouched]) {
                SerializationUtil.deserialize(this, ps);
            }
        };
        return target;
    };
}
exports.ScriptType = ScriptType;
// path: 指定编辑器脚本所在模块, 暂时不支持相对路径
function Inspector(path, className) {
    return function (target) {
        // 暂时简单实现
        target.prototype.__editor__ = require(path)[className];
        return target;
    };
}
exports.Inspector = Inspector;
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
        let slots = target[SerializedFields];
        if (typeof slots === "undefined") {
            slots = target[SerializedFields] = {};
        }
        let slot = slots[propertyKey] = meta || {};
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
class EditorUtil {
    /**
     * 默认编辑器绘制行为
     */
    static draw(target, extra) {
        SerializationUtil.forEach(target, extra, (propertyKey, slot, self, extra) => {
            if (slot.visible) {
                let label = slot.label || propertyKey;
                let editablePE = !slot.editorOnly || !UnityEditor_1.EditorApplication.isPlaying;
                switch (slot.type) {
                    case "integer": {
                        let oldValue = self[propertyKey];
                        if (slot.editable && editablePE) {
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
                        if (slot.editable && editablePE) {
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
                        if (slot.editable && editablePE) {
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
                        if (slot.editable && editablePE) {
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
        });
    }
}
exports.EditorUtil = EditorUtil;
class SerializationUtil {
    static forEach(target, extra, cb) {
        let slots = target[SerializedFields];
        if (typeof slots !== "undefined") {
            for (let propertyKey in slots) {
                cb(propertyKey, slots[propertyKey], target, extra);
            }
        }
    }
    static serialize(target, ps) {
        target[PropertiesTouched] = true;
        this.forEach(target, ps, (propertyKey, slot, self, extra) => {
            if (slot.serializable) {
                let value = self[propertyKey];
                // console.log("serializing", slot.propertyKey, value);
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
        });
    }
    static deserialize(target, ps) {
        target[PropertiesTouched] = true;
        this.forEach(target, ps, (propertyKey, slot, self, extra) => {
            if (slot.serializable) {
                let value = null;
                switch (slot.type) {
                    case "integer": {
                        value = extra.GetInteger(slot.name);
                        break;
                    }
                    case "float": {
                        value = extra.GetNumber(slot.name);
                        break;
                    }
                    case "string": {
                        value = extra.GetString(slot.name);
                        break;
                    }
                    case "object": {
                        value = extra.GetObject(slot.name);
                        break;
                    }
                }
                self[propertyKey] = value;
            }
            // console.log("deserialize", slot.propertyKey, value);
        });
    }
}
exports.SerializationUtil = SerializationUtil;
//# sourceMappingURL=inspector.js.map