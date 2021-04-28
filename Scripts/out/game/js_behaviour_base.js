"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.JSBehaviourBase = void 0;
const UnityEngine_1 = require("UnityEngine");
const inspector_1 = require("../editor/decorators/inspector");
class JSBehaviourBase extends UnityEngine_1.MonoBehaviour {
    OnBeforeSerialize(ps) {
        inspector_1.SerializationUtil.serialize(this, ps);
    }
    OnAfterDeserialize(ps) {
        inspector_1.SerializationUtil.deserialize(this, ps);
    }
}
exports.JSBehaviourBase = JSBehaviourBase;
//# sourceMappingURL=js_behaviour_base.js.map