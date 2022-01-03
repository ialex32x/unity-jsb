debugger;
import { GameObject, PrimitiveType, Vector3, Quaternion, ParticleSystem } from "UnityEngine";
import { MySubClass, MyClass, RotateBehaviour, Something5Behaviour } from "./components/sample_monobehaviour";

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
