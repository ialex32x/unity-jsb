"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.JSConsole = exports.fillAutoCompletion = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
const auto_completion_field_1 = require("./auto_completion_field");
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
        this._searchField = new auto_completion_field_1.AutoCompletionField();
        this._history = [];
    }
    Awake() {
        this._searchField.on("change", this, this.onSearchChange);
        this._searchField.on("confirm", this, this.onSearchConfirm);
    }
    onSearchChange(s) {
        this._searchField.clearResults();
        fillAutoCompletion(globalThis, s).forEach(element => {
            if (element != s) {
                this._searchField.addResult(element);
            }
        });
    }
    onSearchConfirm(s) {
        console.log("confirm:", s);
    }
    OnEnable() {
        this.titleContent = new UnityEngine_1.GUIContent("Javascript Console");
    }
    OnGUI() {
        let evt = UnityEngine_1.Event.current;
        this._searchField.onGUI();
        if (evt.type == UnityEngine_1.EventType.KeyUp) {
            switch (evt.keyCode) {
                case UnityEngine_1.KeyCode.Return: {
                    let code = this._searchField.searchString;
                    if (code != null && code.length > 0) {
                        try {
                            let rval = eval(code);
                            console.log(JSON.stringify(rval));
                        }
                        catch (e) {
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
exports.JSConsole = JSConsole;
//# sourceMappingURL=js_console.js.map