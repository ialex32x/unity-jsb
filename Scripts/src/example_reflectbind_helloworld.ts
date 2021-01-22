import { DelegateTest } from "Example";
import { Debug, GameObject, MonoBehaviour, Vector3 } from "UnityEngine";

console.log("hello, world");
Debug.Log(<any>"hello, world");

let go = new GameObject("Happy Bot");

console.log("go.transform = ", go.transform);
console.log("go.transform.localPosition.x = ", go.transform.localPosition.x);
console.log("new Vector3(1, 1, 1).x = ", new Vector3(1, 1, 1).x);

go.transform.localPosition = new Vector3(1, 1, 1);

DelegateTest.onStaticActionWithArgs("set", (a1, a2, a3) => console.log("delegate in js:", a1, a2, a3));
DelegateTest.CallStaticActionWithArgs("hello", 123, 999);

// class HelloBehaviour extends MonoBehaviour {
// }

// go.AddComponent(HelloBehaviour);

