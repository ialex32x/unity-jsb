"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const UnityEngine_1 = require("UnityEngine");
let u = new UnityEngine_1.Vector3(1, 2, 3);
console.assert(u.x == 1, "u.x should equals to 1");
u.Normalize();
console.assert(Math.abs(u.magnitude - 1) < 0.00001, "u.magnitude should equals to 1 after being normalized");
let v1 = new UnityEngine_1.Vector3(0, 0, 0);
let start = Date.now();
for (let i = 1; i < 200000; i++) {
    v1.Set(i, i, i);
    v1.Normalize();
}
console.log("js/vector3/normailize", (Date.now() - start) / 1000);
let null_value = null;
let undefined_value = undefined;
console.assert(!(null_value instanceof UnityEngine_1.Vector3), "null instanceof Vector3");
console.assert(!(undefined_value instanceof UnityEngine_1.Vector3), "undefined instanceof Vector3");
console.assert(v1 instanceof UnityEngine_1.Vector3, "(correct value) instanceof Vector3");
console.assert(!(UnityEngine_1.Vector2.zero instanceof UnityEngine_1.Vector3), "(wrong value) instanceof Vector3");
// everytime you access Vector3.zero will return a new copy
console.assert(UnityEngine_1.Vector3.zero.magnitude == 0, "Vector3.zero");
UnityEngine_1.Vector3.zero.Set(1, 2, 3);
console.assert(UnityEngine_1.Vector3.zero.magnitude == 0, "Vector3.zero");
//# sourceMappingURL=example_valuetype.js.map