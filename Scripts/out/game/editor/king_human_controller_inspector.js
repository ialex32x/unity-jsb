"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.KingHumanControllerInspector = void 0;
const UnityEditor_1 = require("UnityEditor");
const UnityEngine_1 = require("UnityEngine");
const editor_decorators_1 = require("plover/editor/editor_decorators");
const king_human_controller_1 = require("../king_human_controller");
let KingHumanControllerInspector = class KingHumanControllerInspector extends UnityEditor_1.Editor {
    OnInspectorGUI() {
        let p = this.target;
        // EditorUtil.draw(p);
        UnityEditor_1.EditorGUI.BeginChangeCheck();
        p.animator = UnityEditor_1.EditorGUILayout.ObjectField("Animator", p.animator, UnityEngine_1.Animator, true);
        p.moveSpeed = UnityEditor_1.EditorGUILayout.FloatField("Move Speed", p.moveSpeed);
        if (!p.nestedValue) {
            p.nestedValue = new king_human_controller_1.MyNestedPlainObject();
        }
        p.nestedValue.nestedString = UnityEditor_1.EditorGUILayout.TextField("nestedString", p.nestedValue.nestedString);
        p.nestedValue.nestedVector3 = UnityEditor_1.EditorGUILayout.Vector3Field("nestedVector3", p.nestedValue.nestedVector3);
        if (UnityEngine_1.GUILayout.Button("Add Position")) {
            if (p.nestedValue.positions == null) {
                p.nestedValue.positions = [];
            }
            p.nestedValue.positions.push(UnityEngine_1.Vector2.zero);
            UnityEditor_1.EditorUtility.SetDirty(p);
        }
        let positionCount = p.nestedValue.positions != null ? p.nestedValue.positions.length : 0;
        for (let i = 0; i < positionCount; i++) {
            p.nestedValue.positions[i] = UnityEditor_1.EditorGUILayout.Vector2Field("Position", p.nestedValue.positions[i] || UnityEngine_1.Vector2.zero);
        }
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
};
KingHumanControllerInspector = __decorate([
    editor_decorators_1.ScriptEditor(king_human_controller_1.KingHumanController)
], KingHumanControllerInspector);
exports.KingHumanControllerInspector = KingHumanControllerInspector;
//# sourceMappingURL=king_human_controller_inspector.js.map