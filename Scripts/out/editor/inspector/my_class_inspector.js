"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.MyClassInspector = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
class MyClassInspector extends UnityEditor_1.Editor {
    Awake() {
        console.log("my class inspector class awake");
    }
    OnInspectorGUI() {
        let p = this.target;
        UnityEditor_1.EditorGUILayout.HelpBox("WHY ARE YOU SO SERIOUS?", UnityEditor_1.MessageType.Info);
        UnityEditor_1.EditorGUILayout.IntField("vv", p.vv);
        if (UnityEngine_1.GUILayout.Button("test")) {
            p.speak("hello");
        }
    }
}
exports.MyClassInspector = MyClassInspector;
//# sourceMappingURL=my_class_inspector.js.map