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
const serialize_1 = require("./plover/editor/serialize");
let MyScriptableObject = class MyScriptableObject extends UnityEngine_1.ScriptableObject {
    constructor() {
        super(...arguments);
        this.value1 = 1;
        this.value2 = "hello";
        this.value3 = UnityEngine_1.Vector3.zero;
    }
    Reset() {
        this.value1 = 0;
        this.value2 = "";
        this.value3 = UnityEngine_1.Vector3.zero;
    }
};
__decorate([
    editor_decorators_1.ScriptNumber()
], MyScriptableObject.prototype, "value1", void 0);
__decorate([
    editor_decorators_1.ScriptString()
], MyScriptableObject.prototype, "value2", void 0);
__decorate([
    editor_decorators_1.ScriptProperty({ type: serialize_1.As.Vector3 })
], MyScriptableObject.prototype, "value3", void 0);
MyScriptableObject = __decorate([
    editor_decorators_1.ScriptAsset()
], MyScriptableObject);
exports.MyScriptableObject = MyScriptableObject;
if (require.main == module) {
    let js_data = UnityEngine_1.Resources.Load("data/js_data");
    if (js_data) {
        console.log("type check:", js_data instanceof MyScriptableObject);
        console.log("type values:", js_data.value1, js_data.value2);
    }
    else {
        console.error("failed to load js_data, please create the asset at first.");
    }
}
//# sourceMappingURL=example_scriptable_object.js.map