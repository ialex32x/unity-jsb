import { JSBehaviourProperties } from "QuickJS.Unity";
import { MonoBehaviour } from "UnityEngine";
import { SerializationUtil } from "../editor/decorators/inspector";

export class JSBehaviourBase extends MonoBehaviour {
    OnBeforeSerialize(ps: JSBehaviourProperties) {
        SerializationUtil.serialize(this, ps);
    }

    OnAfterDeserialize(ps: JSBehaviourProperties) {
        SerializationUtil.deserialize(this, ps);
    }

}
