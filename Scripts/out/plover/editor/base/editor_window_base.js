"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.EditorWindowBase = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
const menu_builder_1 = require("../base/menu_builder");
const splitview_1 = require("../base/splitview");
const treenode_1 = require("../base/treenode");
const treeview_1 = require("../base/treeview");
const jsb = require("jsb");
const string_utils_1 = require("../../text/string_utils");
const breadcrumb_1 = require("../base/breadcrumb");
class EditorWindowBase extends UnityEditor_1.EditorWindow {
    constructor() {
        super(...arguments);
        this._treeViewScroll = UnityEngine_1.Vector2.zero;
        this._toolbarRect = UnityEngine_1.Rect.zero;
        this._leftRect = UnityEngine_1.Rect.zero;
        this._rightRect = UnityEngine_1.Rect.zero;
        // protected _topSplitLine = Rect.zero;
        this._searchLabel = new UnityEngine_1.GUIContent("Search");
        this._tempRect = UnityEngine_1.Rect.zero;
        this._contents = {};
        this.toobarHeight = 24;
    }
    onTreeNodeNameEditEnded(node, newName) {
        if (node.isRoot || !node.data) {
            return;
        }
        node.name = newName;
    }
    onTreeNodeNameChanged(node, oldName) {
        if (node.isRoot || !node.data) {
            return;
        }
    }
    onTreeNodeCreated(node) {
        if (node.data) {
            return;
        }
    }
    onTreeNodeContextMenu(node, builder) {
        if (!node.isRoot) {
        }
    }
    buildBreadcrumbMenu(top, node, builder) {
        node.forEachChild(child => {
            let relativePath = child.getRelativePath(top);
            builder.addAction(relativePath, () => {
                this._treeView.selected = child;
            });
            this.buildBreadcrumbMenu(top, child, builder);
        });
    }
    onClickBreadcrumb(node, isContext) {
        if (isContext) {
            let builder = new menu_builder_1.MenuBuilder();
            this.buildBreadcrumbMenu(node, node, builder);
            let menu = builder.build();
            if (menu) {
                menu.ShowAsContext();
            }
        }
        else {
            this._treeView.selected = node;
        }
    }
    Awake() {
        jsb.AddCacheString("");
        this._hSplitView = new splitview_1.HSplitView();
        this._treeView = new treeview_1.UTreeView(this);
        this._breadcrumb = new breadcrumb_1.Breadcrumb();
        this._breadcrumb.on(breadcrumb_1.Breadcrumb.CLICKED, this, this.onClickBreadcrumb);
    }
    drawLeftTreeView(width, height) {
        this._treeView.searchString = UnityEditor_1.EditorGUILayout.TextField(this._treeView.searchString);
        this._treeViewScroll = UnityEditor_1.EditorGUILayout.BeginScrollView(this._treeViewScroll);
        if (this._treeView.draw(this._treeViewScroll.x, this._treeViewScroll.y, width, height)) {
            this.Repaint();
        }
        UnityEditor_1.EditorGUILayout.EndScrollView();
    }
    drawConfigView(data, node) { }
    drawFolderView(data, node) { }
    TRect(x, y, w, h) {
        this._tempRect.Set(x, y, w, h);
        return this._tempRect;
    }
    TContent(name, icon, tooltip, text) {
        let content = this._contents[name];
        if (typeof content === "undefined") {
            if (typeof text === "string") {
                if (typeof tooltip === "string") {
                    content = new UnityEngine_1.GUIContent(text, treenode_1.BuiltinIcons.getIcon(icon), tooltip);
                }
                else {
                    content = new UnityEngine_1.GUIContent(text, treenode_1.BuiltinIcons.getIcon(icon));
                }
            }
            else {
                if (typeof tooltip === "string") {
                    content = new UnityEngine_1.GUIContent(treenode_1.BuiltinIcons.getIcon(icon), tooltip);
                }
                else {
                    content = new UnityEngine_1.GUIContent(treenode_1.BuiltinIcons.getIcon(icon));
                }
            }
            this._contents[name] = content;
        }
        return content;
    }
    OnGUI() {
        this._event = UnityEngine_1.Event.current;
        let padding = 8;
        let windowStartY = this.toobarHeight + padding * 0.5;
        let windowWidth = this.position.width;
        let windowHeight = this.position.height - windowStartY;
        this._toolbarRect.Set(padding * 0.5, padding * 0.5, windowWidth - padding, this.toobarHeight);
        UnityEngine_1.GUILayout.BeginArea(this._toolbarRect);
        UnityEngine_1.GUILayout.BeginHorizontal();
        this.drawToolBar();
        UnityEngine_1.GUILayout.EndHorizontal();
        UnityEngine_1.GUILayout.EndArea();
        this._tempRect.Set(0, windowStartY, windowWidth, 1);
        UnityEditor_1.EditorGUI.DrawRect(this._tempRect, this._hSplitView.cursorHintColor);
        this._hSplitView.draw(this, windowStartY, windowWidth, windowHeight);
        this._leftRect.Set(0, windowStartY, this._hSplitView.cursorChangeRect.x, windowHeight);
        UnityEngine_1.GUILayout.BeginArea(this._leftRect);
        this.drawLeftTreeView(this._leftRect.width, this._leftRect.height);
        UnityEngine_1.GUILayout.EndArea();
        this._rightRect.Set(this._leftRect.width + this._hSplitView.cursorChangeRect.width + padding, windowStartY + padding, windowWidth - this._hSplitView.cursorChangeRect.xMax - padding * 2, windowHeight - padding * 2 - windowStartY);
        UnityEngine_1.GUILayout.BeginArea(this._rightRect);
        let selected = this._treeView.selected;
        if (selected && selected.data) {
            this._breadcrumb.draw(selected);
            this._tempRect.Set(0, this._breadcrumb.height - 6, this._rightRect.width, 1);
            UnityEditor_1.EditorGUI.DrawRect(this._tempRect, this._hSplitView.cursorHintColor);
            if (selected.isFolder) {
                this.drawFolderView(selected.data, selected);
            }
            else {
                this.drawConfigView(selected.data, selected);
            }
        }
        else {
            UnityEditor_1.EditorGUILayout.HelpBox(string_utils_1.TEXT("Nothing Selected"), UnityEditor_1.MessageType.Warning);
        }
        UnityEngine_1.GUILayout.EndArea();
    }
}
exports.EditorWindowBase = EditorWindowBase;
//# sourceMappingURL=editor_window_base.js.map