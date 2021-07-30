"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.JSXWidgetBridge = void 0;
const UnityEngine_1 = require("UnityEngine");
const class_decorators_1 = require("../runtime/class_decorators");
let JSXWidgetBridge = class JSXWidgetBridge extends UnityEngine_1.MonoBehaviour {
    get data() { return null; }
    OnDestroy() {
        if (this._widget) {
            this._widget.destroy();
        }
    }
};
JSXWidgetBridge = __decorate([
    class_decorators_1.ScriptType()
], JSXWidgetBridge);
exports.JSXWidgetBridge = JSXWidgetBridge;
//# sourceMappingURL=bridge.js.map