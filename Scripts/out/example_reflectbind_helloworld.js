"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const UnityEngine_1 = require("UnityEngine");
console.log("hello, world");
UnityEngine_1.Debug.Log("hello, world");
let go = new UnityEngine_1.GameObject("Happy Bot");
console.log("go.transform = ", go.transform);
console.log("go.transform.localPosition.x = ", go.transform.localPosition.x);
console.log("new Vector3(1, 1, 1).x = ", new UnityEngine_1.Vector3(1, 1, 1).x);
go.transform.localPosition = new UnityEngine_1.Vector3(1, 1, 1);
class HelloBehaviour extends UnityEngine_1.MonoBehaviour {
}
go.AddComponent(HelloBehaviour);
//# sourceMappingURL=example_reflectbind_helloworld.js.map