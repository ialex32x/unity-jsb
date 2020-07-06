
import * as std from "std";
import * as os from "os";

import {fib as fib1} from "./examples/fib_module.js";
import {fib as fib2} from "./examples/fib_module.js";

print("fib1:", fib1(6));
print("fib2:", fib2(6));

print("Foo.Foo test", Foo.Foo);

let fc = new Foo();

fc.call();

let setTimeout = os.setTimeout;

for (var i = 0; i < 5; i++) {
    let foo = new Foo();
    foo = undefined;
}

let goo = new Goo();
goo = undefined;

let goo2 = new Goo();

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
    quit();
}

setTimeout(() => {
    print("[timeout] test");
    quit();
}, 1500);

test();

print("end of script");

