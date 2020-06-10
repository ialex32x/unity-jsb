// import { fib } from "./fib_module.js";
import { fib } from "Assets/fib.js";

print(jsb);
print(jsb.Foo);
let foo = new jsb.Foo();
print(foo);
print(123, 456);

// print(new jsb.Goo());

print("fib:", fib(12));

function delay(secs) {
    return new Promise((resolve, reject) => {
        setTimeout(() => {
            print("[async] resolve");
            resolve();
        }, secs * 1000);
    });
}

async function test() {
    print("[async] begin");
    await delay(3);
    print("[async] end");
}

test();

setTimeout(() => {
    print("[timeout] test");
}, 1500);

let v3 = new jsb.SValue();
v3.Test();
v3.Test();

print("foo instanceof jsb.Foo:", foo instanceof jsb.Foo);
print("foo instanceof jsb.SValue:", foo instanceof jsb.SValue);
print("v3 instanceof jsb.SValue:", v3 instanceof jsb.SValue);
print("v3 instanceof jsb.Foo:", v3 instanceof jsb.Foo);

let u = new UnityEngine.Vector3(1, 2, 3);

console.log(u.x);
u.Normalize();
console.log(u.x, u.y, u.z);

let go = new UnityEngine.GameObject("test");
console.log(go.name);
go.name = "testing";
console.log(go.name);

async function destroy() {
    await delay(5);
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
}
actions.CallActionWithArgs("string", 123, 456);

actions.onFunc = v => v * 2;
console.log(actions.CallFunc(111));
actions.onFunc = undefined;

let v1 = new UnityEngine.Vector3(0, 0, 0)
let start = Date.now();
for (let i = 1; i < 200000; i++) {
    v1.Set(i, i, i)
    v1.Normalize()
}
console.log("js/vector3/normailize", (Date.now() - start) / 1000);

print("end of script");

print("require.require:");
print("require 1:", require("./req_test1").test);
print("require 2:", require("./req_test1").test);
print("require 3:", require("./req_test1").test);

// 通过 require 直接读 json
print("json:", require("./data.json").name);

Object.keys(require.cache).forEach(key => console.log("module loaded:", key));

// print("require (node_modules): ", require("blink1").test);
// print("require (node_modules): ", require("blink3").test);
