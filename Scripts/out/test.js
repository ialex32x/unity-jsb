let s = Symbol.for("test");
class Foo {
    constructor() {
        this.v1 = 1;
        this.v2 = 2;
    }
    foo() {
    }
}
let foo = new Foo();
foo[s] = 1;
for (let k in foo) {
    console.log(k, typeof foo[k]);
}
console.log("symbol", foo[s]);
//# sourceMappingURL=test.js.map