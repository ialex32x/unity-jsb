"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.RotateBehaviour = exports.MySubClass = exports.MyClass = void 0;
const UnityEngine_1 = require("UnityEngine");
const jsb = require("jsb");
const inspector_1 = require("./editor/decorators/inspector");
let MyClass = class MyClass extends UnityEngine_1.MonoBehaviour {
    constructor() {
        super(...arguments);
        this.vv = 0;
        this._tick = 0;
    }
    Awake() {
        console.log("MyClass.Awake", this._tick++);
    }
    async OnEnable() {
        console.log("MyClass.OnEnable", this._tick++);
        await jsb.Yield(new UnityEngine_1.WaitForSeconds(1));
        console.log("MyClass.OnEnable (delayed)", this._tick++);
    }
    OnDisable() {
        console.log("MyClass.OnDisable", this._tick++);
    }
    OnDestroy() {
        console.log("MyClass.OnDestroy", this._tick++);
    }
    speak(text) {
        console.log(text);
    }
    async test() {
        console.log("MyClass.test (will be destroied after 5 secs.", this.transform);
        await jsb.Yield(new UnityEngine_1.WaitForSeconds(5));
        UnityEngine_1.Object.Destroy(this.gameObject);
    }
};
MyClass = __decorate([
    inspector_1.Inspector("editor/inspector/my_class_inspector", "MyClassInspector")
], MyClass);
exports.MyClass = MyClass;
class MySubClass extends MyClass {
    Awake() {
        super.Awake();
        console.log("MySubClass.Awake", this._tick++);
    }
    Update() {
        if (UnityEngine_1.Input.GetMouseButtonUp(0)) {
            let ray = UnityEngine_1.Camera.main.ScreenPointToRay(UnityEngine_1.Input.mousePosition);
            let hitInfo = { type: UnityEngine_1.RaycastHit };
            let layerMask = 1 << UnityEngine_1.LayerMask.NameToLayer("Default");
            console.log("hittest", layerMask);
            if (UnityEngine_1.Physics.Raycast(ray, hitInfo, 1000, layerMask)) {
                console.log("hit", hitInfo.value.transform.name);
            }
        }
    }
    play() {
        console.log("MySubClass.play");
    }
}
exports.MySubClass = MySubClass;
let RotateBehaviour = class RotateBehaviour extends UnityEngine_1.MonoBehaviour {
    constructor() {
        super(...arguments);
        this._rotation = 0;
        this.rotationSpeed = 50;
    }
    Reset() {
        this._rotation = 0;
    }
    Awake() {
        let ps = this.GetComponent(UnityEngine_1.ParticleSystem);
        if (ps) {
            ps.main.simulationSpace = UnityEngine_1.ParticleSystemSimulationSpace.World;
            console.log("ps.main.simulationSpace:", ps.main.simulationSpace);
        }
    }
    Update() {
        this._rotation += this.rotationSpeed * UnityEngine_1.Time.deltaTime;
        //@ts-ignore
        let p = UnityEngine_1.Quaternion.Euler(0, this._rotation, 0) * UnityEngine_1.Vector3.right * 5;
        p.z *= 0.5;
        this.transform.localPosition = p;
    }
};
RotateBehaviour = __decorate([
    inspector_1.Inspector("editor/inspector/rotate_inspector", "RotateBehaviourInspector")
], RotateBehaviour);
exports.RotateBehaviour = RotateBehaviour;
if (module == require.main) {
    print("example_monobehaviour");
    let gameObject = new UnityEngine_1.GameObject();
    let comp1 = gameObject.AddComponent(MySubClass);
    let comp2 = gameObject.AddComponent(MyClass);
    let cube = UnityEngine_1.GameObject.CreatePrimitive(UnityEngine_1.PrimitiveType.Cube);
    cube.transform.localPosition = new UnityEngine_1.Vector3(1, 2, 3);
    cube.transform.localRotation = UnityEngine_1.Quaternion.Euler(30, 60, 90);
    cube.transform.localScale = new UnityEngine_1.Vector3(2, 3, 4);
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
    let ps = UnityEngine_1.GameObject.Find("/Particle System");
    if (!ps) {
        ps = new UnityEngine_1.GameObject("Particle System");
        ps.AddComponent(UnityEngine_1.ParticleSystem);
    }
    console.log("Particle System:", ps);
    if (ps) {
        ps.AddComponent(RotateBehaviour);
    }
}
//# sourceMappingURL=example_monobehaviour.js.map