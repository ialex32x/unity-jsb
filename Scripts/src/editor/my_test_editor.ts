import { AddCacheString } from "jsb";
import { EditorGUILayout, EditorWindow } from "UnityEditor";
import { GUILayout } from "UnityEngine";
import { ScriptEditorWindow } from "../plover/editor/editor_decorators";

@ScriptEditorWindow()
export class MyTestEditorWindow extends EditorWindow {
    private _testString = "";
    private _sel = 0;

    Awake() {
        AddCacheString("Test");
        AddCacheString("");
    }

    OnGUI() {
        this._testString = EditorGUILayout.TextField("Test", this._testString || "");
        this._sel = EditorGUILayout.Popup(<any>"A", <any>this._sel, <any>["1", "2", "3"]);
    }
}

