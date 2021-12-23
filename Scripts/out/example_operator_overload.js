"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const UnityEngine_1 = require("UnityEngine");
const jsb = require("jsb");
if (module == require.main) {
    let support = jsb.isOperatorOverloadingSupported;
    console.log("IsOperatorOverloadingSupported:", support);
    // operators
    {
        let vec1 = new UnityEngine_1.Vector3(1, 2, 3);
        let vec2 = new UnityEngine_1.Vector3(9, 8, 7);
        // @ts-ignore
        let vec3 = support ? vec1 + vec2 : UnityEngine_1.Vector3.op_Addition(vec1, vec2);
        // @ts-ignore
        let vec4 = support ? vec1 + vec2 : UnityEngine_1.Vector3.op_Addition(vec1, vec2);
        console.log("v1 = ", vec1);
        console.log("v2 = ", vec2);
        console.log("v1 + v2 =", vec3);
        // @ts-ignore
        console.log("v3 * 2 =", support ? vec3 * 2 : UnityEngine_1.Vector3.op_Multiply(vec3, 2));
        // @ts-ignore
        console.log("2 * v3=", support ? 2 * vec3 : UnityEngine_1.Vector3.op_Multiply(2, vec3));
        // @ts-ignore
        console.log("v3 / 2 =", support ? vec3 / 2 : UnityEngine_1.Vector3.op_Division(vec3, 2));
        console.log("v3 == v4:", support ? vec3 == vec4 : UnityEngine_1.Vector3.op_Equality(vec3, vec4));
    }
    {
        let c1 = new UnityEngine_1.Color(0, 0, 0, 1);
        let c2 = new UnityEngine_1.Color(0.5, 0.1, 0.2, 0);
        // @ts-ignore
        let r = support ? c2 / 3 : UnityEngine_1.Color.op_Division(c2, 3);
        print(c1, c2, r);
    }
}
//# sourceMappingURL=example_operator_overload.js.map