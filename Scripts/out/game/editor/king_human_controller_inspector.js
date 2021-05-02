"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.KingHumanControllerInspector = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
// @CustomEditor(KingHumanController)
class KingHumanControllerInspector extends UnityEditor_1.Editor {
    OnInspectorGUI() {
        let p = this.target;
        UnityEditor_1.EditorGUI.BeginChangeCheck();
        p.animator = UnityEditor_1.EditorGUILayout.ObjectField("Animator", p.animator, UnityEngine_1.Animator, true);
        p.moveSpeed = UnityEditor_1.EditorGUILayout.FloatField("Move Speed", p.moveSpeed);
        if (UnityEditor_1.EditorGUI.EndChangeCheck()) {
            UnityEditor_1.EditorUtility.SetDirty(p);
        }
        if (UnityEngine_1.GUILayout.Button("Speak")) {
            console.log("Hello, world 666 !");
        }
        if (UnityEngine_1.GUILayout.Button("Attack")) {
            p.animator.Play("Attack", 0, 1);
        }
        if (UnityEngine_1.GUILayout.Button("Idle")) {
            p.animator.Play("Idle", 0, 1);
        }
    }
}
exports.KingHumanControllerInspector = KingHumanControllerInspector;
//# sourceMappingURL=king_human_controller_inspector.js.map