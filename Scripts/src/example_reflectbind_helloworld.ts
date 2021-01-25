import { DelegateTest } from "Example";
import { BoxCollider, Debug, GameObject, MonoBehaviour, Vector3 } from "UnityEngine";

console.log("hello, world");
Debug.Log(<any>"hello, world");

let go = new GameObject("Happy Bot");

console.log("go.transform = ", go.transform);
console.log("go.transform.localPosition.x = ", go.transform.localPosition.x);

let v = new Vector3(1, 1, 1);
v.x = 2;
console.log("v.x = ", v.x);
console.log("v.magnitude = ", v.magnitude);

go.transform.localPosition = new Vector3(1, 1, 1);

DelegateTest.onStaticActionWithArgs("set", (a1, a2, a3) => console.log("delegate in js:", a1, a2, a3));
DelegateTest.CallStaticActionWithArgs("hello", 123, 999);

let delegateTest = new DelegateTest();
delegateTest.onActionWithArgs("add", (a1, a2, a3) => console.log("delegate in js1:", a1, a2, a3));
delegateTest.onActionWithArgs("add", (a1, a2, a3) => console.log("delegate in js2:", a1, a2, a3));
delegateTest.CallActionWithArgs("hello", 123, 999);

class HelloBehaviour extends MonoBehaviour {
    Awake() {
        console.log("Hello Behaviour Awake!");
    }

    Greet() {
        console.log("Good day!");
    }
}

go.AddComponent(HelloBehaviour);
let helloBehaviour = go.GetComponent(HelloBehaviour);
helloBehaviour.Greet();

console.log(go.AddComponent);
let box = go.AddComponent(BoxCollider);
console.log(box);
