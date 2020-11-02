import { Vector3 } from "UnityEngine";

let u = new Vector3(1, 2, 3);

console.log(u.x);
u.Normalize();
console.log(u.x, u.y, u.z);

let v1 = new Vector3(0, 0, 0)
let start = Date.now();
for (let i = 1; i < 200000; i++) {
    v1.Set(i, i, i)
    v1.Normalize()
}
console.log("js/vector3/normailize", (Date.now() - start) / 1000);
