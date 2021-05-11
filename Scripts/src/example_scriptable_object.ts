import { Resources, ScriptableObject } from "UnityEngine";
import { ScriptAsset, ScriptNumber, ScriptString } from "./plover/editor/editor_decorators";

@ScriptAsset()
export class MyScriptableObject extends ScriptableObject {
    @ScriptNumber()
    value1 = 1;

    @ScriptString()
    value2 = "hello";
}

//TODO: 加載还没实现
// let js_data = Resources.Load("data/js_data");
