import { GenericMenu } from "UnityEditor";
import { GUIContent } from "UnityEngine";

export interface IMenuItem {
    name: string;
    build(menu: GenericMenu);
}

export abstract class MenuAbstractItem implements IMenuItem {
    private _name: string;

    get name() { return this._name; }

    constructor(name: string) {
        this._name = name;
    }

    abstract build(menu: GenericMenu);
}

export class MenuSeparator extends MenuAbstractItem {
    build(menu: GenericMenu) {
        menu.AddSeparator(this.name);
    }
}

export class MenuAction extends MenuAbstractItem {
    private _action: Function;

    get action() { return this._action; }

    constructor(name: string, action: Function) {
        super(name);
        this._action = action;
    }

    build(menu: GenericMenu) {
        let content = new GUIContent(this.name);
        menu.AddItem(content, false, () => this._action());
    }
}

export class MenuDisabledAction extends MenuAbstractItem {
    constructor(name: string) {
        super(name);
    }

    build(menu: GenericMenu) {
        let content = new GUIContent(this.name);
        menu.AddDisabledItem(content, false);
    }
}

export class MenuBuilder {
    private _items: Array<IMenuItem> = [];

    addAction(name: string, action: Function, isDisabled: boolean = false) {
        if (isDisabled) {
            return this.addDisabledAction(name);
        }
        this._items.push(new MenuAction(name, action));
    }

    addDisabledAction(name: string) {
        this._items.push(new MenuDisabledAction(name));
    }

    addSeperator() {
        this._items.push(new MenuSeparator(""));
    }

    build() {
        let count = this._items.length;

        if (count > 0) {
            let menu = new GenericMenu();
            for (let i = 0; i < count; i++) {
                let item = this._items[i];
                item.build(menu);
            }

            return menu;
        }

        return null;
    }
}
