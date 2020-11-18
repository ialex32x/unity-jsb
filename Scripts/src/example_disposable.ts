import { DisposableObject } from "Example";

function test() {
    let obj = new DisposableObject();

    // 垃圾回收时将自动执行 obj.Dispose (JS主线程)
    obj = undefined;
}

console.log("before test()");
test();
console.log("after test()");
