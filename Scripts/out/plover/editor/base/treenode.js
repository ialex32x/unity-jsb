"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.UTreeNode = exports.BuiltinIcons = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
const dispatcher_1 = require("../../../plover/events/dispatcher");
const menu_builder_1 = require("./menu_builder");
const treeview_1 = require("./treeview");
class BuiltinIcons {
    static getIcon(name) {
        let icon = this._cache[name];
        if (typeof icon === "undefined") {
            icon = this._cache[name] = UnityEditor_1.AssetDatabase.LoadAssetAtPath(`Assets/jsb/Editor/Icons/${name}.png`, UnityEngine_1.Texture);
        }
        return icon;
    }
}
exports.BuiltinIcons = BuiltinIcons;
BuiltinIcons._cache = {};
class UTreeNode {
    constructor(tree, parent, isFolder, name) {
        this._children = null;
        this._expanded = true;
        this._name = "noname";
        this.isSearchable = true;
        this.isEditable = true;
        // _lineStart = Vector3.zero;
        // _lineStartIn = Vector3.zero;
        // protected _lineEnd = Vector3.zero;
        this._foldoutRect = UnityEngine_1.Rect.zero;
        this._bFocusTextField = false;
        this._bVisible = true;
        this._height = 0;
        this._bMatch = true;
        this._name = name;
        this._tree = tree;
        this._parent = parent;
        this._children = isFolder ? [] : null;
    }
    get isMatch() { return this._bMatch; }
    get height() { return this._height; }
    /**
     * 当前层级是否展开
     */
    get expanded() { return this._expanded; }
    set expanded(value) {
        if (this._expanded != value) {
            this._expanded = value;
        }
    }
    get isFolder() { return !!this._children; }
    get visible() { return this._bVisible; }
    set visible(value) {
        if (this._bVisible != value) {
            this._bVisible = value;
        }
    }
    get parent() { return this._parent; }
    get isRoot() { return this._parent == null; }
    get name() { return this._name; }
    set name(value) {
        if (this._name != value) {
            let oldName = this._name;
            this._name = value;
            this._tree.handler.onTreeNodeNameChanged(this, oldName);
        }
    }
    get fullPath() {
        let path = this._name;
        let node = this._parent;
        while (node && !node.isRoot) {
            if (node._name && node._name.length > 0) {
                path = node._name + "/" + path;
            }
            node = node._parent;
        }
        return path;
    }
    get treeView() { return this._tree; }
    get childCount() { return this._children ? this._children.length : 0; }
    on(evt, caller, fn) {
        if (!this._events) {
            this._events = new dispatcher_1.EventDispatcher();
        }
        this._events.on(evt, caller, fn);
    }
    off(evt, caller, fn) {
        if (this._events) {
            this._events.off(evt, caller, fn);
        }
    }
    dispatch(name, arg0, arg1, arg2) {
        if (!this._events) {
            this._events = new dispatcher_1.EventDispatcher();
        }
        this._events.dispatch(name, arg0, arg1, arg2);
    }
    match(p) {
        if (p == null || p.length == 0) {
            return this._bMatch = true;
        }
        return this._bMatch = this.isSearchable && this._name.indexOf(p) >= 0;
    }
    getRelativePath(top) {
        let path = this._name;
        let node = this._parent;
        while (node && node != top) {
            path = node._name + "/" + path;
            node = node._parent;
        }
        return path;
    }
    expandAll() {
        this._setExpandAll(true);
    }
    collapseAll() {
        this._setExpandAll(false);
    }
    _setExpandAll(state) {
        this._expanded = state;
        if (this._children) {
            for (let i = 0, count = this._children.length; i < count; i++) {
                this._children[i]._setExpandAll(state);
            }
        }
    }
    expandUp() {
        let node = this._parent;
        while (node) {
            node.expanded = true;
            node = node.parent;
        }
    }
    /**
     * 获取指定节点的在当前层级中的下一个相邻节点
     */
    findNextSibling(node) {
        if (this._children) {
            let index = this._children.indexOf(node);
            if (index >= 0 && index < this._children.length - 1) {
                return this._children[index + 1];
            }
        }
        return null;
    }
    /**
     * 获取指定节点的在当前层级中的上一个相邻节点
     */
    findLastSibling(node) {
        if (this._children) {
            let index = this._children.indexOf(node);
            if (index > 0) {
                return this._children[index - 1];
            }
        }
        return null;
    }
    forEachChild(fn) {
        if (this._children) {
            for (let i = 0, count = this._children.length; i < count; i++) {
                fn(this._children[i]);
            }
        }
    }
    /**
     * 获取当前层级下的子节点
     * @param index 索引 或者 命名
     * @param autoNew 不存在时是否创建 (仅通过命名获取时有效)
     * @returns 子节点
     */
    getFolderByName(name, isAutoCreate, data) {
        if (this._children) {
            for (let i = 0, size = this._children.length; i < size; i++) {
                let child = this._children[i];
                if (child.isFolder && child.name == name) {
                    return child;
                }
            }
            if (isAutoCreate) {
                let child = this._addChild(name, true, data);
                return child;
            }
        }
        return null;
    }
    getLeafByName(name, isAutoCreate, data) {
        if (this._children) {
            for (let i = 0, size = this._children.length; i < size; i++) {
                let child = this._children[i];
                if (!child.isFolder && child.name == name) {
                    return child;
                }
            }
            if (isAutoCreate) {
                let child = this._addChild(name, false, data);
                return child;
            }
        }
        return null;
    }
    getChildByIndex(index) {
        return this._children[index];
    }
    /**
     * 当前层级最后一个子节点
     */
    getLastChild() {
        return this._children && this._children.length > 0 ? this._children[this._children.length - 1] : null;
    }
    /**
     * 当前层级第一个子节点
     */
    getFirstChild() {
        return this._children && this._children.length > 0 ? this._children[0] : null;
    }
    addFolderChild(name) {
        return this.getFolderByName(name, true, null);
    }
    addLeafChild(name) {
        return this.getLeafByName(name, true, null);
    }
    allocLeafChild(name, data) {
        return this.getLeafByName(name, true, data);
    }
    /**
     * 在当前层级添加一个子节点
     */
    _addChild(name, isFolder, data) {
        if (this._children) {
            let node = new UTreeNode(this._tree, this, isFolder, name);
            this._children.push(node);
            node._expanded = true;
            node.data = data;
            this._tree.handler.onTreeNodeCreated(node);
            return node;
        }
        return null;
    }
    /**
     * 将一个子节点从当前层级中移除
     */
    removeChild(node) {
        if (this._children) {
            let index = this._children.indexOf(node);
            if (index >= 0) {
                this._children.splice(index, 1);
                return true;
            }
        }
        return false;
    }
    removeAll() {
        if (this._children) {
            this._children.splice(0);
        }
    }
    calcRowHeight() {
        this._height = UnityEditor_1.EditorGUIUtility.singleLineHeight;
        return this._height;
    }
    drawMenu(treeView, pos, handler) {
        let builder = new menu_builder_1.MenuBuilder();
        handler.onTreeNodeContextMenu(this, builder);
        treeView.dispatch(treeview_1.UTreeView.CONTEXT_MENU, builder, this);
        let menu = builder.build();
        if (menu) {
            menu.ShowAsContext();
        }
    }
    draw(rect, bSelected, bEditing, indentSize) {
        // let lineY = rect.y + rect.height * 0.5;
        // this._lineStartIn.Set(rect.x - indentSize * 1.5, lineY, 0);
        // this._lineStart.Set(rect.x - indentSize * 1.5, rect.y + rect.height, 0);
        this._bVisible = true;
        if (this._children && this._children.length > 0) {
            this._foldoutRect.Set(rect.x - 14, rect.y, 12, rect.height);
            /*this._expanded =*/ UnityEditor_1.EditorGUI.Foldout(this._foldoutRect, this._expanded, UnityEngine_1.GUIContent.none);
            // this._lineEnd.Set(rect.x - indentSize, lineY, 0);
            let image = this._expanded ? BuiltinIcons.getIcon("FolderOpened") : BuiltinIcons.getIcon("Folder");
            if (!this._label) {
                this._label = new UnityEngine_1.GUIContent(this._name, image);
            }
            else {
                this._label.image = image;
            }
        }
        else {
            // this._lineEnd.Set(rect.x - 4, lineY, 0);
            if (!this._label) {
                this._label = new UnityEngine_1.GUIContent(this._name, BuiltinIcons.getIcon("JsScript"));
            }
        }
        // Handles.color = Color.gray;
        // Handles.DrawLine(this._lineStartIn, this._lineEnd);
        if (bEditing) {
            let text;
            if (this._bFocusTextField) {
                UnityEngine_1.GUI.SetNextControlName("TreeViewNode.Editing");
                this._label.text = UnityEditor_1.EditorGUI.TextField(rect, this._label.text);
            }
            else {
                UnityEngine_1.GUI.SetNextControlName("TreeViewNode.Editing");
                this._label.text = UnityEditor_1.EditorGUI.TextField(rect, this._label.text);
                UnityEngine_1.GUI.FocusControl("TreeViewNode.Editing");
            }
        }
        else {
            this._bFocusTextField = false;
            UnityEditor_1.EditorGUI.LabelField(rect, this._label, bSelected ? UnityEditor_1.EditorStyles.whiteLabel : UnityEditor_1.EditorStyles.label);
        }
    }
    endEdit() {
        if (this._label.text != this._name) {
            this._tree.handler.onTreeNodeNameEditEnded(this, this._label.text);
        }
    }
}
exports.UTreeNode = UTreeNode;
//# sourceMappingURL=treenode.js.map