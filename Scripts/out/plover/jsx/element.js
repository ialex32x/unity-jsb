"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.JSXText = exports.JSXWidget = exports.JSXCompoundNode = exports.registerElement = exports.createElement = exports.element = exports.findUIComponent = exports.JSXNode = void 0;
const UnityEngine_UI_1 = require("UnityEngine.UI");
const vue_1 = require("./vue");
let elementActivators = {};
class JSXNode {
    get parent() { return this._parent; }
    set parent(value) {
        if (this._parent != value) {
            this._parent = value;
            this.onParentSet();
        }
    }
    get widget() {
        let p = this._parent;
        while (p) {
            if (p instanceof JSXWidget) {
                return p;
            }
            p = p._parent;
        }
    }
}
exports.JSXNode = JSXNode;
function findUIComponent(transform, name, type) {
    let n = transform.childCount;
    for (let i = 0; i < n; i++) {
        let child = transform.GetChild(i);
        if (child.name == name) {
            let com = child.GetComponent(type);
            if (com) {
                return com;
            }
        }
        let com = findUIComponent(child, name, type);
        if (com) {
            return com;
        }
    }
}
exports.findUIComponent = findUIComponent;
function element(name) {
    return function (target) {
        registerElement(name, target);
    };
}
exports.element = element;
function createElement(name, attributes, ...children) {
    let act = elementActivators[name];
    if (typeof act !== "undefined") {
        let element = new act();
        element.init(attributes, ...children);
        return element;
    }
}
exports.createElement = createElement;
function registerElement(name, activator) {
    elementActivators[name] = activator;
}
exports.registerElement = registerElement;
class JSXCompoundNode extends JSXNode {
    init(attributes, ...children) {
        this._children = children;
        for (let i = 0; i < this._children.length; i++) {
            let child = this._children[i];
            child.parent = this;
        }
    }
    evaluate() {
        for (let i = 0; i < this._children.length; i++) {
            let child = this._children[i];
            child.evaluate();
        }
    }
    destroy() {
        for (let i = 0; i < this._children.length; i++) {
            let child = this._children[i];
            child.destroy();
        }
    }
}
exports.JSXCompoundNode = JSXCompoundNode;
// export interface IWidgetInstance {
//     readonly gameObject: GameObject;
//     readonly data: any;
// }
let JSXWidget = class JSXWidget extends JSXCompoundNode {
    get instance() { return this._instance; }
    get data() { return this._instance.data; }
    init(attributes, ...children) {
        this._instance = attributes.class;
        super.init(attributes, ...children);
    }
    onParentSet() {
    }
};
JSXWidget = __decorate([
    element("widget")
], JSXWidget);
exports.JSXWidget = JSXWidget;
let JSXText = class JSXText extends JSXNode {
    init(attributes, ...children) {
        if (attributes) {
            this._name = attributes.name;
            this._text = attributes.text;
        }
    }
    onParentSet() {
        this._component = findUIComponent(this.widget.instance.transform, this._name, UnityEngine_UI_1.Text);
        this._watcher = vue_1.ViewModel.expression(this.widget.data, this._text, this.onValueChanged.bind(this));
    }
    onValueChanged(value) {
        this._component.text = value;
    }
    evaluate() {
        if (this._watcher) {
            this._watcher.evaluate();
        }
    }
    destroy() {
        if (this._watcher) {
            this._watcher.teardown();
            this._watcher = null;
        }
    }
};
JSXText = __decorate([
    element("text")
], JSXText);
exports.JSXText = JSXText;
//# sourceMappingURL=element.js.map