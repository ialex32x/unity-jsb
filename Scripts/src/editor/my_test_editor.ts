import { AddCacheString } from "jsb";
import { EditorGUILayout, EditorWindow } from "UnityEditor";
import { ScriptEditorWindow } from "../plover/editor/editor_decorators";

@ScriptEditorWindow()
export class MyTestEditorWindow extends EditorWindow {
    private _testString = "";

    Awake() {
        AddCacheString("Test");
        AddCacheString("");
    }

    OnGUI() {
        this._testString = EditorGUILayout.TextField("Test", this._testString) || "";
    }
}

