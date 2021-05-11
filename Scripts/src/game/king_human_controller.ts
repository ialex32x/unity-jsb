import { Animator, Input, KeyCode, SpriteRenderer, Time, Vector3 } from "UnityEngine";
import { ScriptType, ScriptNumber, ScriptObject } from "../plover/editor/editor_decorators";
import { JSBehaviourBase } from "./js_behaviour_base";

@ScriptType()
export class KingHumanController extends JSBehaviourBase {
    @ScriptObject({editorOnly: true})
    animator: Animator;

    @ScriptNumber()
    moveSpeed = 1.8;

    private moving = false;
    private spriteRenderer: SpriteRenderer;

    Awake() {
        // this.transform.localScale = new Vector3(1, 1, 1);
        // this.transform.localPosition = new Vector3(1.0, 2.2, 0);
    }

    OnAfterDeserialize() {
        // 发生脚本重载时不会触发 Awake 所以在此处赋值
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

            // if (Input.GetKey(KeyCode.W)) {
            //     vert = 1;
            // } else if (Input.GetKey(KeyCode.S)) {
            //     vert = -1;
            // }

            if (hori != 0 || vert != 0) {
                let scale = Time.deltaTime * this.moveSpeed;
                this.transform.Translate(hori * scale, vert * scale, 0);
                if (hori != 0) {
                    this.spriteRenderer.flipX = hori < 0;
                }
                if (!this.moving) {
                    this.moving = true;
                    this.animator.Play("Run", 0);
                    // console.log("go1");
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

