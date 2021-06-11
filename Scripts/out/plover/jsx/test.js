// not implemented yet, it's imagination for fun
console.log("test jsx");
let userWidget = jsx("widget", null,
    jsx("label", { name: "test", bind: "expression {test.value}" }),
    jsx("list", { name: "list_test", bind: "mydata.mylist", "entry-class": "SomeType" }),
    jsx("button", { name: "button_test", bind: "mydata.myaction", onclick: "this.onclick" }));
//# sourceMappingURL=test.js.map