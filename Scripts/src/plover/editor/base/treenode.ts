import { AssetDatabase, EditorGUI, EditorGUILayout, EditorGUIUtility, EditorStyles, GenericMenu, Handles } from "UnityEditor";
import { Color, Event, EventType, FocusType, GUI, GUIContent, GUILayout, GUIUtility, KeyCode, Rect, Texture, Vector2, Vector3 } from "UnityEngine";
import { EventDispatcher } from "../../../plover/events/dispatcher";
import { MenuBuilder } from "./menu_builder";
import { UTreeView } from "./treeview";

export interface ITreeNodeEventHandler {
    onTreeNodeContextMenu(node: UTreeNode, builder: MenuBuilder);
    onTreeNodeCreated(node: UTreeNode);
    onTreeNodeNameEditEnded(node: UTreeNode, newName: string);
    onTreeNodeNameChanged(node: UTreeNode, oldName: string);
}

export class BuiltinIcons {
    private static _cache: { [key: string]: Texture } = {};

    static getIcon(name: string) {
        let icon = this._cache[name];
        if (typeof icon === "undefined") {
            icon = this._cache[name] = <Texture>AssetDatabase.LoadAssetAtPath(`Assets/jsb/Editor/Icons/${name}.png`, Texture);
        }
        return icon;
    }
}

export class UTreeNode {
    protected _tree: UTreeView;
    protected _parent: UTreeNode;
    protected _children: Array<UTreeNode> = null;
    protected _expanded: boolean = true;
    protected _name: string = "noname";
    protected _events: EventDispatcher;

    data: any;
    isSearchable: boolean = true;
    isEditable: boolean = true;

    // _lineStart = Vector3.zero;
    // _lineStartIn = Vector3.zero;
    // protected _lineEnd = Vector3.zero;
    _foldoutRect = Rect.zero;
    protected _label: GUIContent;
    protected _folderClose: Texture;
    protected _folderOpen: Texture;

    private _bFocusTextField = false;
    private _bVisible = true;
    private _height = 0;
    private _bMatch = true;

    get isMatch() { return this._bMatch; }

    get height() { return this._height; }

    /**
     * 当前层级是否展开
     */
    get expanded() { return this._expanded; }

    set expanded(value: boolean) {
        if (this._expanded != value) {
            this._expanded = value;
        }
    }

    get isFolder() { return !!this._children; }

    get visible() { return this._bVisible; }

    set visible(value: boolean) {
        if (this._bVisible != value) {
            this._bVisible = value;
        }
    }

    get parent() { return this._parent; }

    get isRoot() { return this._parent == null; }

    get name() { return this._name; }

    set name(value: string) {
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

    constructor(tree: UTreeView, parent: UTreeNode, isFolder: boolean, name: string) {
        this._name = name;
        this._tree = tree;
        this._parent = parent;
        this._children = isFolder ? [] : null
    }

    get childCount() { return this._children ? this._children.length : 0; }

    on(evt: string, caller: any, fn?: Function) {
        if (!this._events) {
            this._events = new EventDispatcher();
        }
        this._events.on(evt, caller, fn);
    }

    off(evt: string, caller: any, fn?: Function) {
        if (this._events) {
            this._events.off(evt, caller, fn);
        }
    }

    dispatch(name: string, arg0?: any, arg1?: any, arg2?: any) {
        if (!this._events) {
            this._events = new EventDispatcher();
        }
        this._events.dispatch(name, arg0, arg1, arg2);
    }

    match(p: string) {
        if (p == null || p.length == 0) {
            return this._bMatch = true;
        }

        return this._bMatch = this.isSearchable && this._name.indexOf(p) >= 0;
    }

    getRelativePath(top: UTreeNode) {
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

    private _setExpandAll(state: boolean) {
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
    findNextSibling(node: UTreeNode) {
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
    findLastSibling(node: UTreeNode) {
        if (this._children) {
            let index = this._children.indexOf(node);
            if (index > 0) {
                return this._children[index - 1];
            }
        }
        return null;
    }

    forEachChild(fn: (child: UTreeNode) => void) {
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
    getFolderByName(name: string, isAutoCreate: boolean, data: any) {
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

    getLeafByName(name: string, isAutoCreate: boolean, data: any) {
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

    getChildByIndex(index: number) {
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

    addFolderChild(name: string) {
        return this.getFolderByName(name, true, null);
    }

    addLeafChild(name: string) {
        return this.getLeafByName(name, true, null);
    }

    allocLeafChild(name: string, data: any) {
        return this.getLeafByName(name, true, data);
    }

    /**
     * 在当前层级添加一个子节点
     */
    private _addChild(name: string, isFolder: boolean, data: any) {
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
    removeChild(node: UTreeNode) {
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
        this._height = EditorGUIUtility.singleLineHeight;
        return this._height;
    }

    drawMenu(treeView: UTreeView, pos: Vector2, handler: ITreeNodeEventHandler) {
        let builder = new MenuBuilder();

        handler.onTreeNodeContextMenu(this, builder);
        treeView.dispatch(UTreeView.CONTEXT_MENU, builder, this);
        let menu = builder.build();
        if (menu) {
            menu.ShowAsContext();
        }
    }

    draw(rect: Rect, bSelected: boolean, bEditing: boolean, indentSize: number) {
        // let lineY = rect.y + rect.height * 0.5;

        // this._lineStartIn.Set(rect.x - indentSize * 1.5, lineY, 0);
        // this._lineStart.Set(rect.x - indentSize * 1.5, rect.y + rect.height, 0);

        this._bVisible = true;
        if (this._children && this._children.length > 0) {
            this._foldoutRect.Set(rect.x - 14, rect.y, 12, rect.height);
            /*this._expanded =*/EditorGUI.Foldout(this._foldoutRect, this._expanded, GUIContent.none);
            // this._lineEnd.Set(rect.x - indentSize, lineY, 0);

            let image = this._expanded ? BuiltinIcons.getIcon("FolderOpened") : BuiltinIcons.getIcon("Folder");
            if (!this._label) {
                this._label = new GUIContent(this._name, image);
            } else {
                this._label.image = image;
            }
        } else {
            // this._lineEnd.Set(rect.x - 4, lineY, 0);
            if (!this._label) {
                this._label = new GUIContent(this._name, BuiltinIcons.getIcon("JsScript"));
            }
        }
        // Handles.color = Color.gray;
        // Handles.DrawLine(this._lineStartIn, this._lineEnd);

        if (bEditing) {
            let text: string;
            if (this._bFocusTextField) {
                GUI.SetNextControlName("TreeViewNode.Editing");
                this._label.text = EditorGUI.TextField(rect, this._label.text);
            } else {
                GUI.SetNextControlName("TreeViewNode.Editing");
                this._label.text = EditorGUI.TextField(rect, this._label.text);
                GUI.FocusControl("TreeViewNode.Editing");
            }
        } else {
            this._bFocusTextField = false;
            EditorGUI.LabelField(rect, this._label, bSelected ? EditorStyles.whiteLabel : EditorStyles.label);
        }
    }

    endEdit() {
        if (this._label.text != this._name) {
            this._tree.handler.onTreeNodeNameEditEnded(this, this._label.text);
        }
    }
}
