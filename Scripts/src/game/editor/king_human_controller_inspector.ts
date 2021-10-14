import { Editor, EditorGUI, EditorGUILayout, EditorUtility, MessageType } from "UnityEditor";
import { Animator, GUILayout, Object } from "UnityEngine";
import { EditorUtil, ScriptEditor } from "../../plover/editor/editor_decorators";
import { KingHumanController, MyNestedPlainObject } from "../king_human_controller";

@ScriptEditor(KingHumanController)
export class KingHumanControllerInspector extends Editor {
    OnInspectorGUI() {
        let p = <KingHumanController>this.target;

        // EditorUtil.draw(p);
        EditorGUI.BeginChangeCheck();
        p.animator = <Animator>EditorGUILayout.ObjectField("Animator", p.animator, Animator, true);
        p.moveSpeed = EditorGUILayout.FloatField("Move Speed", p.moveSpeed);
        if (!p.nestedValue) {
            p.nestedValue = new MyNestedPlainObject();
        }
        p.nestedValue.nestedString = EditorGUILayout.TextField("nestedString", p.nestedValue.nestedString);
        p.nestedValue.nestedVector3 = EditorGUILayout.Vector3Field("nestedVector3", p.nestedValue.nestedVector3);
        
        if (EditorGUI.EndChangeCheck()) {
            EditorUtility.SetDirty(p);
        }

        if (GUILayout.Button("Speak")) {
            console.log("Hello, world 666 !");
        }

        if (GUILayout.Button("Attack")) {
            p.animator.Play("Attack", 0, 1);
        }

        if (GUILayout.Button("Idle")) {
            p.animator.Play("Idle", 0, 1);
        }
    }
}
