"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const UnityEngine_1 = require("UnityEngine");
let prefab = UnityEngine_1.Resources.Load("sprites/King Human");
if (prefab) {
    UnityEngine_1.Object.Instantiate(prefab);
}
else {
    console.error("game prefab not found");
}
//# sourceMappingURL=game_demo.js.map