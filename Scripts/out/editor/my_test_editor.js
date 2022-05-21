"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.MyTestEditorWindow = void 0;
const jsb_1 = require("jsb");
const System_1 = require("System");
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
const editor_decorators_1 = require("plover/editor/editor_decorators");
const Example_1 = require("Example");
function _onWindowGUI() { }
let MyTestEditorWindow = class MyTestEditorWindow extends UnityEditor_1.EditorWindow {
    constructor() {
        super(...arguments);
        this._testString = "";
        this._sel = 0;
        this._thisWindowRect = new UnityEngine_1.Rect(0, 0, 500, 100);
    }
    Awake() {
        jsb_1.AddCacheString("Test");
        jsb_1.AddCacheString("");
        this._scenes = System_1.Array.CreateInstance(System_1.String, 1);
        this._scenes.SetValue("Assets/Examples/Scenes/BasicRun.unity", 0);
    }
    OnEnable() {
        this.titleContent = new UnityEngine_1.GUIContent("Test EditorWindow");
    }
    OnGUI() {
        this._testString = UnityEditor_1.EditorGUILayout.TextField("Test", this._testString || "");
        this._sel = UnityEditor_1.EditorGUILayout.Popup("Pick", this._sel, ["1", "2", "3", "4"]);
        if (UnityEngine_1.GUILayout.Button("Test Delegate")) {
            // DelegateTest.UseDelegateInParameter(_onWindowGUI);
            Example_1.DelegateTest.UseDelegateInParameter(function () { });
        }
        if (UnityEngine_1.GUILayout.Button("Test Build")) {
            UnityEditor_1.BuildPipeline.BuildPlayer(this._scenes, "Build/macos.app", UnityEditor_1.BuildTarget.StandaloneOSX, UnityEditor_1.BuildOptions.Development);
        }
        let id = UnityEngine_1.GUIUtility.GetControlID(UnityEngine_1.FocusType.Passive);
        this._thisWindowRect = UnityEngine_1.GUILayout.Window(id, this._thisWindowRect, function () { }, "My JS Editor Window");
        UnityEditor_1.HandleUtility.AddDefaultControl(UnityEngine_1.GUIUtility.GetControlID(UnityEngine_1.FocusType.Passive));
    }
};
MyTestEditorWindow = __decorate([
    editor_decorators_1.ScriptEditorWindow()
], MyTestEditorWindow);
exports.MyTestEditorWindow = MyTestEditorWindow;
//# sourceMappingURL=my_test_editor.js.map