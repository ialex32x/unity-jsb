import { Editor, EditorGUI, EditorGUILayout, EditorUtility, MessageType, Undo } from "UnityEditor";
import { GUILayout, Object } from "UnityEngine";
import { MyClass } from "../../components/sample_monobehaviour";
import { DefaultEditor, ScriptEditor } from "plover/editor/editor_decorators";

@ScriptEditor(MyClass)
export class MyClassInspector extends DefaultEditor {
    Awake() {
        console.log("my class inspector class awake");
    }

    OnInspectorGUI() {
        let p = <MyClass>this.target;

        EditorGUILayout.HelpBox("WHY ARE YOU SO SERIOUS?", MessageType.Info);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Object", p.gameObject, Object, true);
        EditorGUI.EndDisabledGroup();
        super.OnInspectorGUI();
        if (GUILayout.Button("test")) {
            p.speak("hello");
        }
    }
}