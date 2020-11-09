
import { FileState, FileWatcher } from "./file_watcher";

if (typeof globalThis["__fw"] !== "undefined") {
    globalThis["__fw"].dispose();
    delete globalThis["__fw"];
}

let fw = new FileWatcher("Scripts", "*.js");

fw.on("*", this, function (filestate: FileState) {
    for (let moduleId in require.cache) {
        let module = require.cache[moduleId];

        if (module.filename == filestate.fullPath) {
            // console.warn("change", module.filename);
            delete require.cache[moduleId];
            return;
        }
    }
    // console.warn("file-change", filestate.fullPath);
})

globalThis["__fw"] = fw;

["OnPostprocessTexture", "OnPostprocessModel", "OnPostprocessAudio", "OnPostprocessMaterial", "OnPostprocessAllAssets"].forEach(k => {
    globalThis[k] = function () {
        const p = require("./asset_importer")[k];
        if (p) {
            try {
                p(...arguments);
            } catch (e) {
                console.error(e);
            }
        }
    }
});
