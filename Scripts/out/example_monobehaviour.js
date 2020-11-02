"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.MySubClass = exports.MyClass = void 0;
const UnityEngine_1 = require("UnityEngine");
const jsb = require("jsb");
class MyClass extends UnityEngine_1.MonoBehaviour {
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
    Update() {
        if (UnityEngine_1.Input.GetMouseButtonUp(0)) {
            let ray = UnityEngine_1.Camera.main.ScreenPointToRay(UnityEngine_1.Input.mousePosition);
            let point = ray.origin;
            console.log(point.x, point.y, point.z);
        }
    }
    async test() {
        console.log("MyClass.test (will be destroied after 5 secs.", this.transform);
        await jsb.Yield(new UnityEngine_1.WaitForSeconds(5));
        UnityEngine_1.Object.Destroy(this.gameObject);
    }
}
exports.MyClass = MyClass;
class MySubClass extends MyClass {
    Awake() {
        super.Awake();
        console.log("MySubClass.Awake", this._tick++);
    }
    play() {
        console.log("MySubClass.play");
    }
}
exports.MySubClass = MySubClass;
if (module == require.main) {
    print("example_monobehaviour");
    let gameObject = new UnityEngine_1.GameObject();
    let comp1 = gameObject.AddComponent(MySubClass);
    let comp2 = gameObject.AddComponent(MyClass);
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
}
//# sourceMappingURL=example_monobehaviour.js.map