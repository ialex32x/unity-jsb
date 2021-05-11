"use strict";
/*
https://github.com/marijnz/unity-autocomplete-search-field
*/
Object.defineProperty(exports, "__esModule", { value: true });
exports.AutoCompletionField = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEditor_IMGUI_Controls_1 = require("UnityEditor.IMGUI.Controls");
const UnityEngine_1 = require("UnityEngine");
const dispatcher_1 = require("../events/dispatcher");
let Styles = {
    resultHeight: 20,
    resultsBorderWidth: 2,
    resultsMargin: 15,
    resultsLabelOffset: 2,
    entryEven: UnityEngine_1.GUIStyle.op_Implicit("CN EntryBackEven"),
    entryOdd: UnityEngine_1.GUIStyle.op_Implicit("CN EntryBackOdd"),
    labelStyle: new UnityEngine_1.GUIStyle(UnityEditor_1.EditorStyles.label),
    resultsBorderStyle: UnityEngine_1.GUIStyle.op_Implicit("hostview"),
};
Styles.labelStyle.alignment = UnityEngine_1.TextAnchor.MiddleLeft;
Styles.labelStyle.richText = true;
class AutoCompletionField extends dispatcher_1.EventDispatcher {
    constructor() {
        super();
        this.searchString = "";
        this.maxResults = 15;
        this.results = [];
        this.selectedIndex = -1;
        this.previousMousePosition = UnityEngine_1.Vector2.zero;
        this.selectedIndexByMouse = false;
        this.showResults = false;
    }
    addResult(result) {
        this.results.push(result);
    }
    clearResults() {
        this.results.splice(0);
    }
    onToolbarGUI() {
        this.draw(true);
    }
    onGUI() {
        this.draw(false);
    }
    draw(asToolbar) {
        let rect = UnityEngine_1.GUILayoutUtility.GetRect(1, 1, 18, 18, UnityEngine_1.GUILayout.ExpandWidth(true));
        UnityEngine_1.GUILayout.BeginHorizontal();
        this.doSearchField(rect, asToolbar);
        UnityEngine_1.GUILayout.EndHorizontal();
        rect.y += 18;
        this.doResults(rect);
    }
    doSearchField(rect, asToolbar) {
        if (this.searchField == null) {
            this.searchField = new UnityEditor_IMGUI_Controls_1.SearchField();
            this.searchField.downOrUpArrowKeyPressed("add", this.onDownOrUpArrowKeyPressed.bind(this));
        }
        var result = asToolbar
            ? this.searchField.OnToolbarGUI(rect, this.searchString)
            : this.searchField.OnGUI(rect, this.searchString);
        if (typeof result === "string") {
            if (result != this.searchString) {
                this.dispatch("change", result);
                this.selectedIndex = -1;
                this.showResults = true;
            }
            this.searchString = result;
            if (this.hasSearchbarFocused()) {
                this.repaintFocusedWindow();
            }
        }
    }
    onDownOrUpArrowKeyPressed() {
        let current = UnityEngine_1.Event.current;
        if (current.keyCode == UnityEngine_1.KeyCode.UpArrow) {
            current.Use();
            this.selectedIndex--;
            this.selectedIndexByMouse = false;
        }
        else {
            current.Use();
            this.selectedIndex++;
            this.selectedIndexByMouse = false;
        }
        if (this.selectedIndex >= this.results.length)
            this.selectedIndex = this.results.length - 1;
        else if (this.selectedIndex < 0)
            this.selectedIndex = -1;
    }
    doResults(rect) {
        if (this.results.length <= 0 || !this.showResults)
            return;
        var current = UnityEngine_1.Event.current;
        rect.height = Styles.resultHeight * Math.min(this.maxResults, this.results.length);
        rect.x = Styles.resultsMargin;
        rect.width -= Styles.resultsMargin * 2;
        var elementRect = new UnityEngine_1.Rect(rect);
        rect.height += Styles.resultsBorderWidth;
        UnityEngine_1.GUI.Label(rect, "", Styles.resultsBorderStyle);
        var mouseIsInResultsRect = rect.Contains(current.mousePosition);
        if (mouseIsInResultsRect) {
            this.repaintFocusedWindow();
        }
        var movedMouseInRect = this.previousMousePosition != current.mousePosition;
        elementRect.x += Styles.resultsBorderWidth;
        elementRect.width -= Styles.resultsBorderWidth * 2;
        elementRect.height = Styles.resultHeight;
        var didJustSelectIndex = false;
        for (var i = 0; i < this.results.length && i < this.maxResults; i++) {
            if (current.type == UnityEngine_1.EventType.Repaint) {
                var style = i % 2 == 0 ? Styles.entryOdd : Styles.entryEven;
                style.Draw(elementRect, false, false, i == this.selectedIndex, false);
                var labelRect = new UnityEngine_1.Rect(elementRect);
                labelRect.x += Styles.resultsLabelOffset;
                UnityEngine_1.GUI.Label(labelRect, this.results[i], Styles.labelStyle);
            }
            if (elementRect.Contains(current.mousePosition)) {
                if (movedMouseInRect) {
                    this.selectedIndex = i;
                    this.selectedIndexByMouse = true;
                    didJustSelectIndex = true;
                }
                if (current.type == UnityEngine_1.EventType.MouseDown) {
                    this.onConfirm(this.results[i]);
                }
            }
            elementRect.y += Styles.resultHeight;
        }
        if (current.type == UnityEngine_1.EventType.Repaint && !didJustSelectIndex && !mouseIsInResultsRect && this.selectedIndexByMouse) {
            this.selectedIndex = -1;
        }
        if ((UnityEngine_1.GUIUtility.hotControl != this.searchField.searchFieldControlID && UnityEngine_1.GUIUtility.hotControl > 0)
            || (current.rawType == UnityEngine_1.EventType.MouseDown && !mouseIsInResultsRect)) {
            this.showResults = false;
        }
        if (current.type == UnityEngine_1.EventType.KeyUp && current.keyCode == UnityEngine_1.KeyCode.Return && this.selectedIndex >= 0) {
            this.onConfirm(this.results[this.selectedIndex]);
        }
        if (current.type == UnityEngine_1.EventType.Repaint) {
            this.previousMousePosition = current.mousePosition;
        }
    }
    onConfirm(result) {
        this.searchString = result;
        this.dispatch("confirm", result);
        this.dispatch("change", result);
        this.repaintFocusedWindow();
        UnityEngine_1.GUIUtility.keyboardControl = 0; // To avoid Unity sometimes not updating the search field text
    }
    hasSearchbarFocused() {
        return UnityEngine_1.GUIUtility.keyboardControl == this.searchField.searchFieldControlID;
    }
    repaintFocusedWindow() {
        if (UnityEditor_1.EditorWindow.focusedWindow != null) {
            UnityEditor_1.EditorWindow.focusedWindow.Repaint();
        }
    }
}
exports.AutoCompletionField = AutoCompletionField;
//# sourceMappingURL=auto_completion_field.js.map