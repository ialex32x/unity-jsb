"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.EdCache = void 0;
const UnityEngine_1 = require("UnityEngine");
class EdCache {
    static T(title, tooltip = null, image = null) {
        let item = this.cache[title];
        if (typeof item === "undefined") {
            item = this.cache[title] = tooltip == null ? new UnityEngine_1.GUIContent(title, image) : new UnityEngine_1.GUIContent(title, image, tooltip);
        }
        return item;
    }
}
exports.EdCache = EdCache;
EdCache.cache = {};
//# sourceMappingURL=content_cache.js.map