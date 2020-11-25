"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Example_1 = require("Example");
function test() {
    let obj = new Example_1.DisposableObject();
    // 垃圾回收时将自动执行 obj.Dispose (JS主线程)
    obj = undefined;
}
console.log("before test()");
test();
console.log("after test()");
//# sourceMappingURL=example_disposable.js.map