import { MonoBehaviour, WaitForSeconds, Object, Input, Camera, GameObject, PrimitiveType, Vector3, Quaternion, Physics, LayerMask, RaycastHit, Transform, Time, ParticleSystem, ParticleSystemSimulationSpace } from "UnityEngine";
import * as jsb from "jsb";
import { Out } from "jsb";
import { Button } from "UnityEngine.UI";
import { ScriptType, ScriptProperty, ScriptFunction, ScriptNumber, ScriptInteger } from "./plover/runtime/class_decorators";

@ScriptType()
export class ASimpleGuiDialog extends MonoBehaviour {
    //TODO: [NOT_IMPLEMENTED] 自动绑定界面组件
    @ScriptProperty({ type: "object", bind: { widget: Button } })
    nameLabel: Button;

    //TODO: [NOT_IMPLEMENTED] 使 UnityEvent 识别并接受脚本函数作为回调
    @ScriptFunction()
    onClicked() {
        console.log("you clicked the button");
    }
}

@ScriptType()
export class MyClass extends MonoBehaviour {

    @ScriptNumber()
    vv = 0;

    @ScriptInteger({ editable: false, serializable: false })
    protected _tick = 0;

    Awake() {
        console.log("MyClass.Awake", this._tick++);
    }

    async OnEnable() {
        console.log("MyClass.OnEnable", this._tick++);
        await jsb.Yield(new WaitForSeconds(1));
        console.log("MyClass.OnEnable (delayed)", this._tick++);
    }

    OnDisable() {
        console.log("MyClass.OnDisable", this._tick++);
    }

    OnDestroy() {
        console.log("MyClass.OnDestroy", this._tick++);
    }

    OnApplicationFocus(hasFocus: boolean) {
        console.log("OnApplicationFocus:", hasFocus);
    }

    OnApplicationPause(pause: boolean) {
        console.log("OnApplicationPause:", pause);
    }

    speak(text: string) {
        console.log("modified", text);
    }

    async test() {
        console.log("MyClass.test (will be destroied after 5 secs.", this.transform);
        await jsb.Yield(new WaitForSeconds(5));
        Object.Destroy(this.gameObject);
    }
}

@ScriptType()
export class MySubClass extends MyClass {
    Awake() {
        super.Awake();
        console.log("MySubClass.Awake", this._tick++);
    }

    Update() {
        if (Input.GetMouseButtonUp(0)) {
            let ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            let hitInfo: Out<RaycastHit> = { type: RaycastHit };
            let layerMask = 1 << LayerMask.NameToLayer("Default");

            console.log("hittest", layerMask);
            if (Physics.Raycast(ray, hitInfo, 1000, layerMask)) {
                console.log("hit", hitInfo.value.transform.name);
            }
        }
    }

    play() {
        console.log("MySubClass.play");
    }
}

@ScriptType()
export class RotateBehaviour extends MonoBehaviour {
    private _rotation: number = 0;

    @ScriptNumber({ serializable: false })
    rotationSpeed = 50;

    Reset() {
        this._rotation = 0;
    }

    Awake() {
        let ps = this.GetComponent(ParticleSystem);
        if (ps) {
            ps.main.simulationSpace = ParticleSystemSimulationSpace.World;
            console.log("ps.main.simulationSpace:", ps.main.simulationSpace);
        }
    }

    Update() {
        this._rotation += this.rotationSpeed * Time.deltaTime;
        //@ts-ignore
        let p: Vector3 = Quaternion.Euler(0, this._rotation, 0) * Vector3.right * 5;
        p.z *= 0.5;
        this.transform.localPosition = p;
    }
}

@ScriptType()
export class Something5Behaviour extends MonoBehaviour {
    
    @ScriptProperty({ type: "Vector3" })
    pos: Vector3;

}

if (module == require.main) {
    print("example_monobehaviour");
    let gameObject = new GameObject();
    let comp1 = gameObject.AddComponent(MySubClass);
    let comp2 = gameObject.AddComponent(MyClass);

    let cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

    cube.transform.localPosition = new Vector3(1, 2, 3);
    cube.transform.localRotation = Quaternion.Euler(30, 60, 90);
    cube.transform.localScale = new Vector3(2, 3, 4);

    comp1.vv = 1;
    comp2.vv = 2;

    comp1.play();

    {
        let results = gameObject.GetComponents(MySubClass);
        results.forEach(it => console.log("GetComponents(MySubClass):", it.vv));
    }

    {
        let results = gameObject.GetComponents(MyClass);
        results.forEach(it => console.log("GetComponents(MyClass):", it.vv));
    }

    let ps = GameObject.Find("/Particle System");
    if (!ps) {
        ps = new GameObject("Particle System");
        ps.AddComponent(ParticleSystem);
    }
    console.log("Particle System:", ps);
    if (ps) {
        ps.AddComponent(RotateBehaviour);
    }

    {
        let sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = new Vector3(2, 2, 2);
        sphere.transform.TestWithScriptObject(function () {
            console.warn("TestWithScriptObject");
        });

        sphere.AddComponent(Something5Behaviour);
    }
}
