import { ViewModel } from "./vue";

export interface Activator<T = JSXNode> {
    new(): T;
}

let elementActivators: { [key: string]: Activator } = {};

export abstract class JSXNode {
    abstract init(attributes: any, ...children: Array<JSXNode>);
    abstract evaluate();
    abstract destroy();
}

export function createElement(name: string, attributes: any, ...children: Array<JSXNode>): JSXNode {
    let act = elementActivators[name];

    if (typeof act !== "undefined") {
        let element = new act();
        element.init(attributes, ...children);
        return element;
    }
}

export function registerElement(name: string, activator: Activator) {
    elementActivators[name] = activator;
}

export class JSXCompoundNode extends JSXNode {
    private _children: Array<JSXNode>;

    init(attributes: any, ...children: Array<JSXNode>) {
        this._children = children;
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

export class JSXWidget extends JSXCompoundNode {
}

export class JSXText extends JSXNode {
    private _text: string;

    init(attributes: any, ...children: Array<JSXNode>) {
        if (attributes) {
            this._text = attributes.text;
        }
    }

    evaluate() {
        // ViewModel.expression(vm)
    }

    destroy() {
    }
}
