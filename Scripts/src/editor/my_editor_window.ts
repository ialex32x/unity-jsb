
const GUILayout = UnityEngine.GUILayout;

// @jsb.Shortcut("Window/JS/MyEditorWindow")
export class MyEditorWindow extends UnityEditor.EditorWindow {
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
