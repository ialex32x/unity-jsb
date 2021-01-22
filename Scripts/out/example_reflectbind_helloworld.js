"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Example_1 = require("Example");
const UnityEngine_1 = require("UnityEngine");
console.log("hello, world");
UnityEngine_1.Debug.Log("hello, world");
let go = new UnityEngine_1.GameObject("Happy Bot");
console.log("go.transform = ", go.transform);
console.log("go.transform.localPosition.x = ", go.transform.localPosition.x);
console.log("new Vector3(1, 1, 1).x = ", new UnityEngine_1.Vector3(1, 1, 1).x);
go.transform.localPosition = new UnityEngine_1.Vector3(1, 1, 1);
Example_1.DelegateTest.onStaticActionWithArgs("set", (a1, a2, a3) => console.log("delegate in js:", a1, a2, a3));
Example_1.DelegateTest.CallStaticActionWithArgs("hello", 123, 999);
let delegateTest = new Example_1.DelegateTest();
delegateTest.onActionWithArgs("add", (a1, a2, a3) => console.log("delegate in js1:", a1, a2, a3));
delegateTest.onActionWithArgs("add", (a1, a2, a3) => console.log("delegate in js2:", a1, a2, a3));
delegateTest.CallActionWithArgs("hello", 123, 999);
// class HelloBehaviour extends MonoBehaviour {
// }
// go.AddComponent(HelloBehaviour);
//# sourceMappingURL=example_reflectbind_helloworld.js.map