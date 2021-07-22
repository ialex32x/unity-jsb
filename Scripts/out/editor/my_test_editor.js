"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.MyTestEditorWindow = void 0;
const jsb_1 = require("jsb");
const UnityEditor_1 = require("UnityEditor");
const editor_decorators_1 = require("../plover/editor/editor_decorators");
let MyTestEditorWindow = class MyTestEditorWindow extends UnityEditor_1.EditorWindow {
    constructor() {
        super(...arguments);
        this._testString = "";
    }
    Awake() {
        jsb_1.AddCacheString("Test");
        jsb_1.AddCacheString("");
    }
    OnGUI() {
        this._testString = UnityEditor_1.EditorGUILayout.TextField("Test", this._testString) || "";
    }
};
MyTestEditorWindow = __decorate([
    editor_decorators_1.ScriptEditorWindow()
], MyTestEditorWindow);
exports.MyTestEditorWindow = MyTestEditorWindow;
//# sourceMappingURL=my_test_editor.js.map