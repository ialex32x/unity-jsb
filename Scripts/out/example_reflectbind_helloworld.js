"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Example_1 = require("Example");
const UnityEngine_1 = require("UnityEngine");
console.log("hello, world");
UnityEngine_1.Debug.Log("hello, world");
let go = new UnityEngine_1.GameObject("Happy Bot");
console.log("go.transform = ", go.transform);
console.log("go.transform.localPosition.x = ", go.transform.localPosition.x);
let v = new UnityEngine_1.Vector3(1, 1, 1);
v.x = 2;
console.log("v.x = ", v.x);
console.log("v.magnitude = ", v.magnitude);
go.transform.localPosition = new UnityEngine_1.Vector3(1, 1, 1);
Example_1.DelegateTest.onStaticActionWithArgs("set", (a1, a2, a3) => console.log("delegate in js:", a1, a2, a3));
Example_1.DelegateTest.CallStaticActionWithArgs("hello", 123, 999);
let delegateTest = new Example_1.DelegateTest();
delegateTest.onActionWithArgs("add", (a1, a2, a3) => console.log("delegate in js1:", a1, a2, a3));
delegateTest.onActionWithArgs("add", (a1, a2, a3) => console.log("delegate in js2:", a1, a2, a3));
delegateTest.CallActionWithArgs("hello", 123, 999);
class HelloBehaviour extends UnityEngine_1.MonoBehaviour {
    Awake() {
        console.log("Hello Behaviour Awake!");
    }
    Greet() {
        console.log("Good day!");
    }
}
go.AddComponent(HelloBehaviour);
let helloBehaviour = go.GetComponent(HelloBehaviour);
helloBehaviour.Greet();
console.log(go.AddComponent);
let box = go.AddComponent(UnityEngine_1.BoxCollider);
console.log(box);
//# sourceMappingURL=example_reflectbind_helloworld.js.map