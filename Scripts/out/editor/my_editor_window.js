"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var MyEditorWindow_1;
Object.defineProperty(exports, "__esModule", { value: true });
exports.MyEditorWindow = void 0;
const System_1 = require("System");
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
const editor_decorators_1 = require("plover/editor/editor_decorators");
const class_decorators_1 = require("plover/runtime/class_decorators");
const jsb = require("jsb");
const jsb_editor_1 = require("jsb.editor");
class TempWindow extends UnityEditor_1.EditorWindow {
    constructor() {
        super(...arguments);
        this._greeting = false;
    }
    static show(rect, size) {
        for (let w of UnityEngine_1.Resources.FindObjectsOfTypeAll(TempWindow)) {
            w.Close();
            UnityEngine_1.Object.DestroyImmediate(w);
        }
        let inst = UnityEngine_1.ScriptableObject.CreateInstance(TempWindow);
        inst.ShowAsDropDown(UnityEngine_1.GUIUtility.GUIToScreenRect(rect), size);
    }
    Awake() {
        console.log("awake temp window");
    }
    OnDestroy() {
        console.log("destroy temp window");
    }
    OnEnable() {
        console.log("enable temp window");
    }
    OnDisable() {
        console.log("disable temp window");
    }
    OnGUI() {
        if (UnityEngine_1.GUILayout.Button("Hi")) {
            this._greeting = true;
            this.Repaint();
        }
        if (UnityEngine_1.GUILayout.Button("Close")) {
            this.Close();
        }
        if (this._greeting) {
            UnityEditor_1.EditorGUILayout.HelpBox("Hi, nice to meet you.", UnityEditor_1.MessageType.Info);
        }
    }
}
let MyEditorWindow = MyEditorWindow_1 = class MyEditorWindow extends UnityEditor_1.EditorWindow {
    constructor() {
        super(...arguments);
        this._parentWindowRect = new UnityEngine_1.Rect(0, 0, 0, 0);
        this._resizeStart = new UnityEngine_1.Rect(0, 0, 0, 0);
        this._minWindowSize = new UnityEngine_1.Vector2(120, 100);
        this._thisWindowRect = new UnityEngine_1.Rect(50, 50, 400, 300);
        this._resizerContent = new UnityEngine_1.GUIContent("* ", "Resize");
        this._isResizing = false;
        this._windowIndex = 0;
        this.continuousInteger = 0;
        this._lastHour = -1;
        this._lastMinute = -1;
        this._lastSecond = -1;
    }
    Awake() {
        this._onSceneGui = this.onSceneGui.bind(this);
        this._onMenuTest = this.onMenuTest.bind(this);
        this._onWindowGUI = this.onWindowGUI.bind(this);
    }
    OnEnable() {
        this.titleContent = new UnityEngine_1.GUIContent("Blablabla0");
        UnityEditor_1.SceneView.duringSceneGui("add", this._onSceneGui);
    }
    OnDisable() {
        UnityEditor_1.SceneView.duringSceneGui("remove", this._onSceneGui);
    }
    onWindowGUI() {
        if (UnityEngine_1.GUILayout.Button("Click")) {
            console.log("you clicked");
        }
        let mousePosition = UnityEngine_1.Event.current.mousePosition;
        if (this._styleWindowResize == null) {
            this._styleWindowResize = UnityEngine_1.GUI.skin.box;
        }
        let resizerRect = UnityEngine_1.GUILayoutUtility.GetRect(this._resizerContent, this._styleWindowResize, UnityEngine_1.GUILayout.ExpandWidth(false));
        resizerRect = new UnityEngine_1.Rect(this._thisWindowRect.width - resizerRect.width, this._thisWindowRect.height - resizerRect.height, resizerRect.width, resizerRect.height);
        if (UnityEngine_1.Event.current.type == UnityEngine_1.EventType.MouseDown && resizerRect.Contains(mousePosition)) {
            this._isResizing = true;
            this._resizeStart = new UnityEngine_1.Rect(mousePosition.x, mousePosition.y, this._thisWindowRect.width, this._thisWindowRect.height);
            //Event.current.Use();  // the GUI.Button below will eat the event, and this way it will show its active state
        }
        else if (UnityEngine_1.Event.current.type == UnityEngine_1.EventType.MouseUp && this._isResizing) {
            this._isResizing = false;
        }
        else if (UnityEngine_1.Event.current.type != UnityEngine_1.EventType.MouseDrag && UnityEngine_1.Event.current.isMouse) {
            // if the mouse is over some other window we won't get an event, this just kind of circumvents that by checking the button state directly
            this._isResizing = false;
        }
        else if (this._isResizing) {
            // console.log("resizing");
            this._thisWindowRect.width = Math.max(this._minWindowSize.x, this._resizeStart.width + (mousePosition.x - this._resizeStart.x));
            this._thisWindowRect.height = Math.max(this._minWindowSize.y, this._resizeStart.height + (mousePosition.y - this._resizeStart.y));
            this._thisWindowRect.xMax = Math.min(this._parentWindowRect.width, this._thisWindowRect.xMax); // modifying xMax affects width, not x
            this._thisWindowRect.yMax = Math.min(this._parentWindowRect.height, this._thisWindowRect.yMax); // modifying yMax affects height, not y
        }
        UnityEngine_1.GUI.Button(resizerRect, this._resizerContent, this._styleWindowResize);
        UnityEngine_1.GUI.DragWindow(new UnityEngine_1.Rect(0, 0, 10000, 10000));
    }
    onSceneGui(sv) {
        let id = UnityEngine_1.GUIUtility.GetControlID(UnityEngine_1.FocusType.Passive);
        this._parentWindowRect = sv.position;
        if (this._thisWindowRect.yMax > this._parentWindowRect.height) {
            this._thisWindowRect.yMax = this._parentWindowRect.height;
        }
        this._thisWindowRect = UnityEngine_1.GUILayout.Window(id, this._thisWindowRect, this._onWindowGUI, "My JS Editor Window");
        UnityEditor_1.HandleUtility.AddDefaultControl(UnityEngine_1.GUIUtility.GetControlID(UnityEngine_1.FocusType.Passive));
    }
    onMenuTest() {
        console.log("menu item test");
    }
    AddItemsToMenu(menu) {
        menu.AddItem(new UnityEngine_1.GUIContent("Test"), false, this._onMenuTest);
    }
    Update() {
        let d = System_1.DateTime.Now;
        if (this._lastSecond != d.Second) {
            this._lastSecond = d.Second;
            this._lastMinute = d.Minute;
            this._lastHour = d.Hour;
            this.Repaint();
        }
    }
    OnGUI() {
        var _a, _b, _c;
        UnityEditor_1.EditorGUILayout.HelpBox("Hello", UnityEditor_1.MessageType.Info);
        UnityEditor_1.EditorGUILayout.LabelField("Prefs.sourceDir", ((_a = jsb_editor_1.EditorRuntime === null || jsb_editor_1.EditorRuntime === void 0 ? void 0 : jsb_editor_1.EditorRuntime.prefs) === null || _a === void 0 ? void 0 : _a.sourceDir) || "No sourceDir");
        UnityEditor_1.EditorGUILayout.LabelField("TSConfig.outDir", ((_c = (_b = jsb_editor_1.EditorRuntime === null || jsb_editor_1.EditorRuntime === void 0 ? void 0 : jsb_editor_1.EditorRuntime.tsconfig) === null || _b === void 0 ? void 0 : _b.compilerOptions) === null || _c === void 0 ? void 0 : _c.outDir) || "No outDir");
        UnityEditor_1.EditorGUILayout.LabelField(typeof this.continuousInteger);
        this.continuousInteger = UnityEditor_1.EditorGUILayout.IntField("ContinuousInteger", this.continuousInteger || 0) + 1;
        if (UnityEngine_1.GUILayout.Button("I am Javascript")) {
            console.log("Thanks!", System_1.DateTime.Now);
        }
        let popRect = UnityEditor_1.EditorGUILayout.GetControlRect();
        if (UnityEngine_1.GUI.Button(popRect, "Pop A Temp Window")) {
            TempWindow.show(popRect, new UnityEngine_1.Vector2(200, 200));
        }
        if (UnityEngine_1.GUILayout.Button("CreateWindow")) {
            let child = UnityEditor_1.EditorWindow.CreateWindow(MyEditorWindow_1, MyEditorWindow_1);
            if (child) {
                child._windowIndex = this._windowIndex + 1;
                child.titleContent = new UnityEngine_1.GUIContent("Blablabla" + child._windowIndex);
            }
        }
        let w = this.position.width;
        let h = this.position.height;
        let center = new UnityEngine_1.Vector3(w * 0.5, h * 0.5, 0);
        let rotSecond = UnityEngine_1.Quaternion.Euler(0, 0, 360 * this._lastSecond / 60 + 180);
        let rotHour = UnityEngine_1.Quaternion.Euler(0, 0, 360 * this._lastHour / 24 + 180);
        let rotMinute = UnityEngine_1.Quaternion.Euler(0, 0, 360 * this._lastMinute / 60 + 180);
        let lastHandlesColor = UnityEditor_1.Handles.color;
        UnityEditor_1.Handles.color = UnityEngine_1.Color.white;
        if (jsb.isOperatorOverloadingSupported) {
            //@ts-ignore
            UnityEditor_1.Handles.DrawLine(center, center + rotSecond * new UnityEngine_1.Vector3(0, 90, 0));
            //@ts-ignore
            UnityEditor_1.Handles.DrawLine(center, center + rotMinute * new UnityEngine_1.Vector3(0, 75, 0));
            //@ts-ignore
            UnityEditor_1.Handles.DrawLine(center, center + rotHour * new UnityEngine_1.Vector3(0, 60, 0));
        }
        else {
            UnityEditor_1.Handles.DrawLine(center, UnityEngine_1.Vector3.op_Addition(center, UnityEngine_1.Quaternion.op_Multiply(rotSecond, new UnityEngine_1.Vector3(0, 90, 0))));
            UnityEditor_1.Handles.DrawLine(center, UnityEngine_1.Vector3.op_Addition(center, UnityEngine_1.Quaternion.op_Multiply(rotMinute, new UnityEngine_1.Vector3(0, 75, 0))));
            UnityEditor_1.Handles.DrawLine(center, UnityEngine_1.Vector3.op_Addition(center, UnityEngine_1.Quaternion.op_Multiply(rotHour, new UnityEngine_1.Vector3(0, 60, 0))));
        }
        UnityEditor_1.Handles.DrawWireDisc(center, UnityEngine_1.Vector3.back, 100);
        UnityEditor_1.Handles.color = lastHandlesColor;
        UnityEditor_1.EditorGUILayout.BeginHorizontal();
        UnityEditor_1.EditorGUILayout.IntField(this._lastHour);
        UnityEditor_1.EditorGUILayout.IntField(this._lastMinute);
        UnityEditor_1.EditorGUILayout.IntField(this._lastSecond);
        UnityEditor_1.EditorGUILayout.EndHorizontal();
    }
};
__decorate([
    class_decorators_1.ScriptProperty({ type: "Rect" })
], MyEditorWindow.prototype, "_parentWindowRect", void 0);
__decorate([
    class_decorators_1.ScriptProperty({ type: "Rect" })
], MyEditorWindow.prototype, "_resizeStart", void 0);
__decorate([
    class_decorators_1.ScriptProperty({ type: "Vector2" })
], MyEditorWindow.prototype, "_minWindowSize", void 0);
__decorate([
    class_decorators_1.ScriptProperty({ type: "Rect" })
], MyEditorWindow.prototype, "_thisWindowRect", void 0);
__decorate([
    class_decorators_1.ScriptProperty({ type: "object" })
], MyEditorWindow.prototype, "_resizerContent", void 0);
__decorate([
    class_decorators_1.ScriptProperty({ type: "bool" })
], MyEditorWindow.prototype, "_isResizing", void 0);
__decorate([
    class_decorators_1.ScriptProperty({ type: "int" })
], MyEditorWindow.prototype, "_windowIndex", void 0);
__decorate([
    class_decorators_1.ScriptInteger()
], MyEditorWindow.prototype, "continuousInteger", void 0);
MyEditorWindow = MyEditorWindow_1 = __decorate([
    editor_decorators_1.ScriptEditorWindow()
], MyEditorWindow);
exports.MyEditorWindow = MyEditorWindow;
//# sourceMappingURL=my_editor_window.js.map