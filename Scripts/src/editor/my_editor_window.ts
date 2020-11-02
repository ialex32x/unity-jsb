import { EditorWindow, EditorGUILayout, MessageType } from "UnityEditor";
import { GUIContent, GUILayout } from "UnityEngine";

// @jsb.Shortcut("Window/JS/MyEditorWindow")
export class MyEditorWindow extends EditorWindow {
    Awake() {
        console.log("MyEditorWindow.Awake");
    }

    OnEnable() {
        this.titleContent = new GUIContent("Blablabla");
    }

    OnGUI() {
        EditorGUILayout.HelpBox("Hello", MessageType.Info);
        if (GUILayout.Button("I am Javascript")) {
            console.log("Thanks");
        }

        if (GUILayout.Button("CreateWindow")) {
            EditorWindow.CreateWindow(MyEditorWindow);
        }
    }
}
