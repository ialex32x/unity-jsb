import { EditorWindow, EditorGUILayout, MessageType, SceneView, Handles, GenericMenu, HandleUtility } from "UnityEditor";
import { FocusType, GUIContent, GUILayout, GUIUtility, Rect, Event, GUIStyle, GUI, Vector2, GUILayoutUtility, EventType } from "UnityEngine";

// @jsb.Shortcut("Window/JS/MyEditorWindow")
export class MyEditorWindow extends EditorWindow {
    private _onSceneGui: (sv: SceneView) => void;
    private _onMenuTest: () => void;
    private _onWindowGUI: () => void;
    private _styleWindowResize: GUIStyle;

    private _parentWindowRect = new Rect(0, 0, 0, 0);
    private _resizeStart = new Rect(0, 0, 0, 0);
    private _minWindowSize = new Vector2(120, 100);
    private _thisWindowRect = new Rect(50, 50, 400, 300);
    private _resizerContent = new GUIContent("* ", "Resize");
    private _isResizing = false;

    Awake() {
        this._onSceneGui = this.onSceneGui.bind(this);
        this._onMenuTest = this.onMenuTest.bind(this);
        this._onWindowGUI = this.onWindowGUI.bind(this);
    }

    OnEnable() {
        this.titleContent = new GUIContent("Blablabla6");
        SceneView.duringSceneGui("add", this._onSceneGui);
    }

    OnDisable() {
        SceneView.duringSceneGui("remove", this._onSceneGui);
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
        if (Event.current.type == EventType.mouseDown && resizerRect.Contains(mousePosition)) {
            this._isResizing = true;
            this._resizeStart = new Rect(mousePosition.x, mousePosition.y, this._thisWindowRect.width, this._thisWindowRect.height);
            //Event.current.Use();  // the GUI.Button below will eat the event, and this way it will show its active state
        } else if (Event.current.type == EventType.mouseUp && this._isResizing) {
            this._isResizing = false;
        } else if (Event.current.type != EventType.mouseDrag && Event.current.isMouse) {
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

    OnGUI() {
        EditorGUILayout.HelpBox("Hello", MessageType.Info);
        if (GUILayout.Button("I am Javascript")) {
            console.log("Thanks!");
        }

        if (GUILayout.Button("CreateWindow")) {
            EditorWindow.CreateWindow(MyEditorWindow);
        }
    }
}
