"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.JSConsole = exports.fillAutoCompletion = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
function fillAutoCompletion(scope, pattern) {
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
    let result = [];
    for (let k in scope) {
        if (k.indexOf(pattern) == 0) {
            result.push(head + left + k);
        }
    }
    return result;
}
exports.fillAutoCompletion = fillAutoCompletion;
class JSConsole extends UnityEditor_1.EditorWindow {
    constructor() {
        super(...arguments);
        this._code = "";
        this._suggestions = [];
        this._historyIndex = -1;
        this._history = [];
    }
    OnEnable() {
        this.titleContent = new UnityEngine_1.GUIContent("Javascript Console");
    }
    OnGUI() {
        let evt = UnityEngine_1.Event.current;
        let code = UnityEditor_1.EditorGUILayout.TextField("Eval", this._code);
        for (let s of this._suggestions) {
            UnityEditor_1.EditorGUILayout.LabelField(s);
        }
        if (evt.type == UnityEngine_1.EventType.KeyUp) {
            switch (evt.keyCode) {
                case UnityEngine_1.KeyCode.Return: {
                    if (code != null && code.length > 0) {
                        try {
                            let rval = eval(code);
                            console.log(JSON.stringify(rval));
                        }
                        catch (e) {
                            console.error(e);
                        }
                        this._history.push(code);
                        this._code = code = "";
                        this.Repaint();
                    }
                    break;
                }
                case UnityEngine_1.KeyCode.UpArrow: {
                    if (evt.alt && this._history.length > 0) {
                        if (this._historyIndex == -1) {
                            this._historyIndex = this._history.length - 1;
                        }
                        else {
                            if (this._historyIndex > 0) {
                                this._historyIndex--;
                            }
                            else {
                                this._historyIndex = this._history.length - 1;
                            }
                        }
                        code = this._history[this._historyIndex];
                        UnityEngine_1.GUI.FocusControl("DUMMY");
                        this.Repaint();
                    }
                }
            }
        }
        if (this._code != code) {
            this._code = code;
            this._suggestions = fillAutoCompletion(globalThis, code);
        }
        UnityEngine_1.GUI.SetNextControlName("DUMMY");
        UnityEngine_1.GUILayout.Label(`${this._historyIndex}/${this._history.length}`);
    }
}
exports.JSConsole = JSConsole;
//# sourceMappingURL=js_console.js.map