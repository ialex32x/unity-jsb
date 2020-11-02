"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.MyEditorWindow = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
// @jsb.Shortcut("Window/JS/MyEditorWindow")
class MyEditorWindow extends UnityEditor_1.EditorWindow {
    Awake() {
        console.log("MyEditorWindow.Awake");
    }
    OnEnable() {
        this.titleContent = new UnityEngine_1.GUIContent("Blablabla");
    }
    OnGUI() {
        UnityEditor_1.EditorGUILayout.HelpBox("Hello", UnityEditor_1.MessageType.Info);
        if (UnityEngine_1.GUILayout.Button("I am Javascript")) {
            console.log("Thanks");
        }
        if (UnityEngine_1.GUILayout.Button("CreateWindow")) {
            UnityEditor_1.EditorWindow.CreateWindow(MyEditorWindow);
        }
    }
}
exports.MyEditorWindow = MyEditorWindow;
//# sourceMappingURL=my_editor_window.js.map