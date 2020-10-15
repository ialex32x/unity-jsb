"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.MyEditorWindow = void 0;
const GUILayout = UnityEngine.GUILayout;
const EditorGUILayout = UnityEditor.EditorGUILayout;
const MessageType = UnityEditor.MessageType;
// @jsb.Shortcut("Window/JS/MyEditorWindow")
class MyEditorWindow extends UnityEditor.EditorWindow {
    Awake() {
        console.log("MyEditorWindow.Awake");
    }
    OnEnable() {
        this.titleContent = new UnityEngine.GUIContent("Blablabla");
    }
    OnGUI() {
        EditorGUILayout.HelpBox("Hello", MessageType.Info);
        if (GUILayout.Button("I am Javascript")) {
            console.log("Thanks");
        }
        if (GUILayout.Button("CreateWindow")) {
            UnityEditor.EditorWindow.CreateWindow(MyEditorWindow);
        }
    }
}
exports.MyEditorWindow = MyEditorWindow;
//# sourceMappingURL=my_editor_window.js.map