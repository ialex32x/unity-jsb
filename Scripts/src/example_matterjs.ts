import { Bodies, Body, Composite, Engine, Runner } from "matter-js";
import { Camera, GameObject, MonoBehaviour, PrimitiveType, Quaternion, Random, Vector3 } from "UnityEngine";

let engine = Engine.create({
    gravity: { y: -0.1 }
});

let runner = Runner.create();

class BoxUpdater extends MonoBehaviour {
    body: Body;

    Update() {
        this.transform.localRotation = Quaternion.Euler(0, 0, this.body.angle * 180 / Math.PI);
        this.transform.localPosition = new Vector3(this.body.position.x, this.body.position.y, 0);
    }
}

let index = 0;
function add_box(x: number, y:number, w: number, h: number) {
    let box = Bodies.rectangle(x, y, w, h);
    let go = GameObject.CreatePrimitive(PrimitiveType.Cube);
    let updater = go.AddComponent(BoxUpdater);
    updater.body = Composite.add(engine.world, box).bodies[index++];
    go.transform.localScale = new Vector3(w, h, 1);
}

function add_static_box(x: number, y:number, w: number, h: number) {
    let box = Bodies.rectangle(x, y, w, h, { isStatic: true });
    let go = GameObject.CreatePrimitive(PrimitiveType.Cube);
    let v = Composite.add(engine.world, box).bodies[index++].position;
    go.transform.localPosition = new Vector3(v.x, v.y, 0);
    go.transform.localScale = new Vector3(w, h, 1);
}

add_box(4, 100, 8, 8);
add_box(6, 70, 8, 8);
add_static_box(0, -100, 400, 6);

Runner.run(runner, engine);
Camera.main.orthographicSize = 100;