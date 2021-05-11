import { Resources, ScriptableObject } from "UnityEngine";
import { ScriptAsset, ScriptNumber, ScriptString } from "./plover/editor/editor_decorators";

@ScriptAsset()
export class MyScriptableObject extends ScriptableObject {
    @ScriptNumber()
    value1 = 1;

    @ScriptString()
    value2 = "hello";
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
