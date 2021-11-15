import { DataBinding, Subscriber } from "plover/events/data_binding"
import { Dispatcher } from "plover/events/dispatcher"

interface MyData {
    name: string
    age: number
    address: string
}

// data
let data: MyData = {
    name: "test name",
    age: 30,
    address: "somewhere",
}

let test = DataBinding.bind(data);

class TextInput {
    private _text: string;

    onchange = new Dispatcher();

    get text() { return this._text; }

    set text(value: string) {
        this._text = value;
        console.log("[TextInput] set text:", value);
        this.onchange.dispatch(this);
    }
}

class TextInputWatcher extends Subscriber {
    private _input: TextInput;

    constructor(model: DataBinding, key: string, input: TextInput) {
        super(model, key);
        this._input = input;
        this._input.onchange.on(this, this.onchange);
    }

    protected update(value: string) {
        this._input.text = value;
    }

    onchange(input: TextInput) {
        this.value = input.text;
    }
}

let input = new TextInput();
let sub = DataBinding.subscribe(TextInputWatcher, test, "name", input);

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
