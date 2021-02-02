"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.RotateBehaviourInspector = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
class RotateBehaviourInspector extends UnityEditor_1.Editor {
    Awake() {
        console.log("my class inspector class awake");
    }
    OnInspectorGUI() {
        let p = this.target;
        UnityEditor_1.EditorGUILayout.ObjectField("Object", p.gameObject, UnityEngine_1.Object, true);
        p.rotationSpeed = UnityEditor_1.EditorGUILayout.FloatField("Rotation Speed", p.rotationSpeed);
        if (UnityEngine_1.GUILayout.Button("Reset")) {
            p.Reset();
        }
    }
}
exports.RotateBehaviourInspector = RotateBehaviourInspector;
//# sourceMappingURL=rotate_inspector.js.map