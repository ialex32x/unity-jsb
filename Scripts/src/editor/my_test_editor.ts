import { AddCacheString } from "jsb";
import { Array, String } from "System";
import { BuildOptions, BuildPipeline, BuildTarget, EditorGUILayout, EditorWindow, HandleUtility } from "UnityEditor";
import { FocusType, GUIContent, GUILayout, GUIUtility, Rect } from "UnityEngine";
import { ScriptEditorWindow } from "plover/editor/editor_decorators";
import { DelegateTest } from "Example";

function _onWindowGUI() { }

@ScriptEditorWindow()
export class MyTestEditorWindow extends EditorWindow {
    private _testString = "";
    private _sel = 0;
    private _scenes: Array<string>;
    private _thisWindowRect = new Rect(0, 0, 500, 100);

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

        if (GUILayout.Button("Test Delegate")) {
            // DelegateTest.UseDelegateInParameter(_onWindowGUI);
            DelegateTest.UseDelegateInParameter(function () { });
        }

        if (GUILayout.Button("Test Build")) {
            BuildPipeline.BuildPlayer(this._scenes, "Build/macos.app", BuildTarget.StandaloneOSX, BuildOptions.Development);
        }

        let id = GUIUtility.GetControlID(FocusType.Passive);
        this._thisWindowRect = GUILayout.Window(id, this._thisWindowRect, function () { }, "My JS Editor Window");
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    }
}

