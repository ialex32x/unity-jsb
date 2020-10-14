"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.MyEditorWindow = void 0;
const GUILayout = UnityEngine.GUILayout;
// @jsb.Shortcut("Window/JS/MyEditorWindow")
class MyEditorWindow extends UnityEditor.EditorWindow {
    Awake() {
        console.log("MyEditorWindow.Awake");
    }
    OnEnable() {
        this.titleContent = new UnityEngine.GUIContent("Blablabla");
    }
    OnGUI() {
        if (GUILayout.Button("I am Javascript")) {
            console.log("Thanks");
        }
    }
}
exports.MyEditorWindow = MyEditorWindow;
//# sourceMappingURL=my_editor_window.js.map