import { Editor, EditorGUILayout, MessageType } from "UnityEditor";
import { GUILayout, Object } from "UnityEngine";
import { MyClass } from "../../example_monobehaviour";

export class MyClassInspector extends Editor {
    Awake() {
        console.log("my class inspector class awake");
    }

    OnInspectorGUI() {
        let p = <MyClass>this.target;

        EditorGUILayout.HelpBox("WHY ARE YOU SO SERIOUS?", MessageType.Info);
        EditorGUILayout.ObjectField("Object", p.gameObject, Object, true);
        EditorGUILayout.IntField("vv", p.vv);
        if (GUILayout.Button("test")) {
            p.speak("hello");
        }
    }
}