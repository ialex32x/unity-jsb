"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const UnityEngine_1 = require("UnityEngine");
let path = "prefab/game_stage";
let prefab = UnityEngine_1.Resources.Load(path);
if (prefab) {
    UnityEngine_1.Object.Instantiate(prefab);
}
else {
    console.error("game stage not found:", path);
}
//# sourceMappingURL=game_demo.js.map