import { EditorGUI, EditorGUILayout, EditorWindow, MessageType } from "UnityEditor";
import { Event, EventType, GUI, GUIContent, GUILayout, Rect, Vector2 } from "UnityEngine";
import { MenuBuilder } from "../base/menu_builder";
import { HSplitView } from "../base/splitview";
import { BuiltinIcons, ITreeNodeEventHandler, UTreeNode } from "../base/treenode";
import { UTreeView } from "../base/treeview";
import * as jsb from "jsb";
import { TEXT } from "../../text/string_utils";
import { Breadcrumb } from "../base/breadcrumb";

export abstract class EditorWindowBase extends EditorWindow implements ITreeNodeEventHandler {
    protected _treeView: UTreeView;
    protected _breadcrumb: Breadcrumb;
    protected _treeViewScroll = Vector2.zero;
    protected _hSplitView: HSplitView;
    protected _toolbarRect = Rect.zero;
    protected _leftRect = Rect.zero;
    protected _rightRect = Rect.zero;
    // protected _topSplitLine = Rect.zero;
    protected _searchLabel = new GUIContent("Search");
    protected _tempRect = Rect.zero;
    protected _event: Event;
    protected _contents: { [key: string]: GUIContent } = {};

    toobarHeight = 24;

    onTreeNodeNameEditEnded(node: UTreeNode, newName: string) {
        if (node.isRoot || !node.data) {
            return;
        }

        node.name = newName;
    }

    onTreeNodeNameChanged(node: UTreeNode, oldName: string) {
        if (node.isRoot || !node.data) {
            return;
        }

    }

    onTreeNodeCreated(node: UTreeNode) {
        if (node.data) {
            return;
        }

    }

    onTreeNodeContextMenu(node: UTreeNode, builder: MenuBuilder) {
        if (!node.isRoot) {

        }
    }

    buildBreadcrumbMenu(top: UTreeNode, node: UTreeNode, builder: MenuBuilder) {
        node.forEachChild(child => {
            let relativePath = child.getRelativePath(top);
            builder.addAction(relativePath, () => {
                this._treeView.selected = child;
            });

            this.buildBreadcrumbMenu(top, child, builder);
        });
    }

    onClickBreadcrumb(node: UTreeNode, isContext: boolean) {
        if (isContext) {
            let builder = new MenuBuilder();

            this.buildBreadcrumbMenu(node, node, builder);
            let menu = builder.build();
            if (menu) {
                menu.ShowAsContext();
            }
        } else {
            this._treeView.selected = node;
        }
    }

    Awake() {
        jsb.AddCacheString("");
        this._hSplitView = new HSplitView();
        this._treeView = new UTreeView(this);
        this._breadcrumb = new Breadcrumb();
        this._breadcrumb.on(Breadcrumb.CLICKED, this, this.onClickBreadcrumb);
    }

    private drawLeftTreeView(width: number, height: number) {
        this._treeView.searchString = EditorGUILayout.TextField(this._treeView.searchString);
        this._treeViewScroll = EditorGUILayout.BeginScrollView(this._treeViewScroll);
        if (this._treeView.draw(this._treeViewScroll.x, this._treeViewScroll.y, width, height)) {
            this.Repaint();
        }
        EditorGUILayout.EndScrollView();
    }

    protected drawConfigView(data: any, node: UTreeNode) { }

    protected drawFolderView(data: any, node: UTreeNode) { }

    protected abstract drawToolBar();

    protected TRect(x: number, y: number, w: number, h: number) {
        this._tempRect.Set(x, y, w, h);
        return this._tempRect;
    }

    protected TContent(name: string, icon: string, tooltip?: string, text?: string) {
        let content = this._contents[name];
        if (typeof content === "undefined") {
            if (typeof text === "string") {
                if (typeof tooltip === "string") {
                    content = new GUIContent(text, BuiltinIcons.getIcon(icon), tooltip);
                } else {
                    content = new GUIContent(text, BuiltinIcons.getIcon(icon));
                }
            } else {
                if (typeof tooltip === "string") {
                    content = new GUIContent(BuiltinIcons.getIcon(icon), tooltip);
                } else {
                    content = new GUIContent(BuiltinIcons.getIcon(icon));
                }
            }
            this._contents[name] = content;
        }

        return content;
    }

    OnGUI() {
        this._event = Event.current;

        let padding = 8;
        let windowStartY = this.toobarHeight + padding * 0.5;
        let windowWidth = this.position.width;
        let windowHeight = this.position.height - windowStartY;

        this._toolbarRect.Set(padding * 0.5, padding * 0.5, windowWidth - padding, this.toobarHeight);
        GUILayout.BeginArea(this._toolbarRect);
        GUILayout.BeginHorizontal();
        this.drawToolBar();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        this._tempRect.Set(0, windowStartY, windowWidth, 1);
        EditorGUI.DrawRect(this._tempRect, this._hSplitView.cursorHintColor);
        this._hSplitView.draw(this, windowStartY, windowWidth, windowHeight);

        this._leftRect.Set(0, windowStartY, this._hSplitView.cursorChangeRect.x, windowHeight);
        GUILayout.BeginArea(this._leftRect);
        this.drawLeftTreeView(this._leftRect.width, this._leftRect.height);
        GUILayout.EndArea();

        this._rightRect.Set(this._leftRect.width + this._hSplitView.cursorChangeRect.width + padding, windowStartY + padding, windowWidth - this._hSplitView.cursorChangeRect.xMax - padding * 2, windowHeight - padding * 2 - windowStartY);
        GUILayout.BeginArea(this._rightRect);

        let selected = this._treeView.selected;
        if (selected && selected.data) {
            this._breadcrumb.draw(selected);
            this._tempRect.Set(0, this._breadcrumb.height - 6, this._rightRect.width, 1);
            EditorGUI.DrawRect(this._tempRect, this._hSplitView.cursorHintColor);

            if (selected.isFolder) {
                this.drawFolderView(selected.data, selected);
            } else {
                this.drawConfigView(selected.data, selected);
            }
        } else {
            EditorGUILayout.HelpBox(TEXT("Nothing Selected"), MessageType.Warning);
        }
        GUILayout.EndArea();
    }
}
