import { EditorGUILayout, EditorWindow } from "UnityEditor";
import { Event, EventType, GUI, GUIContent, GUILayout, KeyCode, Rect } from "UnityEngine";
import { AutoCompletionField } from "./auto_completion_field";

export function fillAutoCompletion(scope: any, pattern: string): Array<string> {
    let result: Array<string> = [];

    if (typeof pattern !== "string") {
        return result;
    }
    
    let head = '';

    pattern.replace(/\\W*([\\w\\.]+)$/, (a, b, c) => {
        head = pattern.substr(0, c + a.length - b.length);
        pattern = b;
        return b;
    });
    let index = pattern.lastIndexOf('.');
    let left = '';

    if (index >= 0) {
        left = pattern.substr(0, index + 1);
        try {
            scope = eval(pattern.substr(0, index));
        }
        catch (e) {
            scope = null;
        }
        pattern = pattern.substr(index + 1);
    }

    for (let k in scope) {
        if (k.indexOf(pattern) == 0) {
            result.push(head + left + k);
        }
    }

    return result;
}

export class JSConsole extends EditorWindow {
    private _searchField = new AutoCompletionField();
    private _history: Array<string> = [];

    Awake() {
        this._searchField.on("change", this, this.onSearchChange);
        this._searchField.on("confirm", this, this.onSearchConfirm);
    }

    private onSearchChange(s: string) {
        this._searchField.clearResults();
        fillAutoCompletion(globalThis, s).forEach(element => {
            if (element != s) {
                this._searchField.addResult(element);
            }
        });
    }

    private onSearchConfirm(s: string) {
        console.log("confirm:", s);
    }

    OnEnable() {
        this.titleContent = new GUIContent("Javascript Console");
    }

    OnGUI() {
        let evt = Event.current;

        this._searchField.onGUI();

        if (evt.type == EventType.KeyUp) {
            switch (evt.keyCode) {
                case KeyCode.Return: {
                    let code = this._searchField.searchString;
                    if (code != null && code.length > 0) {
                        try {
                            let rval = eval(code);
                            console.log(JSON.stringify(rval));
                        } catch (e) {
                            console.error(e);
                        }
                        // this._history.push(code);
                    }
                    
                    break;
                }
            }
        }

        // GUI.Box(new Rect(0, 50, 300, 100), this._history.join("\n"));
    }
}
