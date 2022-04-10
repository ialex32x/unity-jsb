"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const matter_js_1 = require("matter-js");
const UnityEngine_1 = require("UnityEngine");
let engine = matter_js_1.Engine.create({
    gravity: { y: -0.1 }
});
let runner = matter_js_1.Runner.create();
class BoxUpdater extends UnityEngine_1.MonoBehaviour {
    Update() {
        this.transform.localRotation = UnityEngine_1.Quaternion.Euler(0, 0, this.body.angle * 180 / Math.PI);
        this.transform.localPosition = new UnityEngine_1.Vector3(this.body.position.x, this.body.position.y, 0);
    }
}
let index = 0;
function add_box(x, y, w, h) {
    let box = matter_js_1.Bodies.rectangle(x, y, w, h);
    let go = UnityEngine_1.GameObject.CreatePrimitive(UnityEngine_1.PrimitiveType.Cube);
    let updater = go.AddComponent(BoxUpdater);
    updater.body = matter_js_1.Composite.add(engine.world, box).bodies[index++];
    go.transform.localScale = new UnityEngine_1.Vector3(w, h, 1);
}
function add_static_box(x, y, w, h) {
    let box = matter_js_1.Bodies.rectangle(x, y, w, h, { isStatic: true });
    let go = UnityEngine_1.GameObject.CreatePrimitive(UnityEngine_1.PrimitiveType.Cube);
    let v = matter_js_1.Composite.add(engine.world, box).bodies[index++].position;
    go.transform.localPosition = new UnityEngine_1.Vector3(v.x, v.y, 0);
    go.transform.localScale = new UnityEngine_1.Vector3(w, h, 1);
}
add_box(4, 100, 8, 8);
add_box(6, 70, 8, 8);
add_static_box(0, -100, 400, 6);
matter_js_1.Runner.run(runner, engine);
UnityEngine_1.Camera.main.orthographicSize = 100;
//# sourceMappingURL=example_matterjs.js.map