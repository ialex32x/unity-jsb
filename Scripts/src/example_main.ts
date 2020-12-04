print("isMain?", module == require.main);

import { DelegateTest, ValueTest } from "Example";
import * as jsb from "jsb";
import { Camera, Vector3 } from "UnityEngine";
// import { fib } from "./fib_module.js";
import { fib } from "./fib";

console.assert(true, "will not print");
console.assert(false, "assert!!!");

print(DelegateTest);
print(DelegateTest.InnerTest.hello);

try {
    // 强行传入一个错误的参数 (期望参数为无参)
    // @ts-ignore
    DelegateTest.GetArray("error");
} catch (err) {
    console.warn(err + '\n' + err.stack);
}

try {
    // 注意: 因为效率上的考虑, 简单类型是不会抛异常的, 比如 Camera.main.orthographicSize = "abc" 结果 = 0
    //       除非传入 any, 否则大部分情况下ts编译将提示类型错误
    //       可能后续会通过 DEBUG 宏等进行更严格的检查

    // 强行传入一个错误类型参数
    // @ts-ignore
    Camera.main.transparencySortAxis = "wrong value";
} catch (err) {
    console.warn(err);
}

print("fib:", fib(12));

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
} catch (e) {
    console.warn(e);
}

global["testGlobalVar"] = "test";

Object.keys(require.cache).forEach(k => console.log("require.cache entry:", k));
