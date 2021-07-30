import { Resources, ScriptableObject, Vector3 } from "UnityEngine";
import { ScriptAsset, ScriptNumber, ScriptString, ScriptProperty } from "./plover/runtime/class_decorators";

@ScriptAsset()
export class MyScriptableObject extends ScriptableObject {
    @ScriptNumber()
    value1 = 1;

    @ScriptString()
    value2 = "hello";

    @ScriptProperty({ type: "Vector3" })
    value3 = Vector3.zero;

    Reset() {
        this.value1 = 0;
        this.value2 = "";
        this.value3 = Vector3.zero;
    }
}

if (require.main == module) {
    let js_data = <MyScriptableObject>Resources.Load("data/js_data");

    if (js_data) {
        console.log("type check:", js_data instanceof MyScriptableObject);
        console.log("type values:", js_data.value1, js_data.value2);
    } else {
        console.error("failed to load js_data, please create the asset at first.");
    }
} 
