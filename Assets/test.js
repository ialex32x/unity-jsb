// import { fib } from "./fib_module.js";
import {fib} from "Assets/fib.js";

print(jsb);
print(jsb.Foo);
let foo = new jsb.Foo();
print(foo);
print(123, 456);

// print(new jsb.Goo());

print("fib:", fib(12));

function delay() {
    return new Promise((resolve, reject) => {
        setTimeout(() => {
            print("[async] resolve");
            resolve();
        }, 1000);
    });
}

async function test() {
    print("[async] begin");
    await delay();
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

print("end of script");

