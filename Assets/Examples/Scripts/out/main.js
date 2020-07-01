"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
print("first line");
// import { fib } from "./fib_module.js";
const fib_js_1 = require("./fib.js");
// print(new jsb.Goo());
print("line 10");
print(jsb.DelegateTest);
print(jsb.DelegateTest.InnerTest.hello);
print("fib:", fib_js_1.fib(12));
let u = new UnityEngine.Vector3(1, 2, 3);
console.log(u.x);
u.Normalize();
console.log(u.x, u.y, u.z);
setTimeout(() => {
    print("[timeout] test");
}, 1500);
setInterval(() => {
    console.log("interval tick");
}, 1000 * 10);
let go = new UnityEngine.GameObject("test");
console.log(go.name);
go.name = "testing";
console.log(go.name);
async function destroy() {
    await jsb.Yield(new UnityEngine.WaitForSeconds(5));
    UnityEngine.Object.Destroy(go);
}
destroy();
let actions = new jsb.DelegateTest();
actions.onAction = function () {
    console.log("js action");
};
actions.CallAction();
actions.onActionWithArgs = (a, b, c) => {
    console.log(a, b, c);
};
actions.CallActionWithArgs("string", 123, 456);
actions.onFunc = v => v * 2;
console.log(actions.CallFunc(111));
actions.onFunc = undefined;
let v1 = new UnityEngine.Vector3(0, 0, 0);
let start = Date.now();
for (let i = 1; i < 200000; i++) {
    v1.Set(i, i, i);
    v1.Normalize();
}
console.log("js/vector3/normailize", (Date.now() - start) / 1000);
print("require.require:");
print("require 1:", require("./req_test1").test);
print("require 2:", require("./req_test1").test);
print("require 3:", require("./req_test1").test);
// 通过 require 直接读 json
print("json:", require("../config/data.json").name);
// Object.keys(require.cache).forEach(key => console.log("module loaded:", key));
// print("require (node_modules): ", require("blink1").test);
// print("require (node_modules): ", require("blink3").test);
let camera = UnityEngine.GameObject.Find("/Main Camera").GetComponent(UnityEngine.Camera);
let arr = camera.GetComponents(UnityEngine.Camera);
print("array.length:", arr.length);
print("array[0]:", arr[0] == camera);
console.log(camera.name);
const example_websocket_1 = require("./example_websocket");
example_websocket_1.run();
print("end of script");
// 通过反射方式建立未导出类型的交互
let unknown = jsb.DelegateTest.GetNotExportedClass();
print(unknown.value);
print(unknown.Add(12, 21));
print("Equals(unknown, unknown):", System.Object.Equals(unknown, unknown));
print("Equals(unknown, camera):", System.Object.Equals(unknown, camera));
print("ReferenceEquals(unknown, unknown):", System.Object.ReferenceEquals(unknown, unknown));
print("ReferenceEquals(unknown, camera):", System.Object.ReferenceEquals(unknown, camera));
// jsb.DelegateTest.CallHotfixTest();
// jsb.hotfix.replace_single("HotfixTest", "Foo", function (x: number) {
//     print("1 replace by js func, this.value = ", this.value);
//     return x * 3;
// });
// jsb.hotfix.replace_single("HotfixTest2", "Foo", function (x: number) {
//     print("2 replace by js func, this.value = ", this.value);
//     return x * 6;
// });
// jsb.DelegateTest.CallHotfixTest();
var takeBuffer = NoNamespaceClass.MakeBytes();
var testBuffer = new Uint8Array(takeBuffer);
var backBuffer = new Uint8Array(NoNamespaceClass.TestBytes(testBuffer));
backBuffer.forEach(val => print(val));
//# sourceMappingURL=main.js.map