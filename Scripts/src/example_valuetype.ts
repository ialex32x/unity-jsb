import { Vector2, Vector3 } from "UnityEngine";

let u = new Vector3(1, 2, 3);

console.assert(u.x == 1, "u.x should equals to 1");
u.Normalize();
console.assert(Math.abs(u.magnitude - 1) < 0.00001, "u.magnitude should equals to 1 after being normalized");

let v1 = new Vector3(0, 0, 0)
let start = Date.now();
for (let i = 1; i < 200000; i++) {
    v1.Set(i, i, i)
    v1.Normalize()
}
console.log("js/vector3/normailize", (Date.now() - start) / 1000);

let null_value = null;
let undefined_value = undefined;
console.assert(!(null_value instanceof Vector3), "null instanceof Vector3");
console.assert(!(undefined_value instanceof Vector3), "undefined instanceof Vector3");
console.assert(v1 instanceof Vector3, "(correct value) instanceof Vector3");
console.assert(!(Vector2.zero instanceof Vector3), "(wrong value) instanceof Vector3");

// everytime you access Vector3.zero will return a new copy
console.assert(Vector3.zero.magnitude == 0, "Vector3.zero");
Vector3.zero.Set(1, 2, 3);
console.assert(Vector3.zero.magnitude == 0, "Vector3.zero");
