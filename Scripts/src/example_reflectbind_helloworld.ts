import { Debug, GameObject, MonoBehaviour, Vector3 } from "UnityEngine";

console.log("hello, world");
Debug.Log(<any>"hello, world");

let go = new GameObject("Happy Bot");

console.log("go.transform = ", go.transform);
console.log("go.transform.localPosition.x = ", go.transform.localPosition.x);
console.log("new Vector3(1, 1, 1).x = ", new Vector3(1, 1, 1).x);

go.transform.localPosition = new Vector3(1, 1, 1);

class HelloBehaviour extends MonoBehaviour {

}

go.AddComponent(HelloBehaviour);

