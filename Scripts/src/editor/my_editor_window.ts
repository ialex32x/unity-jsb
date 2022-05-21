import { DateTime } from "System";
import { EditorWindow, EditorGUILayout, MessageType, SceneView, Handles, GenericMenu, HandleUtility, EditorApplication, EditorGUI } from "UnityEditor";
import { FocusType, GUIContent, GUILayout, GUIUtility, Rect, Event, GUIStyle, GUI, Vector2, GUILayoutUtility, EventType, Vector3, Quaternion, Resources, ScriptableObject, Object, Color } from "UnityEngine";
import { ScriptEditorWindow } from "plover/editor/editor_decorators";
import { ScriptInteger, ScriptProperty, ScriptString } from "plover/runtime/class_decorators";
import * as jsb from "jsb";
import { EditorRuntime } from "jsb.editor";

class TempWindow extends EditorWindow {
    private _greeting = false;

    static show(rect: Rect, size: Vector2) {
        for (let w of Resources.FindObjectsOfTypeAll(TempWindow)) {
            w.Close();
            Object.DestroyImmediate(w);
        }

        let inst = ScriptableObject.CreateInstance(TempWindow);
        inst.ShowAsDropDown(GUIUtility.GUIToScreenRect(rect), size);
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
        if (GUILayout.Button("Hi")) {
            this._greeting = true;
            this.Repaint();
        }

        if (GUILayout.Button("Close")) {
            this.Close();
        }

        if (this._greeting) {
            EditorGUILayout.HelpBox("Hi, nice to meet you.", MessageType.Info);
        }
    }
}

@ScriptEditorWindow()
export class MyEditorWindow extends EditorWindow {
    private _onSceneGui: (sv: SceneView) => void;
    private _onMenuTest: () => void;
    private _onWindowGUI: () => void;
    private _styleWindowResize: GUIStyle;

    @ScriptProperty({ type: "Rect" })
    private _parentWindowRect = new Rect(0, 0, 0, 0);

    @ScriptProperty({ type: "Rect" })
    private _resizeStart = new Rect(0, 0, 0, 0);

    @ScriptProperty({ type: "Vector2" })
    private _minWindowSize = new Vector2(120, 100);

    @ScriptProperty({ type: "Rect" })
    private _thisWindowRect = new Rect(50, 50, 400, 300);

    // @ScriptProperty({ type: "object", serializable: false })
    private _resizerContent = new GUIContent("* ", "Resize");

    @ScriptProperty({ type: "bool" })
    private _isResizing = false;

    @ScriptProperty({ type: "int" })
    private _windowIndex = 0;

    @ScriptInteger()
    continuousInteger = 0;

    Awake() {
    }

    OnEnable() {
        this._onSceneGui = this.onSceneGui.bind(this);
        this._onMenuTest = this.onMenuTest.bind(this);
        this._onWindowGUI = this.onWindowGUI.bind(this);
        this.titleContent = new GUIContent("Blablabla0");
        SceneView.duringSceneGui("add", this._onSceneGui);
    }

    OnDisable() {
        console.log("on disable")
        SceneView.duringSceneGui("remove", this._onSceneGui);
        
        this._onSceneGui = null;
        this._onMenuTest = null;
        this._onWindowGUI = null;
    }

    OnDestroy() {
        console.log("on destroy")
        this._resizerContent = null;
    }

    onWindowGUI() {
        if (GUILayout.Button("Click")) {
            console.log("you clicked");
        }

        let mousePosition = Event.current.mousePosition;
        if (this._styleWindowResize == null) {
            this._styleWindowResize = GUI.skin.box;
        }

        let resizerRect = GUILayoutUtility.GetRect(this._resizerContent, this._styleWindowResize, GUILayout.ExpandWidth(false));

        resizerRect = new Rect(this._thisWindowRect.width - resizerRect.width, this._thisWindowRect.height - resizerRect.height, resizerRect.width, resizerRect.height);
        if (Event.current.type == EventType.MouseDown && resizerRect.Contains(mousePosition)) {
            this._isResizing = true;
            this._resizeStart = new Rect(mousePosition.x, mousePosition.y, this._thisWindowRect.width, this._thisWindowRect.height);
            //Event.current.Use();  // the GUI.Button below will eat the event, and this way it will show its active state
        } else if (Event.current.type == EventType.MouseUp && this._isResizing) {
            this._isResizing = false;
        } else if (Event.current.type != EventType.MouseDrag && Event.current.isMouse) {
            // if the mouse is over some other window we won't get an event, this just kind of circumvents that by checking the button state directly
            this._isResizing = false;
        } else if (this._isResizing) {
            // console.log("resizing");
            this._thisWindowRect.width = Math.max(this._minWindowSize.x, this._resizeStart.width + (mousePosition.x - this._resizeStart.x));
            this._thisWindowRect.height = Math.max(this._minWindowSize.y, this._resizeStart.height + (mousePosition.y - this._resizeStart.y));
            this._thisWindowRect.xMax = Math.min(this._parentWindowRect.width, this._thisWindowRect.xMax); // modifying xMax affects width, not x
            this._thisWindowRect.yMax = Math.min(this._parentWindowRect.height, this._thisWindowRect.yMax); // modifying yMax affects height, not y
        }

        GUI.Button(resizerRect, this._resizerContent, this._styleWindowResize);
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    }

    onSceneGui(sv: SceneView) {
        let id = GUIUtility.GetControlID(FocusType.Passive);
        this._parentWindowRect = sv.position;
        if (this._thisWindowRect.yMax > this._parentWindowRect.height) {
            this._thisWindowRect.yMax = this._parentWindowRect.height;
        }

        this._thisWindowRect = GUILayout.Window(id, this._thisWindowRect, this._onWindowGUI, "My JS Editor Window");
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    }

    private onMenuTest() {
        console.log("menu item test");
    }

    AddItemsToMenu(menu: GenericMenu) {
        menu.AddItem(new GUIContent("Test"), false, this._onMenuTest);
    }

    private _lastHour = -1;
    private _lastMinute = -1;
    private _lastSecond = -1;

    Update() {
        let d = DateTime.Now;
        if (this._lastSecond != d.Second) {
            this._lastSecond = d.Second;
            this._lastMinute = d.Minute;
            this._lastHour = d.Hour;
            this.Repaint();
        }
    }

    OnGUI() {
        EditorGUILayout.HelpBox("Hello", MessageType.Info);
        EditorGUILayout.LabelField("Prefs.sourceDir", EditorRuntime?.prefs?.sourceDir || "No sourceDir");
        EditorGUILayout.LabelField("TSConfig.outDir", EditorRuntime?.tsconfig?.compilerOptions?.outDir || "No outDir");
        EditorGUILayout.LabelField(typeof this.continuousInteger);
        this.continuousInteger = EditorGUILayout.IntField("ContinuousInteger", this.continuousInteger || 0) + 1;

        if (GUILayout.Button("I am Javascript")) {
            console.log("Thanks!", DateTime.Now);
        }

        let popRect = EditorGUILayout.GetControlRect();
        if (GUI.Button(popRect, "Pop A Temp Window")) {
            TempWindow.show(popRect, new Vector2(200, 200));
        }

        if (GUILayout.Button("CreateWindow")) {
            let child = EditorWindow.CreateWindow(MyEditorWindow, MyEditorWindow);
            if (child) {
                child._windowIndex = this._windowIndex + 1;
                child.titleContent = new GUIContent("Blablabla" + child._windowIndex);
            }
        }

        let w = this.position.width;
        let h = this.position.height;
        let center = new Vector3(w * 0.5, h * 0.5, 0);
        let rotSecond = Quaternion.Euler(0, 0, 360 * this._lastSecond / 60 + 180);
        let rotHour = Quaternion.Euler(0, 0, 360 * this._lastHour / 24 + 180);
        let rotMinute = Quaternion.Euler(0, 0, 360 * this._lastMinute / 60 + 180);
        let lastHandlesColor = Handles.color;
        Handles.color = Color.white;
        if (jsb.isOperatorOverloadingSupported) {
            //@ts-ignore
            Handles.DrawLine(center, center + rotSecond * new Vector3(0, 90, 0));
            //@ts-ignore
            Handles.DrawLine(center, center + rotMinute * new Vector3(0, 75, 0));
            //@ts-ignore
            Handles.DrawLine(center, center + rotHour * new Vector3(0, 60, 0));
        } else {
            Handles.DrawLine(center, Vector3.op_Addition(center, Quaternion.op_Multiply(rotSecond, new Vector3(0, 90, 0))));
            Handles.DrawLine(center, Vector3.op_Addition(center, Quaternion.op_Multiply(rotMinute, new Vector3(0, 75, 0))));
            Handles.DrawLine(center, Vector3.op_Addition(center, Quaternion.op_Multiply(rotHour, new Vector3(0, 60, 0))));
        }
        Handles.DrawWireDisc(center, Vector3.back, 100);
        Handles.color = lastHandlesColor;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.IntField(this._lastHour);
        EditorGUILayout.IntField(this._lastMinute);
        EditorGUILayout.IntField(this._lastSecond);
        EditorGUILayout.EndHorizontal();
    }
}
