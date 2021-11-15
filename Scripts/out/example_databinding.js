"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const data_binding_1 = require("plover/events/data_binding");
const dispatcher_1 = require("plover/events/dispatcher");
// data
let data = {
    name: "test name",
    age: 30,
    address: "somewhere",
};
let test = data_binding_1.DataBinding.bind(data);
class TextInput {
    constructor() {
        this.onchange = new dispatcher_1.Dispatcher();
    }
    get text() { return this._text; }
    set text(value) {
        this._text = value;
        console.log("[TextInput] set text:", value);
        this.onchange.dispatch(this);
    }
}
class TextInputWatcher extends data_binding_1.Subscriber {
    constructor(model, key, input) {
        super(model, key);
        this._input = input;
        this._input.onchange.on(this, this.onchange);
    }
    update(value) {
        this._input.text = value;
    }
    onchange(input) {
        this.value = input.text;
    }
}
let input = new TextInput();
let sub = data_binding_1.DataBinding.subscribe(TextInputWatcher, test, "name", input);
test.name = "temp name";
console.log("[数据变更]", test.name, input.text);
input.text = "输入文字";
console.log("[界面变更]", test.name, input.text);
sub.unsubscribe();
console.log(test.name);
test.name = "new name";
console.log(test.name);
console.log(test.age);
Object.keys(test).forEach(k => console.log("model access:", k));
//# sourceMappingURL=example_databinding.js.map