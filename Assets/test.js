
// import { fib } from "./fib_module.js";
import {fib} from "Assets/fib.js";

print(jsb);
print(jsb.Foo);
print(new jsb.Foo());
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

print("end of script");

