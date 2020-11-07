
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
            delete require.cache[moduleId];
            return;
        }
    }
})

globalThis["__fw"] = fw;
