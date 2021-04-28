import { Animator, Input, KeyCode, SpriteRenderer, Time, Vector3 } from "UnityEngine";
import { Inspector, ScriptType, SerializedObject } from "../editor/decorators/inspector";
import { JSBehaviourBase } from "./js_behaviour_base";

// 暂时不支持相对路径
@Inspector("game/editor/king_human_controller_inspector", "KingHumanControllerInspector")
@ScriptType
export class KingHumanController extends JSBehaviourBase {
    @SerializedObject()
    animator: Animator;

    private moving = false;
    private spriteRenderer: SpriteRenderer;

    Awake() {
        this.transform.localScale = new Vector3(5, 5, 5);
        this.spriteRenderer = this.GetComponent(SpriteRenderer);
    }

    Update() {
        if (Input.anyKey) {
            let hori = 0;
            let vert = 0;

            if (Input.GetKey(KeyCode.A)) {
                hori = -1;
            } else if (Input.GetKey(KeyCode.D)) {
                hori = 1;
            }

            if (Input.GetKey(KeyCode.W)) {
                vert = 1;
            } else if (Input.GetKey(KeyCode.S)) {
                vert = -1;
            }

            if (hori != 0 || vert != 0) {
                let scale = Time.deltaTime * 5;
                this.transform.Translate(hori * scale, vert * scale, 0);
                if (hori != 0) {
                    this.spriteRenderer.flipX = hori < 0;
                }
                if (!this.moving) {
                    this.moving = true;
                    this.animator.Play("Run", 0);
                }
            } else {
                if (this.moving) {
                    this.moving = false;
                    this.animator.Play("Idle", 0);
                }
            }
        } else {
            if (this.moving) {
                this.moving = false;
                this.animator.Play("Idle", 0);
            }
        }
    }
}

