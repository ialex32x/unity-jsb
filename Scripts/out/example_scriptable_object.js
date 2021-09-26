"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const UnityEngine_1 = require("UnityEngine");
const my_scriptable_object_1 = require("./my_scriptable_object");
console.log("============ Load ScriptableObject from Resources ============");
let js_data = UnityEngine_1.Resources.Load("data/js_data");
if (js_data) {
    console.log("typeof:", typeof js_data);
    console.log("type check:", js_data instanceof my_scriptable_object_1.MyScriptableObject);
    console.log("type values:", js_data.value1, js_data.value2);
}
else {
    console.error("failed to load js_data, please create the asset at first.");
}
console.log("============ ScriptableObject.CreateInstance ============");
let so = UnityEngine_1.ScriptableObject.CreateInstance(my_scriptable_object_1.MyScriptableObject);
if (so) {
    so.value3 = new UnityEngine_1.Vector3(1, 2, 3);
    so.value2 = "Hello";
    console.log("value1:", so.Process());
    UnityEngine_1.Object.DestroyImmediate(so);
}
//# sourceMappingURL=example_scriptable_object.js.map