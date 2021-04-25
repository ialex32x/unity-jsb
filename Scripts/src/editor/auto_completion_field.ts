
/*
https://github.com/marijnz/unity-autocomplete-search-field
*/

import { EditorStyles, EditorWindow } from "UnityEditor";
import { SearchField } from "UnityEditor.IMGUI.Controls";
import { Event, EventType, GUI, GUILayout, GUILayoutUtility, GUIStyle, GUIUtility, KeyCode, Rect, TextAnchor, Vector2 } from "UnityEngine";
import { EventDispatcher } from "../events/dispatcher";

let Styles = {
    resultHeight: 20,
    resultsBorderWidth: 2,
    resultsMargin: 15,
    resultsLabelOffset: 2,

    entryEven: GUIStyle.op_Implicit("CN EntryBackEven"),
    entryOdd: GUIStyle.op_Implicit("CN EntryBackOdd"),
    labelStyle: new GUIStyle(EditorStyles.label),
    resultsBorderStyle: GUIStyle.op_Implicit("hostview"),
};

Styles.labelStyle.alignment = TextAnchor.MiddleLeft;
Styles.labelStyle.richText = true;

export class AutoCompletionField extends EventDispatcher {
    searchString: string = "";
    maxResults = 15;

    private results: Array<string> = [];
    private selectedIndex = -1;
    private searchField: SearchField;

    private previousMousePosition = Vector2.zero;
    private selectedIndexByMouse = false;
    private showResults = false;

    constructor() {
        super();
    }

    public addResult(result: string) {
        this.results.push(result);
    }

    public clearResults() {
        this.results.splice(0)
    }

    public onToolbarGUI() {
        this.draw(true);
    }

    public onGUI() {
        this.draw(false);
    }

    private draw(asToolbar: boolean) {
        let rect = GUILayoutUtility.GetRect(1, 1, 18, 18, GUILayout.ExpandWidth(true));
        GUILayout.BeginHorizontal();
        this.doSearchField(rect, asToolbar);
        GUILayout.EndHorizontal();
        rect.y += 18;
        this.doResults(rect);
    }

    private doSearchField(rect: Rect, asToolbar: boolean) {
        if (this.searchField == null) {
            this.searchField = new SearchField();
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

    private onDownOrUpArrowKeyPressed() {
        let current = Event.current;

        if (current.keyCode == KeyCode.UpArrow) {
            current.Use();
            this.selectedIndex--;
            this.selectedIndexByMouse = false;
        }
        else {
            current.Use();
            this.selectedIndex++;
            this.selectedIndexByMouse = false;
        }

        if (this.selectedIndex >= this.results.length) this.selectedIndex = this.results.length - 1;
        else if (this.selectedIndex < 0) this.selectedIndex = -1;
    }

    private doResults(rect: Rect) {
        if (this.results.length <= 0 || !this.showResults) return;

        var current = Event.current;
        rect.height = Styles.resultHeight * Math.min(this.maxResults, this.results.length);
        rect.x = Styles.resultsMargin;
        rect.width -= Styles.resultsMargin * 2;

        var elementRect = new Rect(rect);

        rect.height += Styles.resultsBorderWidth;
        GUI.Label(rect, "", Styles.resultsBorderStyle);

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
            if (current.type == EventType.Repaint) {
                var style = i % 2 == 0 ? Styles.entryOdd : Styles.entryEven;

                style.Draw(elementRect, false, false, i == this.selectedIndex, false);

                var labelRect = new Rect(elementRect);
                labelRect.x += Styles.resultsLabelOffset;
                GUI.Label(labelRect, this.results[i], Styles.labelStyle);
            }
            if (elementRect.Contains(current.mousePosition)) {
                if (movedMouseInRect) {
                    this.selectedIndex = i;
                    this.selectedIndexByMouse = true;
                    didJustSelectIndex = true;
                }
                if (current.type == EventType.MouseDown) {
                    this.onConfirm(this.results[i]);
                }
            }
            elementRect.y += Styles.resultHeight;
        }

        if (current.type == EventType.Repaint && !didJustSelectIndex && !mouseIsInResultsRect && this.selectedIndexByMouse) {
            this.selectedIndex = -1;
        }

        if ((GUIUtility.hotControl != this.searchField.searchFieldControlID && GUIUtility.hotControl > 0)
            || (current.rawType == EventType.MouseDown && !mouseIsInResultsRect)) {
            this.showResults = false;
        }

        if (current.type == EventType.KeyUp && current.keyCode == KeyCode.Return && this.selectedIndex >= 0) {
            this.onConfirm(this.results[this.selectedIndex]);
        }

        if (current.type == EventType.Repaint) {
            this.previousMousePosition = current.mousePosition;
        }
    }

    private onConfirm(result: string) {
        this.searchString = result;
        this.dispatch("confirm", result)
        this.dispatch("change", result);
        this.repaintFocusedWindow();
        GUIUtility.keyboardControl = 0; // To avoid Unity sometimes not updating the search field text
    }

    private hasSearchbarFocused(): boolean {
        return GUIUtility.keyboardControl == this.searchField.searchFieldControlID;
    }

    private repaintFocusedWindow() {
        if (EditorWindow.focusedWindow != null) {
            EditorWindow.focusedWindow.Repaint();
        }
    }
}
