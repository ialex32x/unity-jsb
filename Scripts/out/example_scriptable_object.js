"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.MyScriptableObject = void 0;
const UnityEngine_1 = require("UnityEngine");
const editor_decorators_1 = require("./plover/editor/editor_decorators");
let MyScriptableObject = class MyScriptableObject extends UnityEngine_1.ScriptableObject {
    constructor() {
        super(...arguments);
        this.value1 = 1;
        this.value2 = "hello";
    }
};
__decorate([
    editor_decorators_1.ScriptNumber()
], MyScriptableObject.prototype, "value1", void 0);
__decorate([
    editor_decorators_1.ScriptString()
], MyScriptableObject.prototype, "value2", void 0);
MyScriptableObject = __decorate([
    editor_decorators_1.ScriptAsset()
], MyScriptableObject);
exports.MyScriptableObject = MyScriptableObject;
//TODO: 加載还没实现
// let js_data = Resources.Load("data/js_data");
//# sourceMappingURL=example_scriptable_object.js.map