
import { FileState, FileWatcher } from "./file_watcher";

console.log("hello, editor");

setTimeout(function () {
    console.log("hello, editor after 3 secs");
}, 3000);

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
