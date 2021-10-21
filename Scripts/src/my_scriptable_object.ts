import { ScriptableObject, Vector3 } from "UnityEngine";
import { ScriptAsset, ScriptNumber, ScriptString, ScriptProperty, ScriptSerializable } from "./plover/runtime/class_decorators";

@ScriptAsset()
export class MyScriptableObject extends ScriptableObject {
    @ScriptNumber()
    value1 = 1;

    @ScriptString()
    value2 = "hello";

    @ScriptProperty({ type: "Vector3" })
    value3 = Vector3.zero;

    @ScriptNumber()
    value5: Array<number> = [];

    Process() {
        return `${this.value2} ${this.value3}`;
    }

    Reset() {
        this.value1 = 0;
        this.value2 = "";
        this.value3 = Vector3.zero;
        this.value5 = [1, 2, 3];
    }
}
