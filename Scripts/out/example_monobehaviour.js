"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
debugger;
const UnityEngine_1 = require("UnityEngine");
const sample_monobehaviour_1 = require("./components/sample_monobehaviour");
if (module == require.main) {
    print("example_monobehaviour");
    let gameObject = new UnityEngine_1.GameObject();
    let comp1 = gameObject.AddComponent(sample_monobehaviour_1.MySubClass);
    let comp2 = gameObject.AddComponent(sample_monobehaviour_1.MyClass);
    let cube = UnityEngine_1.GameObject.CreatePrimitive(UnityEngine_1.PrimitiveType.Cube);
    cube.transform.localPosition = new UnityEngine_1.Vector3(1, 2, 3);
    cube.transform.localRotation = UnityEngine_1.Quaternion.Euler(30, 60, 90);
    cube.transform.localScale = new UnityEngine_1.Vector3(2, 3, 4);
    comp1.vv = 1;
    comp2.vv = 2;
    comp1.play();
    {
        let results = gameObject.GetComponents(sample_monobehaviour_1.MySubClass);
        results.forEach(it => console.log("GetComponents(MySubClass):", it.vv));
    }
    {
        let results = gameObject.GetComponents(sample_monobehaviour_1.MyClass);
        results.forEach(it => console.log("GetComponents(MyClass):", it.vv));
    }
    let ps = UnityEngine_1.GameObject.Find("/Particle System");
    if (!ps) {
        ps = new UnityEngine_1.GameObject("Particle System");
        ps.AddComponent(UnityEngine_1.ParticleSystem);
    }
    console.log("Particle System:", ps);
    if (ps) {
        ps.AddComponent(sample_monobehaviour_1.RotateBehaviour);
    }
    {
        let sphere = UnityEngine_1.GameObject.CreatePrimitive(UnityEngine_1.PrimitiveType.Sphere);
        sphere.transform.localScale = new UnityEngine_1.Vector3(2, 2, 2);
        sphere.transform.TestWithScriptObject(function () {
            console.warn("TestWithScriptObject");
        });
        sphere.AddComponent(sample_monobehaviour_1.Something5Behaviour);
    }
}
//# sourceMappingURL=example_monobehaviour.js.map