"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.UTreeView = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
const dispatcher_1 = require("../../../plover/events/dispatcher");
const treenode_1 = require("./treenode");
class UTreeView {
    constructor(handler) {
        this.SKIP_RETURN = 0;
        this._eventUsed = false;
        this._skipReturn = 0;
        this._indentSize = 16;
        this._rowRect = UnityEngine_1.Rect.zero;
        this._indentRect = UnityEngine_1.Rect.zero;
        this._tempRect = UnityEngine_1.Rect.zero;
        this._editing = false;
        this._deferredMenuPopup = false;
        this._selectionColor = new UnityEngine_1.Color(44 / 255, 93 / 255, 135 / 255);
        this._rowColor = new UnityEngine_1.Color(0.5, 0.5, 0.5, 0.1);
        this._focusColor = new UnityEngine_1.Color(58 / 255, 121 / 255, 187 / 255);
        this._debug_touchChild = 0;
        this._debug_drawChild = 0;
        this._searchString = "";
        this._handler = handler;
        this._root = new treenode_1.UTreeNode(this, null, true, "/");
        this._root.isEditable = false;
        this._root.isSearchable = false;
        this._root.expanded = true;
    }
    get selected() { return this._selected; }
    set selected(value) {
        var _a;
        if (this._selected != value) {
            (_a = this._selected) === null || _a === void 0 ? void 0 : _a.endEdit();
            this._editing = false;
            this._skipReturn = 0;
            this._selected = value;
        }
    }
    get searchString() { return this._searchString; }
    set searchString(value) { this.search(value); }
    get root() {
        return this._root;
    }
    get handler() { return this._handler; }
    set handler(value) { this._handler = value; }
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
    allocFolderHierarchy(path, data) {
        return this._getFolderHierarchy(path, data);
    }
    getFolderHierarchy(path) {
        return this._getFolderHierarchy(path, null);
    }
    _getFolderHierarchy(path, data) {
        if (path.startsWith("/")) {
            path = path.substring(1);
        }
        let node = this._root;
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
    deleteNode(node) {
        if (node && this._selected == node && node.parent) {
            this._selected = this.findNextNode(node) || this.findPreviousNode(node);
            return node.parent.removeChild(node);
        }
        return false;
    }
    search(p) {
        if (p == null) {
            p = "";
        }
        if (this._searchString != p) {
            this._searchString = p;
            this._search(this._root);
        }
    }
    _search(node) {
        node.match(this._searchString);
        for (let i = 0, count = node.childCount; i < count; i++) {
            this._search(node.getChildByIndex(i));
        }
    }
    expandAll() { this._root.expandAll(); }
    collapseAll() { this._root.collapseAll(); }
    draw(offsetX, offsetY, width, height) {
        var _a;
        let repaint = false;
        let cEvent = UnityEngine_1.Event.current;
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
        }
        else {
            this.calcSearchResultsHeight(this._root);
            this.setControlRect(cEvent);
            this.drawSearchResults(this._root, 0, offsetY, height);
        }
        if (this._controlID == UnityEngine_1.GUIUtility.keyboardControl) {
            this._tempRect.Set(0, 0, 1, height);
            UnityEditor_1.EditorGUI.DrawRect(this._tempRect, this._focusColor);
        }
        if (cEvent.isKey) {
            let eventType = cEvent.type;
            if (this._editing) {
                switch (eventType) {
                    case UnityEngine_1.EventType.KeyUp:
                        {
                            let keyCode = cEvent.keyCode;
                            if (keyCode == UnityEngine_1.KeyCode.Return) {
                                if (this._skipReturn > 0) {
                                    this._skipReturn--;
                                    this.useEvent();
                                }
                                else {
                                    UnityEngine_1.GUI.FocusControl(null);
                                    UnityEngine_1.GUIUtility.keyboardControl = this._controlID;
                                    (_a = this._selected) === null || _a === void 0 ? void 0 : _a.endEdit();
                                    this._editing = false;
                                    this._skipReturn = 0;
                                    this.useEvent();
                                }
                            }
                        }
                        break;
                }
            }
            else {
                if (this._selected && this._controlEventType == UnityEngine_1.EventType.KeyUp && this._controlID == UnityEngine_1.GUIUtility.keyboardControl) {
                    // console.log(GUIUtility.keyboardControl, this._controlID);
                    let keyCode = cEvent.keyCode;
                    if (keyCode == UnityEngine_1.KeyCode.Return) {
                        if (this._selected.isEditable) {
                            this._editing = true;
                            this._skipReturn = this.SKIP_RETURN;
                            this.useEvent();
                        }
                    }
                    else {
                        if (keyCode == UnityEngine_1.KeyCode.UpArrow) {
                            if (this._selected.parent) {
                                let sibling = this.findPreviousNode(this._selected);
                                if (sibling) {
                                    this._selected = sibling;
                                    this._selected.expandUp();
                                    this.useEvent();
                                }
                            }
                        }
                        else if (keyCode == UnityEngine_1.KeyCode.DownArrow) {
                            let sibling = this.findNextNode(this._selected);
                            if (sibling) {
                                this._selected = sibling;
                                this._selected.expandUp();
                                this.useEvent();
                            }
                        }
                        else if (keyCode == UnityEngine_1.KeyCode.LeftArrow) {
                            if (this._selected.expanded && this._selected.isFolder) {
                                this._selected.expanded = false;
                                this._selected.expandUp();
                            }
                            else if (this._selected.parent) {
                                this._selected = this._selected.parent;
                                this._selected.expandUp();
                            }
                            this.useEvent();
                        }
                        else if (keyCode == UnityEngine_1.KeyCode.RightArrow) {
                            this._selected.expanded = true;
                            this._selected.expandUp();
                            this.useEvent();
                        }
                    }
                }
            }
        }
        else {
            if (!this._editing && this._controlEventType == UnityEngine_1.EventType.MouseUp) {
                this._tempRect.Set(0, 0, width, height);
                if (this._tempRect.Contains(this._controlMousePos)) {
                    UnityEngine_1.GUIUtility.keyboardControl = this._controlID;
                    repaint = true;
                }
            }
        }
        return this._deferredMenuPopup || repaint;
    }
    calcRowHeight(node) {
        this._height += node.calcRowHeight();
        if (node.expanded) {
            for (let i = 0, count = node.childCount; i < count; i++) {
                this.calcRowHeight(node.getChildByIndex(i));
            }
        }
    }
    calcSearchResultsHeight(node) {
        if (node.isMatch) {
            this._height += node.calcRowHeight();
        }
        for (let i = 0, count = node.childCount; i < count; i++) {
            this.calcRowHeight(node.getChildByIndex(i));
        }
    }
    setControlRect(cEvent) {
        this._controlRect = UnityEditor_1.EditorGUILayout.GetControlRect(false, this._height, UnityEngine_1.GUILayout.MinWidth(160));
        this._controlID = UnityEngine_1.GUIUtility.GetControlID(UnityEngine_1.FocusType.Keyboard, this._controlRect);
        this._controlEventType = cEvent.GetTypeForControl(this._controlID);
        if (this._controlEventType == UnityEngine_1.EventType.MouseUp) {
            this._controlMousePos = cEvent.mousePosition;
        }
    }
    useEvent() {
        this._eventUsed = true;
        UnityEngine_1.GUI.changed = true;
        UnityEngine_1.Event.current.Use();
    }
    drawSearchResults(node, depth, offsetY, height) {
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
                    UnityEditor_1.EditorGUI.DrawRect(this._rowRect, this._selectionColor);
                }
                else if (this._rowIndex % 2) {
                    UnityEditor_1.EditorGUI.DrawRect(this._rowRect, this._rowColor);
                }
                node.draw(this._indentRect, bSelected, bSelected && this._editing, this._indentSize);
                if (this._controlEventType == UnityEngine_1.EventType.MouseUp) {
                    if (this._rowRect.Contains(this._controlMousePos)) {
                        if (UnityEngine_1.Event.current.button == 1) {
                            if (this._selected == node) {
                                node.drawMenu(this, this._controlMousePos, this._handler);
                                this.useEvent();
                            }
                            else {
                                this.selected = node;
                                if (!this._editing) {
                                    this._deferredMenuPopup = true;
                                }
                                this.useEvent();
                            }
                        }
                        else if (UnityEngine_1.Event.current.button == 0) {
                            if (node.isFolder && node._foldoutRect.Contains(this._controlMousePos)) {
                                node.expanded = !node.expanded;
                            }
                            else {
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
    drawRow(node, depth, offsetY, height) {
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
                UnityEditor_1.EditorGUI.DrawRect(this._rowRect, this._selectionColor);
            }
            else if (this._rowIndex % 2) {
                UnityEditor_1.EditorGUI.DrawRect(this._rowRect, this._rowColor);
            }
            node.draw(this._indentRect, bSelected, bSelected && this._editing, this._indentSize);
            if (this._controlEventType == UnityEngine_1.EventType.MouseUp) {
                if (this._rowRect.Contains(this._controlMousePos)) {
                    if (UnityEngine_1.Event.current.button == 1) {
                        if (this._selected == node) {
                            node.drawMenu(this, this._controlMousePos, this._handler);
                            this.useEvent();
                        }
                        else {
                            this.selected = node;
                            if (!this._editing) {
                                this._deferredMenuPopup = true;
                            }
                            this.useEvent();
                        }
                    }
                    else if (UnityEngine_1.Event.current.button == 0) {
                        if (node.isFolder && node._foldoutRect.Contains(this._controlMousePos)) {
                            node.expanded = !node.expanded;
                        }
                        else {
                            this.selected = node;
                        }
                        this.useEvent();
                    }
                }
            }
        }
        else {
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
    findPreviousNode(node) {
        let sibling = node.parent.findLastSibling(node);
        while (sibling && sibling.expanded && sibling.childCount > 0) {
            sibling = sibling.getLastChild();
        }
        return sibling || node.parent;
    }
    findNextNode(node) {
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
exports.UTreeView = UTreeView;
UTreeView.CONTEXT_MENU = "CONTEXT_MENU";
//# sourceMappingURL=treeview.js.map