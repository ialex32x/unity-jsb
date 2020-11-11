print("isMain?", module == require.main);

import { DelegateTest } from "Example";
import * as jsb from "jsb";
// import { fib } from "./fib_module.js";
import { fib } from "./fib";

console.assert(true, "will not print");
console.assert(false, "assert!!!");

print(DelegateTest);
print(DelegateTest.InnerTest.hello);

try {
    // @ts-ignore
    DelegateTest.GetArray("error");
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

global["testGlobalVar"] = "test";

Object.keys(require.cache).forEach(k => console.log("require.cache entry:", k));
