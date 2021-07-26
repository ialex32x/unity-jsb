"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.Breadcrumb = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
const jsb = require("jsb");
const dispatcher_1 = require("../../../plover/events/dispatcher");
class Breadcrumb extends dispatcher_1.EventDispatcher {
    constructor() {
        super();
        this._cache = [];
        this._color = new UnityEngine_1.Color(1, 1, 1, 0);
        this._sv = UnityEngine_1.Vector2.zero;
        jsb.AddCacheString(">");
        this._height = UnityEditor_1.EditorGUIUtility.singleLineHeight + 14;
        this._heightOptionSV = UnityEngine_1.GUILayout.Height(this._height);
        this._heightOptionHB = UnityEngine_1.GUILayout.Height(this._height - 6);
    }
    get height() { return this._height; }
    draw(node) {
        if (!node || !node.parent) {
            return;
        }
        let count = 0;
        while (node.parent) {
            this._cache[count++] = node;
            node = node.parent;
        }
        this._sv = UnityEditor_1.EditorGUILayout.BeginScrollView(this._sv, this._heightOptionSV);
        UnityEngine_1.GUILayout.BeginHorizontal(this._heightOptionHB);
        let color = UnityEngine_1.GUI.backgroundColor;
        UnityEngine_1.GUI.backgroundColor = this._color;
        for (let i = count - 1; i >= 0; --i) {
            let item = this._cache[i];
            if (UnityEngine_1.GUILayout.Button(item.name, UnityEngine_1.GUILayout.ExpandWidth(false))) {
                this.dispatch(Breadcrumb.CLICKED, item, false);
            }
            if (i != 0) {
                // GUILayout.Label(">", GUILayout.ExpandWidth(false));
                if (UnityEngine_1.GUILayout.Button(">", UnityEngine_1.GUILayout.ExpandWidth(false))) {
                    this.dispatch(Breadcrumb.CLICKED, item, true);
                }
                // let rect = EditorGUILayout.GetControlRect(GUILayout.Width(10));
                // EditorGUI.DrawRect(rect, Color.yellow);
            }
            this._cache[i] = null;
        }
        UnityEngine_1.GUI.backgroundColor = color;
        UnityEngine_1.GUILayout.EndHorizontal();
        UnityEditor_1.EditorGUILayout.EndScrollView();
    }
}
exports.Breadcrumb = Breadcrumb;
Breadcrumb.CLICKED = "CLICKED";
//# sourceMappingURL=breadcrumb.js.map