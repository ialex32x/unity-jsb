import { Camera, Object, Resources, ScriptableObject, Vector3 } from "UnityEngine";
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

try {
    console.log("============ ScriptableObject.CreateInstance ============ 111");
    let so = ScriptableObject.CreateInstance(MyScriptableObject);
    if (so) {
        // Resources.FindObjectsOfTypeAll can also properly process script types now
        let results = Resources.FindObjectsOfTypeAll(MyScriptableObject);
        do {
            if (results) {
                let len = results.length;
                if (len > 0) {
                    for (let i = 0; i < len; i++) {
                        console.log("!!! find object:", results[i]);
                    }
                    break;
                }
            }
            console.warn("something wrong, no object found.");
        } while (false);

        for (let i of Resources.FindObjectsOfTypeAll(Camera)) {
            console.log("!!! camera", i);
        }

        so.value3 = new Vector3(1, 2, 3);
        so.value2 = "Hello";
        console.log("value1:", so.Process());
        Object.DestroyImmediate(so);
    } else {
        console.error("fail to ScriptableObject.CreateInstance");
    }
} catch (err) {
    console.error("fail to ScriptableObject.CreateInstance", err);
}