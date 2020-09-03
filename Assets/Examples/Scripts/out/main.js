"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
print("first line");
// import { fib } from "./fib_module.js";
const fib_1 = require("./fib");
// print(new jsb.Goo());
print("line 10");
print(jsb.DelegateTest);
print(jsb.DelegateTest.InnerTest.hello);
print("fib:", fib_1.fib(12));
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
print("测试: 委托");
actions.AddAction();
// {
//     // (不建议) 可以将 C# 委托强行转换成 Function
//     let dangerous = jsb.ToFunction(<Function>actions.onAction);
//     dangerous.call(null);
// }
actions.CallAction();
print("print 委托对象", actions.onAction);
actions.onAction = function () {
    console.log("js action1");
};
actions.CallAction();
print("print 委托对象", actions.onAction);
actions.onAction = null;
actions.CallAction();
print("print 委托对象", actions.onAction);
print("测试: 带参数的委托");
actions.onActionWithArgs = (a, b, c) => {
    console.log(a, b, c);
};
actions.CallActionWithArgs("string", 123, 456);
actions.onFunc = v => v * 2;
console.log(actions.CallFunc(111));
actions.onFunc = undefined;
print("测试: 事件");
actions.onEvent("add", v => print("测试事件1:", v));
function instanceEventHandler(v) { print("测试事件2:", v); }
actions.onEvent("add", instanceEventHandler);
actions.DipatchEvent(123);
actions.onEvent("remove", instanceEventHandler);
actions.DipatchEvent(123);
print("测试: 静态事件");
jsb.DelegateTest.onStaticEvent("add", v => print("测试静态事件1:", v));
function staticEventHandler(v) { print("测试静态事件2:", v); }
jsb.DelegateTest.onStaticEvent("add", staticEventHandler);
jsb.DelegateTest.DipatchStaticEvent(123);
jsb.DelegateTest.onStaticEvent("remove", staticEventHandler);
jsb.DelegateTest.DipatchStaticEvent(123);
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
require("./example_monobehaviour").run();
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
print(unknown.GetType().value2);
print(unknown.Add(12, 21));
print("Equals(unknown, unknown):", System.Object.Equals(unknown, unknown));
print("Equals(unknown, camera):", System.Object.Equals(unknown, camera));
print("ReferenceEquals(unknown, unknown):", System.Object.ReferenceEquals(unknown, unknown));
print("ReferenceEquals(unknown, camera):", System.Object.ReferenceEquals(unknown, camera));
let HotfixTest = jsb.Import("HotfixTest");
try {
    // 反射导入的类型默认收到访问保护 (hotfix 后保护会被迫移除)
    print(HotfixTest.static_value);
}
catch (err) {
    console.warn("默认拒绝访问私有成员", err);
}
try {
    jsb.hotfix.replace_single("HotfixTest", ".ctor", function () {
        print("[HOTFIX][JS] 构造函数");
    });
    jsb.hotfix.replace_single("HotfixTest", "Foo", function (x) {
        print("[HOTFIX][JS] HotfixTest.Foo [private] this.value = ", this.value);
        return typeof x === "number" ? x + 3 : x + "~~~";
    });
    jsb.hotfix.before_single("HotfixTest", "AnotherStaticCall", function () {
        print("[HOTFIX][JS] HotfixTest.AnotherStaticCall 在 C# 执行前插入 JS 代码");
    });
    jsb.hotfix.replace_single("HotfixTest", "SimpleStaticCall", function () {
        this.AnotherStaticCall();
        print("[HOTFIX][JS] HotfixTest.SimpleStaticCall [private] this.static_value = ", this.static_value);
    });
}
catch (err) {
    console.warn("替换失败, 是否执行过dll注入?");
}
let hotfix = new HotfixTest();
print("[HOTFIX][JS] hotfix.Foo(1) 返回值:", hotfix.Foo(1));
print("[HOTFIX][JS] hotfix.Foo(1) 返回值:", hotfix.Foo("good day"));
HotfixTest.SimpleStaticCall();
var takeBuffer = NoNamespaceClass.MakeBytes();
var testBuffer = new Uint8Array(jsb.ToArrayBuffer(takeBuffer));
var restoreBytes = jsb.ToBytes(testBuffer);
var backBuffer = new Uint8Array(jsb.ToArrayBuffer(NoNamespaceClass.TestBytes(restoreBytes)));
print("byte[] 处理");
backBuffer.forEach(val => print(val));
{
    const { x, z } = NoNamespaceClass.TestOut(233);
    print("out:", x, z);
}
{
    const { g, x, z } = NoNamespaceClass.TestRefOut(2, 666);
    print("ref/out:", g, x, z);
}
async function test_custom_promise() {
    let go = new UnityEngine.GameObject("custome.promise.test");
    var sb = go.AddComponent(SampleBehaviour);
    print("after wait a promise (created in C#):", await sb.Wait());
}
test_custom_promise();
// Optional Chaining/Nullish coalescing Operator 需要较新的 tsc 编译, 或者直接在 js 代码中使用
// // Optional Chaining
// let a: any = 1;
// print("Optional Chaining", a?.b?.c === undefined);
// // Nullish coalescing Operator
// print("Nullish coalescing Operator:", a?.b ?? "ok");
// const protobuf = require("protobufjs");
// print("protobufjs:", protobuf);
jsb.DoFile("dofile_test");
global["testGlobalVar"] = "test";
//# sourceMappingURL=main.js.map