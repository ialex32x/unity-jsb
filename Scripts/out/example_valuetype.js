"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const UnityEngine_1 = require("UnityEngine");
let u = new UnityEngine_1.Vector3(1, 2, 3);
console.log(u.x);
u.Normalize();
console.log(u.x, u.y, u.z);
let v1 = new UnityEngine_1.Vector3(0, 0, 0);
let start = Date.now();
for (let i = 1; i < 200000; i++) {
    v1.Set(i, i, i);
    v1.Normalize();
}
console.log("js/vector3/normailize", (Date.now() - start) / 1000);
//# sourceMappingURL=example_valuetype.js.map