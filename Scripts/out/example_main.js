"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
print("isMain?", module == require.main);
const Example_1 = require("Example");
const jsb = require("jsb");
const UnityEngine_1 = require("UnityEngine");
// import { fib } from "./fib_module.js";
const fib_1 = require("./fib");
console.assert(true, "will not print");
console.assert(false, "assert!!!");
print(jsb.engine, jsb.version);
print(Example_1.DelegateTest);
print(Example_1.DelegateTest.InnerTest.hello);
try {
    // 强行传入一个错误的参数 (期望参数为无参)
    // @ts-ignore
    Example_1.DelegateTest.GetArray("error");
}
catch (err) {
    console.warn(err + '\n' + err.stack);
}
try {
    // 注意: 因为效率上的考虑, 简单类型是不会抛异常的, 比如 Camera.main.orthographicSize = "abc" 结果 = 0
    //       除非传入 any, 否则大部分情况下ts编译将提示类型错误
    //       可能后续会通过 DEBUG 宏等进行更严格的检查
    // 强行传入一个错误类型参数
    // @ts-ignore
    UnityEngine_1.Camera.main.transparencySortAxis = "wrong value";
}
catch (err) {
    console.warn(err);
}
// 一维数组的操作
console.log("ValueTest.values1[2] = ", Example_1.ValueTest.values1.GetValue(2));
// 多维数组的操作
console.log("ValueTest.values2[0, 1] = ", Example_1.ValueTest.values2.GetValue(0, 1));
print("fib:", fib_1.fib(12));
setTimeout(() => {
    print("[timeout] test");
}, 1500);
setInterval(() => {
    console.log("interval tick");
}, 1000 * 10);
print("require.require:");
print("require 1:", require("./req_test1").test);
print("require 2:", require("./req_test1").test);
print("require 3:", require("./req_test1").test);
// 通过 require 直接读 json
print("json:", require("../config/data.json").name);
// Object.keys(require.cache).forEach(key => console.log("module loaded:", key));
// print("require (node_modules): ", require("blink1").test);
// print("require (node_modules): ", require("blink3").test);
// Optional Chaining/Nullish coalescing Operator 需要较新的 tsc 编译, 或者直接在 js 代码中使用
// // Optional Chaining
// let a: any = 1;
// print("Optional Chaining", a?.b?.c === undefined);
// // Nullish coalescing Operator
// print("Nullish coalescing Operator:", a?.b ?? "ok");
// const protobuf = require("protobufjs");
// print("protobufjs:", protobuf);
jsb.DoFile("dofile_test");
try {
    jsb.DoFile("not_exists.file");
}
catch (e) {
    console.warn(e);
}
globalThis["testGlobalVar"] = "test";
Object.keys(require.cache).forEach(k => console.log("require.cache entry:", k));
//# sourceMappingURL=example_main.js.map