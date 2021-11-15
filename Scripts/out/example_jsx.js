"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.MyWidgetTest = void 0;
const UnityEngine_1 = require("UnityEngine");
const bridge_1 = require("plover/jsx/bridge");
const JSX = require("plover/jsx/element");
const vue_1 = require("plover/jsx/vue");
const class_decorators_1 = require("plover/runtime/class_decorators");
let MyWidgetTest = class MyWidgetTest extends bridge_1.JSXWidgetBridge {
    get data() { return this._data; }
    Awake() {
        this._data = vue_1.ViewModel.create({ name: "Unity", tick: 0 });
        this._widget =
            JSX.createElement("widget", { class: this },
                JSX.createElement("text", { name: "label", text: "Hello {{this.name}} {{this.tick}}" }));
        this._timer = setInterval(() => {
            this._data.tick++;
        }, 1000);
        this._data.tick++;
    }
    OnDestroy() {
        super.OnDestroy();
        clearInterval(this._timer);
    }
};
MyWidgetTest = __decorate([
    class_decorators_1.ScriptType()
], MyWidgetTest);
exports.MyWidgetTest = MyWidgetTest;
let parent = UnityEngine_1.GameObject.Find("/Canvas").transform;
let go = UnityEngine_1.Object.Instantiate(UnityEngine_1.Resources.Load("prefab/jsx_test_ui"));
go.transform.SetParent(parent);
let rect = go.GetComponent(UnityEngine_1.RectTransform);
rect.anchorMin = UnityEngine_1.Vector2.zero;
rect.anchorMax = UnityEngine_1.Vector2.one;
rect.anchoredPosition = UnityEngine_1.Vector2.zero;
console.log("load", go, go.AddComponent(MyWidgetTest));
//# sourceMappingURL=example_jsx.js.map