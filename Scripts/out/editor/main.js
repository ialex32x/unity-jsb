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
            delete require.cache[moduleId];
            return;
        }
    }
});
globalThis["__fw"] = fw;
//# sourceMappingURL=main.js.map