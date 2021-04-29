import { GameObject, Object, Resources } from "UnityEngine";

if (!GameObject.Find("/game_stage")) {
    let path = "prefab/game_stage";
    let prefab = Resources.Load(path);

    if (prefab) {
        let inst = Object.Instantiate(prefab);
        inst.name = "game_stage";
    } else {
        console.error("game stage not found:", path);
    }
}
