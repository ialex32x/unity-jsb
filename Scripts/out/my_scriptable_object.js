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
const class_decorators_1 = require("plover/runtime/class_decorators");
let MyScriptableObject = class MyScriptableObject extends UnityEngine_1.ScriptableObject {
    constructor() {
        super(...arguments);
        this.value1 = 1;
        this.value2 = "hello";
        this.value3 = UnityEngine_1.Vector3.zero;
        this.value5 = [];
    }
    Process() {
        return `${this.value2} ${this.value3}`;
    }
    Reset() {
        this.value1 = 0;
        this.value2 = "";
        this.value3 = UnityEngine_1.Vector3.zero;
        this.value5 = [1, 2, 3];
    }
};
__decorate([
    class_decorators_1.ScriptNumber()
], MyScriptableObject.prototype, "value1", void 0);
__decorate([
    class_decorators_1.ScriptString()
], MyScriptableObject.prototype, "value2", void 0);
__decorate([
    class_decorators_1.ScriptProperty({ type: "Vector3" })
], MyScriptableObject.prototype, "value3", void 0);
__decorate([
    class_decorators_1.ScriptNumber()
], MyScriptableObject.prototype, "value5", void 0);
MyScriptableObject = __decorate([
    class_decorators_1.ScriptAsset()
], MyScriptableObject);
exports.MyScriptableObject = MyScriptableObject;
//# sourceMappingURL=my_scriptable_object.js.map