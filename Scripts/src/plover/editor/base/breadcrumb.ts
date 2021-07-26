import { EditorGUI, EditorGUILayout, EditorGUIUtility } from "UnityEditor";
import { Color, GUI, GUILayout, GUILayoutOption, GUIStyle, Vector2 } from "UnityEngine";
import { UTreeNode } from "./treenode";
import * as jsb from "jsb";
import { EventDispatcher } from "../../../plover/events/dispatcher";

export class Breadcrumb extends EventDispatcher {
    static readonly CLICKED = "CLICKED";

    private _height: number;
    private _heightOptionSV: GUILayoutOption;
    private _heightOptionHB: GUILayoutOption;
    private _cache: Array<UTreeNode> = [];
    private _color: Color = new Color(1, 1, 1, 0);
    private _sv = Vector2.zero;

    get height() { return this._height; }

    constructor() {
        super();

        jsb.AddCacheString(">");
        this._height = EditorGUIUtility.singleLineHeight + 14;
        this._heightOptionSV = GUILayout.Height(this._height);
        this._heightOptionHB = GUILayout.Height(this._height - 6);
    }

    draw(node: UTreeNode) {
        if (!node || !node.parent) {
            return;
        }

        let count = 0;
        while (node.parent) {
            this._cache[count++] = node;
            node = node.parent;
        }

        this._sv = EditorGUILayout.BeginScrollView(this._sv, this._heightOptionSV);
        GUILayout.BeginHorizontal(this._heightOptionHB);
        let color = GUI.backgroundColor;
        GUI.backgroundColor = this._color;
        for (let i = count - 1; i >= 0; --i) {
            let item = this._cache[i];

            if (GUILayout.Button(item.name, GUILayout.ExpandWidth(false))) {
                this.dispatch(Breadcrumb.CLICKED, item, false);
            }

            if (i != 0) {
                // GUILayout.Label(">", GUILayout.ExpandWidth(false));
                if (GUILayout.Button(">", GUILayout.ExpandWidth(false))) {
                    this.dispatch(Breadcrumb.CLICKED, item, true);
                }
                // let rect = EditorGUILayout.GetControlRect(GUILayout.Width(10));
                // EditorGUI.DrawRect(rect, Color.yellow);
            }
            this._cache[i] = null;
        }
        GUI.backgroundColor = color;
        GUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }
}
