import { Editor, EditorGUI, EditorGUILayout, EditorUtility, MessageType } from "UnityEditor";
import { Animator, GUILayout, Object } from "UnityEngine";
import { KingHumanController } from "../king_human_controller";

export class KingHumanControllerInspector extends Editor {
    OnInspectorGUI() {
        let p = <KingHumanController>this.target;

        let anim = <Animator>EditorGUILayout.ObjectField("Animator", p.animator, Animator, true);
        if (anim != p.animator) {
            p.animator = anim;
            EditorUtility.SetDirty(p);
        }

        if (GUILayout.Button("Attack")) {
            p.animator.Play("Attack", 0, 1);
        }

        if (GUILayout.Button("Idle")) {
            p.animator.Play("Idle", 0, 1);
        }
    }
}
