"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const file_watcher_1 = require("./file_watcher");
if (typeof globalThis["__fw"] !== "undefined") {
    globalThis["__fw"].dispose();
    delete globalThis["__fw"];
}
let fw = new file_watcher_1.FileWatcher("Scripts", "*.js");
fw.on("*", this, function (filestate) {
    for (let moduleId in require.cache) {
        let module = require.cache[moduleId];
        if (module.filename == filestate.fullPath) {
            // console.warn("change", module.filename);
            delete require.cache[moduleId];
            return;
        }
    }
    // console.warn("file-change", filestate.fullPath);
});
globalThis["__fw"] = fw;
["OnPostprocessTexture", "OnPostprocessModel", "OnPostprocessAudio", "OnPostprocessMaterial", "OnPostprocessAllAssets"].forEach(k => {
    globalThis[k] = function () {
        const p = require("./asset_importer")[k];
        if (p) {
            try {
                p(...arguments);
            }
            catch (e) {
                console.error(e);
            }
        }
    };
});
//# sourceMappingURL=main.js.map