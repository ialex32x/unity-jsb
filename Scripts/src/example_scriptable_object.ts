import { Object, Resources, ScriptableObject, Vector3 } from "UnityEngine";
import { MyScriptableObject } from "./my_scriptable_object";

console.log("============ Load ScriptableObject from Resources ============");
let js_data = <MyScriptableObject>Resources.Load("data/js_data");

if (js_data) {
    console.log("typeof:", typeof js_data);
    console.log("type check:", js_data instanceof MyScriptableObject);
    console.log("type values:", js_data.value1, js_data.value2);
} else {
    console.error("failed to load js_data, please create the asset at first.");
}

console.log("============ ScriptableObject.CreateInstance ============");
let so = ScriptableObject.CreateInstance(MyScriptableObject);
if (so) {
    so.value3 = new Vector3(1, 2, 3);
    so.value2 = "Hello";
    console.log("value1:", so.Process());
    Object.DestroyImmediate(so);
}
