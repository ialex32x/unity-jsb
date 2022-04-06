"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.MyClassInspector = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
const sample_monobehaviour_1 = require("../../components/sample_monobehaviour");
const editor_decorators_1 = require("plover/editor/editor_decorators");
let MyClassInspector = class MyClassInspector extends editor_decorators_1.DefaultEditor {
    Awake() {
        console.log("my class inspector class awake");
    }
    OnInspectorGUI() {
        let p = this.target;
        UnityEditor_1.EditorGUILayout.HelpBox("WHY ARE YOU SO SERIOUS?", UnityEditor_1.MessageType.Info);
        UnityEditor_1.EditorGUI.BeginDisabledGroup(true);
        UnityEditor_1.EditorGUILayout.ObjectField("Object", p.gameObject, UnityEngine_1.Object, true);
        UnityEditor_1.EditorGUI.EndDisabledGroup();
        super.OnInspectorGUI();
        if (UnityEngine_1.GUILayout.Button("test")) {
            p.speak("hello");
        }
    }
};
MyClassInspector = __decorate([
    editor_decorators_1.ScriptEditor(sample_monobehaviour_1.MyClass)
], MyClassInspector);
exports.MyClassInspector = MyClassInspector;
//# sourceMappingURL=my_class_inspector.js.map