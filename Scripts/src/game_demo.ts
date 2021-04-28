import { Object, Resources } from "UnityEngine";

let path = "prefab/game_stage";
let prefab = Resources.Load(path);

if (prefab) {
    Object.Instantiate(prefab);
} else {
    console.error("game stage not found:", path);
}

