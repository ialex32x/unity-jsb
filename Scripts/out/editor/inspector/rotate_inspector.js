"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.RotateBehaviourInspector = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
const sample_monobehaviour_1 = require("../../components/sample_monobehaviour");
const editor_decorators_1 = require("plover/editor/editor_decorators");
let RotateBehaviourInspector = class RotateBehaviourInspector extends UnityEditor_1.Editor {
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
};
RotateBehaviourInspector = __decorate([
    editor_decorators_1.ScriptEditor(sample_monobehaviour_1.RotateBehaviour)
], RotateBehaviourInspector);
exports.RotateBehaviourInspector = RotateBehaviourInspector;
//# sourceMappingURL=rotate_inspector.js.map