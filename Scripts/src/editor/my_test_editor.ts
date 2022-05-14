import { AddCacheString } from "jsb";
import { Array, String } from "System";
import { BuildOptions, BuildPipeline, BuildTarget, EditorGUILayout, EditorWindow } from "UnityEditor";
import { GUIContent, GUILayout } from "UnityEngine";
import { ScriptEditorWindow } from "plover/editor/editor_decorators";

@ScriptEditorWindow()
export class MyTestEditorWindow extends EditorWindow {
    private _testString = "";
    private _sel = 0;
    private _scenes: Array<string>;

    Awake() {
        AddCacheString("Test");
        AddCacheString("");
        this._scenes = Array.CreateInstance(String, 1);
        this._scenes.SetValue("Assets/Examples/Scenes/BasicRun.unity", 0);
    }

    OnEnable() {
        this.titleContent = new GUIContent("Test EditorWindow");
    }

    OnGUI() {
        this._testString = EditorGUILayout.TextField("Test", this._testString || "");
        this._sel = EditorGUILayout.Popup(<any>"Pick", <any>this._sel, <any>["1", "2", "3", "4"]);

        if (GUILayout.Button("Test Build")) {
            BuildPipeline.BuildPlayer(this._scenes, "Build/macos.app", BuildTarget.StandaloneOSX, BuildOptions.Development);
        }
    }
}

