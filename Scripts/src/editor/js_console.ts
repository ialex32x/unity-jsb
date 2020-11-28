import { EditorGUILayout, EditorWindow } from "UnityEditor";
import { Event, EventType, GUI, GUIContent, GUILayout, KeyCode } from "UnityEngine";

export function fillAutoCompletion(scope: any, pattern: string): Array<string> {
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

    let result: Array<string> = [];
    for (let k in scope) {
        if (k.indexOf(pattern) == 0) {
            result.push(head + left + k);
        }
    }

    return result;
}

export class JSConsole extends EditorWindow {
    private _code: string = "";
    private _suggestions: Array<string> = [];
    private _historyIndex = -1;
    private _history: Array<string> = [];

    OnEnable() {
        this.titleContent = new GUIContent("Javascript Console");
    }

    OnGUI() {
        let evt = Event.current;
        let code = EditorGUILayout.TextField("Eval", this._code);

        for (let s of this._suggestions) {
            EditorGUILayout.LabelField(s);
        }

        if (evt.type == EventType.KeyUp) {
            switch (evt.keyCode) {
                case KeyCode.Return: {
                    if (code != null && code.length > 0) {
                        try {
                            let rval = eval(code);
                            console.log(JSON.stringify(rval));
                        } catch (e) {
                            console.error(e);
                        }
                        this._history.push(code);
                        this._code = code = "";
                        this.Repaint();
                    }
                    break;
                }
                case KeyCode.UpArrow: {
                    if (evt.alt && this._history.length > 0) {
                        if (this._historyIndex == -1) {
                            this._historyIndex = this._history.length - 1;
                        } else {
                            if (this._historyIndex > 0) {
                                this._historyIndex--;
                            } else {
                                this._historyIndex = this._history.length - 1;
                            }
                        }
                        code = this._history[this._historyIndex];
                        GUI.FocusControl("DUMMY");
                        this.Repaint();
                    }
                }
            }
        }

        if (this._code != code) {
            this._code = code;
            this._suggestions = fillAutoCompletion(globalThis, code);
        }

        GUI.SetNextControlName("DUMMY")
        GUILayout.Label(`${this._historyIndex}/${this._history.length}`);
    }
}
