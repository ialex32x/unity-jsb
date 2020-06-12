// import { fib } from "./fib_module.js";
import { fib } from "./fib.js";

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

async function testUnityYieldInstructions() {
    console.warn("wait for unity YieldInstruction, begin");
    await jsb.Yield(new UnityEngine.WaitForSeconds(3));

    console.warn("wait for unity YieldInstruction, end;", UnityEngine.Time.frameCount);
    await jsb.Yield(null);
    console.warn("wait for unity YieldInstruction, next frame;", UnityEngine.Time.frameCount);
}
testUnityYieldInstructions();

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

print("require.require:");
print("require 1:", require("./req_test1").test);
print("require 2:", require("./req_test1").test);
print("require 3:", require("./req_test1").test);

// 通过 require 直接读 json
print("json:", require("../config/data.json").name);

// Object.keys(require.cache).forEach(key => console.log("module loaded:", key));

// print("require (node_modules): ", require("blink1").test);
// print("require (node_modules): ", require("blink3").test);

setInterval(() => {
    console.log("interval tick");
}, 1000 * 10);

print("end of script");

// 未完成

class MyClass extends UnityEngine.MonoBehaviour {
    vv = 0;
    protected _tick = 0;

    Awake() {
        console.log("MyClass.Awake", this._tick++);
    }

    async OnEnable() {
        console.log("MyClass.OnEnable", this._tick++);
        await jsb.Yield(new UnityEngine.WaitForSeconds(1));
        console.log("MyClass.OnEnable (delayed)", this._tick++);
    }

    OnDisable() {
        console.log("MyClass.OnDisable", this._tick++);
    }

    OnDestroy() {
        console.log("MyClass.OnDestroy", this._tick++);
    }

    async test() {
        console.log("MyClass.test (will be destroied after 5 secs.", this.transform);
        await jsb.Yield(new UnityEngine.WaitForSeconds(5));
        UnityEngine.Object.Destroy(this.gameObject);
    }
}

class MySubClass extends MyClass {
    Awake() {
        super.Awake();
        console.log("MySubClass.Awake", this._tick++);
    }

    play() {
        console.log("MySubClass.play");
    }
}

let gameObject = new UnityEngine.GameObject();
let comp1 = gameObject.AddComponent(MySubClass);
let comp2 = gameObject.AddComponent(MyClass);

comp1.vv = 1;
comp2.vv = 2;

comp1.play();

{
    let results = gameObject.GetComponents(MySubClass);
    results.forEach(it => console.log("GetComponents(MySubClass):", it.vv));
}

{
    let results = gameObject.GetComponents(MyClass);
    results.forEach(it => console.log("GetComponents(MyClass):", it.vv));
}

let camera = UnityEngine.GameObject.Find("/Main Camera").GetComponent(UnityEngine.Camera);

console.log(camera.name);

// console.log(Operators.create);

// let vec1 = new UnityEngine.Vector3(1, 2, 3);
// let vec2 = new UnityEngine.Vector3(9, 8, 7);
// let vec3 = vec1 + vec2;
// console.log(vec3.ToString());

