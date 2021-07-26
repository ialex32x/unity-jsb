import { AssetDatabase, EditorGUI, EditorGUILayout, EditorGUIUtility, EditorStyles, GenericMenu, Handles } from "UnityEditor";
import { Color, Event, EventType, FocusType, GUI, GUIContent, GUILayout, GUIUtility, KeyCode, Rect, Texture, Vector2, Vector3 } from "UnityEngine";
import { EventDispatcher } from "../../../plover/events/dispatcher";
import { ITreeNodeEventHandler, UTreeNode } from "./treenode";

export class UTreeView {
    static readonly CONTEXT_MENU = "CONTEXT_MENU";
    readonly SKIP_RETURN = 0;

    private _handler: ITreeNodeEventHandler;
    private _events: EventDispatcher;
    private _eventUsed = false;
    private _skipReturn = 0;
    private _root: UTreeNode;
    private _height: number;
    private _drawY: number;
    private _rowIndex: number;
    private _indentSize: number = 16;

    private _controlRect: Rect;
    private _controlID: number;
    private _controlEventType: EventType;
    private _controlMousePos: Vector2;

    private _rowRect = Rect.zero;
    private _indentRect = Rect.zero;
    private _tempRect = Rect.zero;
    // private _point = Vector3.zero;
    // private _drawLine = false;

    private _selected: UTreeNode;
    private _editing = false;
    private _deferredMenuPopup: boolean = false;
    private _searchString: string;
    private _selectionColor = new Color(44 / 255, 93 / 255, 135 / 255);
    private _rowColor = new Color(0.5, 0.5, 0.5, 0.1);
    private _focusColor = new Color(58 / 255, 121 / 255, 187 / 255);

    private _debug_touchChild = 0;
    private _debug_drawChild = 0;

    get selected() { return this._selected; }

    set selected(value: UTreeNode) {
        if (this._selected != value) {
            this._selected?.endEdit();
            this._editing = false;
            this._skipReturn = 0;
            this._selected = value;
        }
    }

    get searchString() { return this._searchString; }

    set searchString(value: string) { this.search(value); }

    get root() {
        return this._root;
    }

    get handler() { return this._handler; }

    set handler(value: ITreeNodeEventHandler) { this._handler = value; }

    constructor(handler: ITreeNodeEventHandler) {
        this._searchString = "";
        this._handler = handler;
        this._root = new UTreeNode(this, null, true, "/");
        this._root.isEditable = false;
        this._root.isSearchable = false;
        this._root.expanded = true;
    }

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

    allocFolderHierarchy(path: string, data: any): UTreeNode {
        return this._getFolderHierarchy(path, data);
    }

    getFolderHierarchy(path: string): UTreeNode {
        return this._getFolderHierarchy(path, null);
    }

    private _getFolderHierarchy(path: string, data: any): UTreeNode {
        if (path.startsWith("/")) {
            path = path.substring(1);
        }

        let node: UTreeNode = this._root;
        if (path.length > 0) {
            let hierarchy = path.split("/");
            for (let i = 0; i < hierarchy.length; i++) {
                node = node.getFolderByName(hierarchy[i], true, data);
            }
        }

        return node;
    }

    removeAll() {
        this._root.removeAll();
        this.selected = null;
    }

    deleteNode(node: UTreeNode) {
        if (node && this._selected == node && node.parent) {
            this._selected = this.findNextNode(node) || this.findPreviousNode(node);
            return node.parent.removeChild(node);
        }

        return false;
    }

    search(p: string) {
        if (p == null) {
            p = "";
        }

        if (this._searchString != p) {
            this._searchString = p;
            this._search(this._root);
        }
    }

    private _search(node: UTreeNode) {
        node.match(this._searchString);

        for (let i = 0, count = node.childCount; i < count; i++) {
            this._search(node.getChildByIndex(i));
        }
    }

    expandAll() { this._root.expandAll(); }

    collapseAll() { this._root.collapseAll(); }

    draw(offsetX: number, offsetY: number, width: number, height: number) {
        let repaint = false;
        let cEvent = Event.current;

        if (this._deferredMenuPopup) {
            this._deferredMenuPopup = false;
            if (this._selected) {
                this._selected.drawMenu(this, cEvent.mousePosition, this._handler);
                repaint = true;
            }
        }

        this._debug_touchChild = 0;
        this._debug_drawChild = 0;
        this._eventUsed = false;
        this._height = 0;
        this._drawY = 0;
        this._rowIndex = 0;

        if (this._searchString == null || this._searchString.length == 0) {
            this.calcRowHeight(this._root);
            this.setControlRect(cEvent);
            this.drawRow(this._root, 0, offsetY, height);
        } else {
            this.calcSearchResultsHeight(this._root);
            this.setControlRect(cEvent);
            this.drawSearchResults(this._root, 0, offsetY, height);
        }

        if (this._controlID == GUIUtility.keyboardControl) {
            this._tempRect.Set(0, 0, 1, height);
            EditorGUI.DrawRect(this._tempRect, this._focusColor);
        }

        if (cEvent.isKey) {
            let eventType = cEvent.type;

            if (this._editing) {
                switch (eventType) {
                    case EventType.KeyUp: {
                        let keyCode = cEvent.keyCode;
                        if (keyCode == KeyCode.Return) {
                            if (this._skipReturn > 0) {
                                this._skipReturn--;
                                this.useEvent();
                            } else {
                                GUI.FocusControl(null);
                                GUIUtility.keyboardControl = this._controlID;
                                this._selected?.endEdit();
                                this._editing = false;
                                this._skipReturn = 0;
                                this.useEvent();
                            }
                        }
                    } break;
                }
            } else {
                if (this._selected && this._controlEventType == EventType.KeyUp && this._controlID == GUIUtility.keyboardControl) {
                    // console.log(GUIUtility.keyboardControl, this._controlID);
                    let keyCode = cEvent.keyCode;
                    if (keyCode == KeyCode.Return) {
                        if (this._selected.isEditable) {
                            this._editing = true;
                            this._skipReturn = this.SKIP_RETURN;
                            this.useEvent();
                        }
                    } else {
                        if (keyCode == KeyCode.UpArrow) {
                            if (this._selected.parent) {
                                let sibling = this.findPreviousNode(this._selected);
                                if (sibling) {
                                    this._selected = sibling;
                                    this._selected.expandUp();
                                    this.useEvent();
                                }
                            }
                        } else if (keyCode == KeyCode.DownArrow) {
                            let sibling = this.findNextNode(this._selected);
                            if (sibling) {
                                this._selected = sibling;
                                this._selected.expandUp();
                                this.useEvent();
                            }
                        } else if (keyCode == KeyCode.LeftArrow) {
                            if (this._selected.expanded && this._selected.isFolder) {
                                this._selected.expanded = false;
                                this._selected.expandUp();
                            } else if (this._selected.parent) {
                                this._selected = this._selected.parent;
                                this._selected.expandUp();
                            }
                            this.useEvent();
                        } else if (keyCode == KeyCode.RightArrow) {
                            this._selected.expanded = true;
                            this._selected.expandUp();
                            this.useEvent();
                        }
                    }
                }
            }
        } else {
            if (!this._editing && this._controlEventType == EventType.MouseUp) {
                this._tempRect.Set(0, 0, width, height);
                if (this._tempRect.Contains(this._controlMousePos)) {
                    GUIUtility.keyboardControl = this._controlID;
                    repaint = true;
                }
            }
        }

        return this._deferredMenuPopup || repaint;
    }

    private calcRowHeight(node: UTreeNode) {
        this._height += node.calcRowHeight();

        if (node.expanded) {
            for (let i = 0, count = node.childCount; i < count; i++) {
                this.calcRowHeight(node.getChildByIndex(i));
            }
        }
    }

    private calcSearchResultsHeight(node: UTreeNode) {
        if (node.isMatch) {
            this._height += node.calcRowHeight();
        }

        for (let i = 0, count = node.childCount; i < count; i++) {
            this.calcRowHeight(node.getChildByIndex(i));
        }
    }

    private setControlRect(cEvent: Event) {
        this._controlRect = EditorGUILayout.GetControlRect(false, this._height, GUILayout.MinWidth(160));
        this._controlID = GUIUtility.GetControlID(FocusType.Keyboard, this._controlRect);
        this._controlEventType = cEvent.GetTypeForControl(this._controlID);
        if (this._controlEventType == EventType.MouseUp) {
            this._controlMousePos = cEvent.mousePosition;
        }
    }

    private useEvent() {
        this._eventUsed = true;
        GUI.changed = true;
        Event.current.Use();
    }

    private drawSearchResults(node: UTreeNode, depth: number, offsetY: number, height: number) {
        let drawY = this._drawY;

        if (node.isMatch) {
            this._drawY += node.height;
            ++this._rowIndex;
            ++this._debug_touchChild;

            if ((this._drawY - offsetY) > 0 && (drawY - offsetY) < height) {
                let rowIndent = 0;
                let baseX = 14;
                let bSelected = this._selected == node;

                ++this._debug_drawChild;
                this._rowRect.Set(this._controlRect.x, this._controlRect.y + drawY, this._controlRect.width, node.height);
                this._indentRect.Set(this._controlRect.x + baseX + rowIndent, this._rowRect.y, this._controlRect.width - rowIndent, node.height);

                if (bSelected) {
                    EditorGUI.DrawRect(this._rowRect, this._selectionColor);
                } else if (this._rowIndex % 2) {
                    EditorGUI.DrawRect(this._rowRect, this._rowColor);
                }

                node.draw(this._indentRect, bSelected, bSelected && this._editing, this._indentSize);

                if (this._controlEventType == EventType.MouseUp) {
                    if (this._rowRect.Contains(this._controlMousePos)) {
                        if (Event.current.button == 1) {
                            if (this._selected == node) {
                                node.drawMenu(this, this._controlMousePos, this._handler);
                                this.useEvent();
                            } else {
                                this.selected = node;
                                if (!this._editing) {
                                    this._deferredMenuPopup = true;
                                }
                                this.useEvent();
                            }
                        } else if (Event.current.button == 0) {
                            if (node.isFolder && node._foldoutRect.Contains(this._controlMousePos)) {
                                node.expanded = !node.expanded;
                            } else {
                                this.selected = node;
                            }
                            this.useEvent();
                        }
                    }
                }
            }
        }

        for (let i = 0, count = node.childCount; i < count; i++) {
            this.drawSearchResults(node.getChildByIndex(i), depth + 1, offsetY, height);
        }
    }

    private drawRow(node: UTreeNode, depth: number, offsetY: number, height: number) {
        let drawY = this._drawY;

        this._drawY += node.height;
        ++this._rowIndex;
        ++this._debug_touchChild;

        if ((this._drawY - offsetY) > 0 && (drawY - offsetY) < height) {
            let rowIndent = this._indentSize * depth;
            let baseX = 14;
            let bSelected = this._selected == node;

            ++this._debug_drawChild;
            this._rowRect.Set(this._controlRect.x, this._controlRect.y + drawY, this._controlRect.width, node.height);
            this._indentRect.Set(this._controlRect.x + baseX + rowIndent, this._rowRect.y, this._controlRect.width - rowIndent, node.height);

            if (bSelected) {
                EditorGUI.DrawRect(this._rowRect, this._selectionColor);
            } else if (this._rowIndex % 2) {
                EditorGUI.DrawRect(this._rowRect, this._rowColor);
            }

            node.draw(this._indentRect, bSelected, bSelected && this._editing, this._indentSize);

            if (this._controlEventType == EventType.MouseUp) {
                if (this._rowRect.Contains(this._controlMousePos)) {
                    if (Event.current.button == 1) {
                        if (this._selected == node) {
                            node.drawMenu(this, this._controlMousePos, this._handler);
                            this.useEvent();
                        } else {
                            this.selected = node;
                            if (!this._editing) {
                                this._deferredMenuPopup = true;
                            }
                            this.useEvent();
                        }
                    } else if (Event.current.button == 0) {
                        if (node.isFolder && node._foldoutRect.Contains(this._controlMousePos)) {
                            node.expanded = !node.expanded;
                        } else {
                            this.selected = node;
                        }
                        this.useEvent();
                    }
                }
            }
        } else {
            node.visible = false;
        }

        if (node.expanded) {
            for (let i = 0, count = node.childCount; i < count; i++) {
                this.drawRow(node.getChildByIndex(i), depth + 1, offsetY, height);
                // if (this._drawLine && i == count - 1) {
                //     this._point.Set(child._lineStart.x, node._lineStart.y, 0);
                //     // Handles.DrawDottedLine(this._point, child._lineStartIn, 1);
                //     Handles.color = Color.gray;
                //     Handles.DrawLine(this._point, child._lineStartIn);
                // }
            }
        }
    }

    findPreviousNode(node: UTreeNode) {
        let sibling = node.parent.findLastSibling(node);
        while (sibling && sibling.expanded && sibling.childCount > 0) {
            sibling = sibling.getLastChild();
        }

        return sibling || node.parent;
    }

    findNextNode(node: UTreeNode) {
        if (node.expanded && node.childCount > 0) {
            return node.getFirstChild();
        }

        while (node.parent) {
            let sibling = node.parent.findNextSibling(node);
            if (sibling) {
                return sibling;
            }
            node = node.parent;
        }
        return null;
    }
}
