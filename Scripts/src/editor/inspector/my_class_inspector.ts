import { Editor, EditorGUI, EditorGUILayout, EditorUtility, MessageType } from "UnityEditor";
import { GUILayout, Object } from "UnityEngine";
import { MyClass } from "../../example_monobehaviour";
import { ScriptEditor } from "../../plover/editor/decorators/inspector";

@ScriptEditor(MyClass)
export class MyClassInspector extends Editor {
    Awake() {
        console.log("my class inspector class awake");
    }

    OnInspectorGUI() {
        let p = <MyClass>this.target;

        EditorGUILayout.HelpBox("WHY ARE YOU SO SERIOUS?", MessageType.Info);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Object", p.gameObject, Object, true);
        EditorGUI.EndDisabledGroup();
        let vv = EditorGUILayout.IntField("vv", p.vv);
        if (vv != p.vv) {
            p.vv = vv;
            // console.log("write value", p.vv);
            EditorUtility.SetDirty(p);
        }
        if (GUILayout.Button("test")) {
            p.speak("hello");
        }
    }
}