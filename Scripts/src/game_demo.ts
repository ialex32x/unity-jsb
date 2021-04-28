import { Object, Resources } from "UnityEngine";

let prefab = Resources.Load("sprites/King Human");

if (prefab) {
    Object.Instantiate(prefab);
} else {
    console.error("game prefab not found");
}

