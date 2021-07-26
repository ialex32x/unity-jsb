"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.MenuBuilder = exports.MenuDisabledAction = exports.MenuAction = exports.MenuSeparator = exports.MenuAbstractItem = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
class MenuAbstractItem {
    constructor(name) {
        this._name = name;
    }
    get name() { return this._name; }
}
exports.MenuAbstractItem = MenuAbstractItem;
class MenuSeparator extends MenuAbstractItem {
    build(menu) {
        menu.AddSeparator(this.name);
    }
}
exports.MenuSeparator = MenuSeparator;
class MenuAction extends MenuAbstractItem {
    constructor(name, action) {
        super(name);
        this._action = action;
    }
    get action() { return this._action; }
    build(menu) {
        let content = new UnityEngine_1.GUIContent(this.name);
        menu.AddItem(content, false, () => this._action());
    }
}
exports.MenuAction = MenuAction;
class MenuDisabledAction extends MenuAbstractItem {
    constructor(name) {
        super(name);
    }
    build(menu) {
        let content = new UnityEngine_1.GUIContent(this.name);
        menu.AddDisabledItem(content, false);
    }
}
exports.MenuDisabledAction = MenuDisabledAction;
class MenuBuilder {
    constructor() {
        this._items = [];
    }
    addAction(name, action, isDisabled = false) {
        if (isDisabled) {
            return this.addDisabledAction(name);
        }
        this._items.push(new MenuAction(name, action));
    }
    addDisabledAction(name) {
        this._items.push(new MenuDisabledAction(name));
    }
    addSeperator() {
        this._items.push(new MenuSeparator(""));
    }
    build() {
        let count = this._items.length;
        if (count > 0) {
            let menu = new UnityEditor_1.GenericMenu();
            for (let i = 0; i < count; i++) {
                let item = this._items[i];
                item.build(menu);
            }
            return menu;
        }
        return null;
    }
}
exports.MenuBuilder = MenuBuilder;
//# sourceMappingURL=menu_builder.js.map